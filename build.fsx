#load ".fake/build.fsx/intellisense.fsx"

open Fake.Api
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

let slnFile = "./TSClientGen.sln"
let nugetOutput = "./nuget"
let gitOwner = "smartcatai"
let githubRepoName = "ts-client-gen"
let gitHubToken = Fake.IO.File.readAsString "github_token"
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
        | Some v -> v.Trim '"'
        | None ->
            match AssemblyInfoFile.getAttributeValue "AssemblyVersion" aiFile with
            | Some v -> v.Trim '"'
            | None -> failwith ("Can't find assembly version attribute in " + (Path.toRelativeFromCurrent aiFile))

    let invalidVersions =
        !! "./**/AssemblyInfo.cs"
        |> Seq.filter (fun aiFile -> SemVer.parse(getAssemblyVersion aiFile).Normalize() <> release.SemVer.Normalize())
        |> List.ofSeq

    for aiFile in invalidVersions do
        Trace.traceErrorfn
            "Assembly version in %s is not equal to the version in RELEASE_NOTES.md"
            (Path.toRelativeFromCurrent aiFile)

    if not invalidVersions.IsEmpty then
        failwith "Invalid versions for some assemblies"

    Trace.trace "All assembly versions are equals to release version"
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

Target.create "Pack" (fun _ ->
    Paket.pack (fun opt -> { opt with
                                OutputPath = nugetOutput;
                                BuildConfig = buildConfig.ToString();
                                Version = release.SemVer.AsString })
)

Target.create "GithubRelease" (fun _ ->

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