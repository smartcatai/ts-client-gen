#load ".fake/build.fsx/intellisense.fsx"

open Fake.Api
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open System.Xml

let slnFile = "./TSClientGen.sln"
let netCoreToolPrjFile = "./TSClientGen/TSClientGen.csproj"
let nugetOutput = "./nuget"
let gitOwner = "smartcatai"
let githubRepoName = "ts-client-gen"
let release = Fake.Core.ReleaseNotes.load "RELEASE_NOTES.md"
let buildConfig = DotNet.BuildConfiguration.Release

Target.create "Clean" (fun _ ->
    !! "./**/bin"
    ++ "./**/obj"
    ++ nugetOutput
    |> Shell.cleanDirs 
)

Target.create "Restore" (fun _ ->
    DotNet.restore id slnFile
)

Target.create "CheckVersions" (fun _ ->
    let getAssemblyVersion aiFile =
        match AssemblyInfoFile.getAttributeValue "AssemblyInformationalVersion" aiFile with
        | Some v -> v.Trim '"' |> Some
        | None ->
            match AssemblyInfoFile.getAttributeValue "AssemblyVersion" aiFile with
            | Some v -> v.Trim '"' |> Some
            | None -> Path.toRelativeFromCurrent aiFile
                        |> sprintf "Can't find assembly version attribute in %s"
                        |> failwith

    let getPackageVersion csprojFile =
        let doc = XmlDocument()
        let versions = 
            doc.Load (filename = csprojFile);
            doc.SelectNodes "//PackageVersion/text()"
                |> Seq.cast<XmlNode>
                |> Seq.map (fun node -> node.Value)
                |> Seq.toList
        match versions with
            | [version] -> Some version
            | [_;_] -> Path.toRelativeFromCurrent csprojFile
                        |> sprintf "More than one PackageVersion property found in %s"
                        |> failwith
            | _ -> None

    let getInvalidVersions pattern parseVersion =
        let hasInvalidVersion file =
            match parseVersion file with
               | Some version -> SemVer.parse(version).Normalize() <> release.SemVer.Normalize()
               | None -> false
        !! pattern |> Seq.filter hasInvalidVersion |> List.ofSeq

    let invalidAssemblyInfoVersions = getInvalidVersions "./**/AssemblyInfo.cs" getAssemblyVersion
    let invalidCsprojVersions = getInvalidVersions "./**/*.csproj" getPackageVersion
    let invalidVersions = List.append invalidAssemblyInfoVersions invalidCsprojVersions

    for file in invalidVersions do
        Path.toRelativeFromCurrent file
            |> Trace.traceErrorfn "Assembly version in %s is not equal to the version in RELEASE_NOTES.md"

    if not invalidVersions.IsEmpty then
        failwith "Invalid versions for some assemblies"

    Trace.trace "All assembly versions are equal to the release version"
)

Target.create "Build" (fun _ ->
    let buildOptionsSetter opt : DotNet.BuildOptions =
        { opt with
            Configuration = buildConfig;
            Common = { DotNet.Options.Create() with Verbosity = Some DotNet.Verbosity.Minimal };
            NoRestore = true }

    DotNet.build buildOptionsSetter slnFile
)

Target.create "Test" (fun _ ->
    DotNet.test (fun opt -> { opt with
                                NoBuild = true;
                                Configuration = buildConfig }) slnFile
)

Target.create "Pack" (fun p ->
    let argVersion =
        match p.Context.Arguments with
        | ["--version"; v] -> Some v
        | _ -> None

    Paket.pack (fun opt -> { opt with
                                ToolType = ToolType.CreateLocalTool();
                                OutputPath = nugetOutput;
                                BuildConfig = string buildConfig;
                                Version = argVersion |> Option.defaultValue release.SemVer.AsString })
    
    DotNet.pack (fun opt -> { opt with
                                    NoBuild = true;
                                    NoRestore = true;
                                    OutputPath = Some nugetOutput; }) netCoreToolPrjFile
)

Target.create "GithubRelease" (fun _ ->

    let gitHubToken = Fake.IO.File.readAsString "github_token"
    let files = !! (nugetOutput + "/*.nupkg")

    GitHub.createClientWithToken gitHubToken
    |> GitHub.draftNewRelease gitOwner githubRepoName release.SemVer.AsString false release.Notes
    |> GitHub.uploadFiles files
    |> Async.RunSynchronously
    |> ignore
)

Target.create "All" ignore

"Clean"
  ==> "Restore"
  ==> "Build"
  ==> "Test"
  ==> "CheckVersions"
  ==> "Pack"
  ==> "GithubRelease"
  ==> "All"

Target.runOrDefaultWithArguments "Test"