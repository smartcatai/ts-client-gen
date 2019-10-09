# TSClientGen (aka ts-client-gen)
TSClientGen is a tool to generate TypeScript client for api exposed by .net web application. It is easy to use and highly extensible at the same time. When using most of its features it can be seen more like a strongly-typed bridge connecting your TypeScript client-side and .NET server-side codebases.

You can run this tool by hand and then place the generated TypeScript modules into your client-side codebase or you can build the execution of this tool into your client-side build pipeline so that you won't even need to store generated modules in source control. It's up to you to decide how deeply you would like to integrate the api client module code generation into your development process.

## Table of contents

* [Basic usage](#basic-usage)
* [Command-line parameters reference](#command-line-parameters-reference)
* [Basic features](#basic-features)
  + [Enums](#enums)
  + [Inheritance in models](#inheritance-in-models)
  + [Swapping client-side http request library](#swapping-client-side-http-request-library)
  + [Providing custom transport module to handle http requests](#providing-custom-transport-module-to-handle-http-requests)
  + [Aborting in-flight http requests](#aborting-in-flight-http-requests)
  + [Cleaning up output folder contents](#cleaning-up-output-folder-contents)
* [Customizing with attributes](#customizing-with-attributes)
  + [Specify module name for api client module](#specify-module-name-for-api-client-module)
  + [Replace generated type definitions](#replace-generated-type-definitions)
  + [Include additional types to generated code](#include-additional-types-to-generated-code)
    - [Handle model inheritance hierarchies](#handle-model-inheritance-hierarchies)
  + [Get url for a given server api method and params at runtime in browser](#get-url-for-a-given-server-api-method-and-params-at-runtime-in-browser)
  + [Issue requests to an external host instead of a page host](#issue-requests-to-an-external-host-instead-of-a-page-host)
  + [Exclude specific api controllers, methods, parameters or type properties from code generation](#exclude-specific-api-controllers-methods-parameters-or-type-properties-from-code-generation)
  + [Upload files with multipart form data requests](#upload-files-with-multipart-form-data-requests)
  + [Extend generated enums with static fields or functions](#extend-generated-enums-with-static-fields-or-functions)
  + [Generate and expose arbitrary static data structures to the client-side codebase](#generate-and-expose-arbitrary-static-data-structures-to-the-client-side-codebase)
* [Customizing with a plugin](#customizing-with-a-plugin)
  + [Alter api methods descriptions before code generation](#alter-api-methods-descriptions-before-code-generation)
  + [Alter interface descriptions before code generation](#alter-interface-descriptions-before-code-generation)
  + [Add custom code to generated api client modules](#add-custom-code-to-generated-api-client-modules)
  + [Expose server-side resources to client-side codebase](#expose-server-side-resources-to-client-side-codebase)
  + [Expose server-side enum value localizations to client-side codebase](#expose-server-side-enum-value-localizations-to-client-side-codebase)
  + [Provide a custom mechanism for discovering api from .net assembly](#provide-a-custom-mechanism-for-discovering-api-from-net-assembly)

## Basic usage
Install the [nuget package](https://www.nuget.org/packages/TSClientGen/) and call TSClientGen.exe with these command line parameters:
```shell
TSClientGen.exe --asm MyWebApi.dll --out-dir .\output --transport axios --cleanup-out-dir
```
The parameters here are the following
* `--asm` - space-delimited list of assemblies containing web api controllers;
* `--out-dir` - output directory for generated TypeScript code;
* `--transport` - client-side http request library to use (`axios`, `fetch`, `jquery` and `superagent` options available out of the box, or you can provide your own custom module for handling requests);
* `--cleanup-out-dir` - instructs TSClientGen to cleanup all directory contents when writing new generated files.

Your output directory will contain several files - one module per each of your api controllers plus special modules `transport-contracts.ts` and `transport-axios.ts`. Transport module serves as proxy between generated api client modules and a specific http request library and is imported in all generated api client modules. Transport contracts module contains interfaces that are implemented by a specific transport module and have to be implemented in your custom transport module if you provide one instead of one of the builtin transport modules. See more details on transport modules in the section on [swapping client-side http request library](#swapping-client-side-http-request-library).

Given the following api controller:
```csharp
[RoutePrefix("simple")]
public class SimpleController : ApiController
{
	[HttpGet, Route("name")]
	public Response Get(Request request)
	{
		return new Response
		{
			Items = Enumerable.Repeat("Item", request.ItemsCount).ToArray()
		};
	}
}

public class Request
{
	public int ItemsCount { get; set; }
}

public class Response
{
	public string[] Items { get; set; }
}  
```
your will get the following `simple.ts` api client module:
```typescript
import { request } from './transport-axios';
import { HttpRequestOptions } from './transport-contracts';

export class SimpleClient {
	public get(requestParam: Request, { getAbortFunc }: HttpRequestOptions = {}) {
		const method = 'get';
		const url = `/simple/name`;
		const queryStringParams = { request: requestParam };
		return request<Response>({ url, method, queryStringParams, getAbortFunc });
	}	
}

export default new SimpleClient();

export interface Request {
	itemsCount: number;
}

export interface Response {
	items: string[];
}
```
By default TSClientGen uses controller name with the `Controller` suffix removed as the name for generated api client module. Each client module contains a class with methods for each of the server-side api methods. This class is exported by name and its instance is also exported as default export. There is also a bunch of interfaces exported by name that are generated for the server-side model types referenced by api method signatures. 

## Command-line parameters reference
* `--asm <assemblies>` (or `-a <assemblies>`) - specifies an assembly or a list of .net assemblies with the api controllers to generate api client modules for. Specify several assemblies using a space delimiter. This parameter is required.

* `--out-dir <folder>` (or `-o <folder>`) - specifies an output folder for the code generation results.

* `--cleanup-out-dir` - instructs TSClientGen to clean up all the files from output folder that were not created or updated as the result of the code generation. See [Cleaning up output folder contents](#Cleaning-up-output-folder-contents) for details.

* `--append-i-prefix` - instructs TSClientGen to append `I` prefix to all the generated interface definitions (e.g. `IRequest` instead of `Request`). This is handy in case you want to follow a widespread C# naming conventions for interfaces in your client-side api client modules.

* `--enum-module <modulename>` - specifies name for the generated module containing all enums. See [Enums](#Enums) for more details on how TSClientGen handles enums.

* `--string-enums` - instucts TSClientGen to generate string enums instead of default number-based ones. See [TypeScript enums](https://www.typescriptlang.org/docs/handbook/enums.html) for more details on enums in TypeScript and [Enums](#Enums) for more details on how TSClientGen handles enums.

* `--transport <axios|jquery|fetch|superagent>` - specifies a client-side http request library to use. You can specify either `--transport` or `--custom-transport` parameter but not both at the same time.

* `--custom-transport <modulename>` - specifies a path to custom transport module for performing http requests to server, serving as a replacement for one of the builtin transport modules. Allows for [swapping client-side http request library](#Swapping-client-side-http-request-library).
  Please note that you should specify module path relative to the output folder of the code generation, because this path will be imported in generated client modules.

* `--get-resource-module <modulename>` - specifies a path to custom TypeScript module responsible for retrieving localized strings from client-side localization resources. This module should export a function named `getResource` that is used for enum value localizations. You have to provide this command-line parameter if you use enum value localization feature (have instances of TSEnumLocalization attributes in your api assemblies). See [Expose server-side enum value localizations to client-side codebase](#Expose-server-side-enum-value-localizations-to-client-side-codebase) for more detailed description of this command line parameter and feature.
  Please note that you should specify module path relative to the output folder of the code generation, because this path will be imported in generated enums module.

* `--loc-lang <languages>` - specifies a comma-separated list of supported localization cultures for application. These cultures will be passed down to `CultureInfo.GetCultureInfo` method, so they must represend valid .net culture names.  Provide this parameter if you use some of the localization features of the TypeScript code generation. Please note that in order to generate client-side resources you also have to provide a plugin assembly with the implementation of `IResourceModuleWriterFactory` interface in it. See [Expose server-side resources to client-side-codebase](#Expose-server-side-resources-to-client-side-codebase) and [Expose server-side enum value localizations to client-side-codebase](#Expose-server-side-enum-value-localizations-to-client-side-codebase) sections for more details.

* `--plugins-assembly <assemblypath>` (or `-p <assemblypath>`) - specifies a plugin assembly for customizing and extending the code generation process. This assembly should contain a bunch of classes implementing interfaces from `TSClientGen.Extensibility.dll` and marked with [MEF](https://docs.microsoft.com/en-us/dotnet/framework/mef/) `Export` attribute. See the section about [customizing TSClientGen with plugins](#Customize-with-a-plugin) for more details on the topic.

## Basic features

### Enums

TypeScript supports enums, so naturally some set of enums is usually shared between server-side and client-side codebases. TSClientGen supports generating TypeScript enums from .net ones. All the enums from api controller parameters, request and response models are collected and written to a separate enums module which is by default named `enums.ts`.
```
out
│ transport-axios.ts
│ transport-contracts.ts
│ enums.ts
│ myapi.ts
```
The reason behind having all enums in a separate module is that you could have several api controllers using the same enum, so generating this enum type twice in different api client modules would result in having two separate incompatible enums in TypeScript code. This would be inconvenient for the client-side development because a single server-side enum should correspond to a single TypeScript enum regardless of how many api client modules reference this enum.
TSClientGen always generates enums with explicit enum values and uses number enums by default. You can make it produce string enums with the `--string-enums` command-line parameter.
Sometimes you may want to share some enums between server-side and client-side code even if they aren't referenced in any api methods or models. This is easy to achieve in TSClientGen by decorating one of the api controllers (for enums it does not matter which one) with the `TSRequireType` attribute specifying the types of .net enums to append to client-side enums module. See [Include additional types to generated code](#Include-additional-types-to-generated-code) for more details.

### Inheritance in models
TSClientGen reconstructs server-side inheritance hierarchies in generated TypeScript interfaces. If you have a server-side class for api model that inherits from another class, you will get two interfaces in TypeScript code with one inheriting the other.
You can instruct TSClientGen to generate descendant classes for the base class in TypeScript code even if your api method signatures do not reference them directly. See [Handle model inheritance hierarchies](#Handle-model-inheritance-hierarchies) for more details.

### Swapping client-side http request library
TSClientGen code generation result includes a special transport module which imports a specific client-side library responsible for communicating with server. You can use one of the builtin transport modules for the most popular libraries or provide a custom implementation of the transport module. The following libraries are supported out of the box:
* `--transport axios` - [Axios](https://github.com/axios/axios)
* `--transport jquery` - [jQuery](http://api.jquery.com/jquery.ajax/). This transport module does not import a jQuery dependency, it assumes that symbol `$` is available in the global scope.
* `--transport fetch` - raw [fetch](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API) api;
* `--transport superagent` - [SuperAgent](https://visionmedia.github.io/superagent/).

Not all libraries have equal feature sets. For example you won't be able to track upload progress when using jQuery or cancel in-flight http requests when using anything but axios.

### Providing custom transport module to handle http requests
You can provide your own transport module in case you want to use some other http request library or you want to modify the logic of builtin transport module in some way. To do this skip the `--transport` command-line parameter and specify `--custom-transport` parameter instead with a path to your transport module as a value. Please note that you should specify the path to your transport module relative to the code generation output folder. TSClientGen does not emit any builtin transport module module when `--custom-transport` parameter is specified. Your custom transport module is imported into the generated api client modules and used for performing requests to the server instead of a builtin transport. You cannot specify both `--transport` and `--custom-transport` parameters at the same time.
A custom transport module has to export two functions with the following signatures:

```typescript
import { GetUriOptions, RequestOptions } from './transport-contracts';

export function request<TResponse>(config: RequestOptions): Promise<TResponse> {
...
}

export function getUri(config: GetUriOptions): string {
...
}
```
[`transport-contracts`](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Core/transport-contracts.ts) is a special module with a bunch of interfaces defining contracts for any custom or builtin transport module. Autogenerated api client modules rely on these contracts to make use of a transport module. This module always appears as the code generation result in the output folder. You should make yourself acquainted with the contents of this module before writing a custom transport module.

Instances of `RequestOptions` and `GetUriOptions` are created and filled by the generated api client modules and passed down to `request` and `getUri` methods of the transport module. They contain all the information about the http request to be made. You'll need to deal with these interfaces only in case of writing a custom transport module.

`HttpRequestOptions `, `UploadFileHttpRequestOptions ` and `NamedBlob ` interfaces are on the contrary under some conditions exposed to the consumers of generated api client modules. `HttpRequestOptions ` interface allows the calling code to [abort in-flight http requests](#aborting-in-flight-http-requests). `NamedBlob` interface is used to provide arbitrary data for [multipart form data requests](#upload-files-with-multipart-form-data-requests), and `UploadFileHttpRequestOptions ` allows for client-side tracking of such requests.

### Aborting in-flight http requests

The last parameter of each generated api client method is optional and contains an instance of  [`HttpRequestOptions`](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Core/transport-contracts.ts) interface:

```typescript
export interface HttpRequestOptions {
	getAbortFunc?: (abort: () => void) => void
}
```

You can provide a value for this parameter to be able to abort http request like this:

```typescript
import api from 'server-api/ExternalApi';

let abortRequest: () => void;
api.postData(..., { getAbortFunc: (abort) => { abortRequest = abort; } });
```

You should store the function provided in parameter to `getAbortFunc` and call it whenever you want to abort the in-progress request. Note that not every http request library supports aborting in-flight requests. Among the builtin transport modules only axios supports this feature,  other transport modules will throw a runtime error if you specify `getAbortFunc` in the last parameter of a method call.

### Cleaning up output folder contents

 `--cleanup-out-dir` command-line option instructs TSClientGen to clean up all the files from output folder that were not created or updated as a result of the code generation. This is implemented in a smart way in order not to conflict with the webpack running simultaneously in watch mode. TSClientGen does not remove any files from the output folder before starting code generation. Instead, it rewrites existing file contents and keeps track of the set of affected files while doing its job. After all the code generation is completed TSClientGen will look at the output folder contents and find files that are present there but were not created or overwritten in the process of code generation. If there are any such files and this command-line option was specified, TSClientGen will remove this files.

Most of the time you will want to use this command-line option. This will be the case when you have a dedicated folder for generated api client modules and you generate all of your api client modules by a single run of TSClientGen. It's easier to keep track of what files have been generated by keeping them in a separate folder, so it's a recommended approach. You wouldn't want to accidentally introduce some manual changes to autogenerated files and loose these changes on the next TSClientGen tool run.

There are however some use cases for not using this command-line option. One of them is keeping your generated api client modules in a common folder along with other files (which is not recommended). The other is running TSClientGen tool several times for getting a complete set of api client modules. You may want to do that if you need to split your generated enum module into several modules or to use different client-side transport libraries for different parts of your api. You can't do that with a single run of TSClientGen tool, but you can achieve these goals by running the tool multiple times specifying the same value for `--out-dir` param and different values for `--enum-module` and\or `--transport` or `--custom-transport` params. In this case you will have to omit the `--cleanup-out-dir` option so that subsequent runs of the tool do not clear the results of the previous runs.

## Customizing with attributes

You can further affect the result of the code generation process by adding a reference to [TSClientGen.Contract nuget package](https://www.nuget.org/packages/TSClientGen.Contract) to your web api project and decorating some api controllers, model types and properties with the attributes from this assembly.

### Specify module name for api client module
By default TSClientGen uses controller name with the `Controller` suffix removed as the name for generated api client module. [TSModule](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSModuleAttribute.cs) attribute applied to asp.net webapi controller allows you to override this behavior and explicitly specify name for a generated api client module. Note that this attribute affects only the api client module name, not the name of the exported class.

***Important*** - once you decorate one api controller with the `TSModule` attribute, you have to do the same with all the controllers you want api client modules to be generated for. This feature can be used to filter api controllers you want to be the sources for code generation.

Note that [TSIgnore](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSIgnoreAttribute.cs) attribute also excludes api controllers from code generation when applied to api controller class. If you want to exclude only a few controllers and you do not need to customize module names for others, then `TSIgnore` attribute is an easier way to do so.

### Replace generated type definitions
You may want to completely opt out of the default processing for some .net type and instead treat it as a different type while generating TypeScript code. You can even write a TypeScript type definition for some of .net types by hand. This is handy for non-primitive types that yet have to be treated as primitive in TypeScript codebase. An example may be the `System.Uri` (represented as string in TypeScript) or some custom entity identifier types that represents some primitive type and are around only for the purpose of improving type safety and code expressiveness. [TSSubstituteType](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSSubstituteTypeAttribute.cs) attribute allows for such use cases.
`TSSubstituteType` attribute can be applied in two different ways:

* with a substitute .NET type - `TSSubstitute(typeof(string))`
In this case all the occurences of the source type will be treated by the code generator as occurences of the substitute type and will be handled respectively. You can specify a primitive type (mapped to TypeScript builtin type) or a complex type (mapped to generated TypeScript interface) as a substitute type - there are no restrictions on neither source type nor substitute type except that source type can't represent enum.
* with a handwritten TypeScript type definition - `TSSubstitute("string | number", true)`
In this case you should also specify whether this type will be represented by the type alias exported by name or will be inlined in all places where the original .net type is referenced in api method signatures or model class properties. This is specified by the attribute constructor's second parameter with the default of defining and exporting a separate type alias.

`TSSubstituteType` can be applied to either original type itself or to a property in a class. When applied to an original type (say, your custom entity identifier type), it affects all the occurences of this type in all api method signatures (parameters and return values) and api model properties. You can also apply this attribute to a property of your api model if you want it to affect only this specific property instead of all occurences of some type. When applied with a handwritten TypeScript definition to a property, the attribute does not allow you to choose between inlining the definition or creating an exported type alias - a type definition is always inlined because a single property on some type should not affect property type's definition outside of this parent type.

### Include additional types to generated code
#### Handle model inheritance hierarchies
There is a common case for inheritance in api models when you have some abstract response model class with a bunch of descendant classes  and your backend actually returns an instance of one of these specific descendants in response for the request. The reverse can be also the case - your frontend sends a request and your backend api method expects an instance of one of the several specific classes having a common ancestor. In both cases the api method signature contains only a base class for request or response. Specific descendant classes are not referenced anywhere in api method signature, but you need all of them in your client-side code to either generate a request or process the response.

TSClientGen allows you to generate all descenant types for some base type by decorating this base type with [TSRequireDescendantTypes](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSRequireDescendantTypesAttribute.cs) attribute. By default TSClientGen scans the assembly of this base type while searching for descendant classes, but you can specify another assembly to scan with the `IncludeDescendantsFromAssembly` attribute's property. You can have multiple `TSRequireDescendantTypes` attributes applied to a base type in case its descenants are scattered across more than one assembly.

Sometimes it may not be feasible to alter base class definition by decorating it with an attribute - maybe you do not want or aren't able to add a reference to `TSClientGen.Contract` assembly to the assembly defining the base type. In this case you can use another option for exposing a full hierarchy of classes to your TypeScript codebase. You can decorate an api controller with [TSRequireType](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSRequireTypeAttribute.cs) attribute for the base type and specify `IncludeDescendantsFromAssembly` property for this attribute. Please note that in this case the default behavior for not specifying the property is different from `TSRequireDescendantTypes` attribute's behavior. If you do not specify `IncludeDescendantsFromAssembly` property for `TSRequireType` attribute, then only the type specified in `TSRequireType` will be output to TypeScript and no assemblies will be scanned for its descendant types.

### Get url for a given server api method at runtime in browser

There is a common need to build url for an api method given all parameter values for url parts and query string. For example, an api method may generate some file based on parameters provided and you may need to generate a link to download the file and navigate to that link or place it in on a html anchor element. TSClientGen enables this use case with the [TSGenerateUrl](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSGenerateUrlAttribute.cs) attribute. Applying it to an api method like this:
```csharp
[HttpGet, Route("{reportName}/download")]
[TSGenerateUrl]
public void DownloadFile(string reportName, DateTime date)
{
}
```
produces the following method in generated api client class along with the regular `downloadFile` method:
```typescript
public downloadFileUrl(reportName: string, date: Date) {
	const url = `/simple/${reportName}/download`;
	const queryStringParams = { date: date.toISOString() };
	return getUri({ url, queryStringParams });
}
```

Generated method for building url saves you a burden of synchronizing url template and parameters list between server-side and client-side codebases. It uses `getUri` method from the imported transport module. That means that each transport can implement its own url building logic and you have to implement `getUri` with url building logic in your custom transport module implementation if you have one. For any builtin transport module `request` and `getUri` methods use the same url building implementation and the same should be true for any custom transport module.

The reason for relying on transport module for url generation is that some http client libraries - jQuery and axios - take care of building a query string from parameters object. For such transport modules it makes perfect sense to reuse their query string building facilities instead of overriding them with some TSClientGen hardcoded implementation. For those transport modules whose underlying libraries do not provide the ability to build query string from parameters object (fetch api and superagent), the builtin modules provide fairly simple hardcoded implementations. You can consult query string building implementation in [fetch API transport module](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Core/transport-fetch.ts) if you need one for your custom transport module.

### Issue requests to an external host instead of a page host

By default generated api clients will issue server requests without specifying a hostname. If you need to explicitly specify the protocol, hostname or port, you should enable this feature on selected api parts in your server codebase. This is done via applying the [TSSupportsExternalHost](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSSupportsExternalHostAttribute.cs) attribute to api controllers. Applying this attribute to api controller results in adding the `hostname` parameter to the constructor of the generated api client class. In order to be able to specify a hostname, yo will need to import api client class from generated module and instantiate it with the hostname parameter. All server requests made by this instance of api client class will then be issued to a specified hostname:
```typescript
import { ExternalApiClient } from 'server-api/ExternalApi';

const api = new ExternalApiClient('https://api-on-another-host.com');
api.postData(...);
```

### Exclude specific api controllers, methods, parameters or type properties from code generation

You can mark any api controller, method, api method parameter or a property of type that is being used in api with the [TSIgnore](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSIgnoreAttribute.cs) attribute to completely exclude it from code generation. The tool will completely omit this controller, method, parameter or property from TypeScript code generation.

### Upload files with multipart form data requests

Uploading files to server is a common use case, and TSClientGen supports doing it via generated api clients in a strongly-typed fashion. Apply [TSUploadFiles](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSUploadFilesAttribute.cs) attribute to an api method to tell TSClientGen that this method expects multipart form data request, which is commonly used to upload arbitrary files to server.

On applying this attribute api client generated method receives an additional parameter `files: Array<NamedBlob | File>`. `File` here is a standard [File](https://developer.mozilla.org/en-US/docs/Web/API/File) object. `NamedBlob` is a simple wrapper around the standard [Blob](https://developer.mozilla.org/en-US/docs/Web/API/Blob) defined in [transport contracts module](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Core/transport-contracts.ts). This wrapper allows you not only to get files from file input but also to generate them from any arbitrary data in your client-side code. If your api method has parameter that is marked with [FromBody](https://docs.microsoft.com/en-us/previous-versions/aspnet/hh834191(v%3Dvs.118)) attribute, then the generated api client will serialize this parameter value to JSON and wrap it in a separate part of multipart form data request with a content type of "application/json".

Please note that TSClientGen makes no assumptions about how your server-side code extracts uploaded files from the http request. The same is true for the api parameter marked with `FromBody` attribute and placed into a separate `application/json` part of request. You extract request files in server-side code by dealing directly with `HttpRequestMessage` or by mapping the files to some method parameter(s) by the means of a custom model mapper. In the latter case be aware that TSClientGen will process these special file parameters as usual and generate parameters in api client class for them, which is probably not what you want. You may want to mark such parameters with [TSIgnore](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSIgnoreAttribute.cs) attribute to omit them from the generated api client method signature.

You can also monitor the progress of uploading process and report it to the user. The last parameter of each generated api client method is optional and contains an instance of type [`HttpRequestOptions`](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Core/transport-contracts.ts):
```typescript
export interface HttpRequestOptions {
	getAbortFunc?: (abort: () => void) => void
}
```
If your api server method is marked with the `TSUploadFiles` attribute then the type of this last parameter will be `UploadFileHttpRequestOptions`:
```typescript
export interface UploadFileHttpRequestOptions extends HttpRequestOptions {
	onUploadProgress?: (progressEvent: ProgressEvent) => void
}
```
The callback provided via the `onUploadProgress` property will be called during the upload process with a [ProgressEvent](https://developer.mozilla.org/en-US/docs/Web/API/ProgressEvent) instance as parameter. Note that not every transport library supports reporting the upload process progress - as for builtin transports, only jQuery and axios provide this feature, others will throw runtime error if you try to provide `onUploadProgress` callback when invoking api method.

### Extend generated enums with static fields or functions

Enums in TypeScripts can be extended with static members as described [here](https://basarat.gitbooks.io/typescript/docs/enums.html#enum-with-static-functions). You can append static functions or fields to TypeScript enum definitions generated by TSClientGen by applying a custom attribute inheriting from [TSExtendEnumAttribute](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSExtendEnumAttribute.cs) to the server-side enum definition.

[TSExtendEnumAttribute](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSExtendEnumAttribute.cs) is an abstract attribute class that defines one method named `GenerateStaticMembers`. It's only parameter is a `StringBuilder` for writing the TypeScript code to be emitted to the `namespace` block for the enum. You do not have to apply`AttributeUsage` attribute to your attribute class because it is already specified on the base class.

Sometimes it may not be feasible to alter enum type definition by decorating it with an attribute - maybe you do not want or aren't able to add a reference to TSClientGen.Contract assembly to the assembly defining the enum type. Then you can inherit your attribute from the `TSClientGen.ForAssembly.TSExtendEnumAttribute` class and apply it to an assembly instead. This base class defines the very same `GenerateStaticMembers` abstract method, and it also has a `EnumType` property and constructor parameter that points to the target enum type. You also do not have to specify `AttributeUsage` on the attribute class when inheriting from `ForAssembly.TSExtendEnumAttribute` class. You can apply this attribute to any of the assemblies that serve as an input for TSClientGen tool - the choice of specific assembly doesn't matter because all enums are generated to the common enums module. 

### Generate and expose arbitrary static data structures to the client-side codebase

Sometimes you may want to expose some static data to the client-side codebase beyound the set of enums and localization resources. For example, you may want to make a list of languages with some of the `CultureInfo` properties accessible to the client-side code. Or you may have an application-wide collection of objects describing possible user roles and permissions in your system. You could just maintain a duplicate of these data structures in TypeScript code, but of course it's better to have a single source of truth for such data and therefore it's better to generate this data structures for the client-side code automatically.

TSClientGen supports it with the [TSStaticContentAttribute](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSStaticContentAttribute.cs). This is an abstract base class to derive from and to apply to an assembly. It doesn't matter which assembly will you apply the attribute to as long as this assembly is passed to TSClientGen. By deriving from this attribute you can generate any objects and emit them to the dedicated TypeScript module in JSON form. You should create an attribute deriving from `TSStaticContentAttribute` and override the getters of the two properties:
* Name - specifies name of TypeScript module that the content will be emitted to. If name matches one of the names of the api client modules, then the content will be appended to that module. If no api client module has the same name, then a new separate TypeScript module will be generated in output folder to hold the content.
* Content - the objects to be serialized to JSON and emitted to the module. This is a dictionary with strings as keys and objects as values. The generated content is emitted as named module exports. The dictionary key is a name of the exported variable and the value is the object to be serialized to JSON and emitted as a value for this exported variable.

You can't emit any arbitrary TypeScript code using the `TSStaticContentAttribute`. It allows you to emit only objects (arrays or primitive types also count) that can be serialized to JSON. The structure of the generated TypeScript is also restricted to a set of named exports of variables.

## Customizing with a plugin

Plugins provide additional ways to customize and extend the code generation process of TSClientGen tool that aren't available with just decorating the codebase with the attributes from `TSClientGen.Contracts` assembly. A TSClientGen plugin is an assembly that should contain one or more classes that implement interfaces defined in [TSClientGen.Extensibility](https://www.nuget.org/packages/TSClientGen.Extensibility) assembly. Your provide a plugin assembly to the TSClientGen tool by specifying a path to the assembly via `--plugins-assembly` (or `-p`) command-line parameter option. Plugins make use of [MEF](https://docs.microsoft.com/en-us/dotnet/framework/mef/), so you should decorate your interface implementations within a plugin assembly with the [Export](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.composition.exportattribute) attribute. The customization facilities provided by plugin support are described in detail in the sections below.

### Alter api methods descriptions before code generation

Implementing the [IMethodDescriptorProvider](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/IMethodDescriptorProvider.cs) interface allows you to intervene in the code generation process to alter api method desciptors after they have been discovered from your .net assemblies but before they have been written to the generated TypeScript code. The interface's only method takes three parameters:
* api controller .net type;
* [`MethodInfo`](https://docs.microsoft.com/en-US/dotnet/api/system.reflection.methodinfo) instance for the api method obtained via reflection;
* [`ApiMethod`](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/ApiDescriptors/ApiMethod.cs) TSClientGen descriptor that has been constructed during the api discovery process and will serve as an input for the code generation process.

You should return the `ApiMethod` instance that will serve as an input for the code generation process instead of the original one provided as a third argument. The `ApiMethod` class is immutable, so you have to create a new instance of it if you want to modify it in any way. You can also return the original `ApiMethod` instance if you do not want to make any changes to this method descriptor.

You can customize every aspect of the api method that is described by the `ApiMethod` class - method name, url template, http method, parameters list (including their names, types, order and whether a parameter is required or not). This customization feature may be useful for hiding some of the api method parameters in case you do not want them to be available in generated api client methods for some reason. There is a simpler way to do this with [TSIgnore attribute](#exclude-specific-api-controllers-methods-parameters-or-type-properties-from-code-generation) but you may choose a plugin way if for example you have a lot of parameters of the same type to hide throughout all the server-side codebase. Another use case for implementing the `IMethodDescriptorProvider` interface is on the contrary, to add some parameters to generated api client methods. These could be some service parameters handled by server-side http modules and thus non-present in api method signatures directly. Such parameters are be appended to the query string if you don't explicitly mark them with `IsBodyContent = true`.

### Alter interface descriptions before code generation

You can alter complex type descriptions before they are passed down to code generation for TypeScript interfaces generation in a way similar to api methods customization. This is done via implementing the [ITypeDescriptorProvider](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/ITypeDescriptorProvider.cs) interface in a plugin assembly. The only method of the interface takes three parameters:
* .net type that is being mapped to a TypeScript interface;
* [`TypeDescriptor`](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/ApiDescriptors/TypeDescriptor.cs) TSClientGen descriptor that has been constructed during the api discovery process and will serve as an input for the code generation process.;
* `Func<TypePropertyDescriptor, PropertyInfo>` function that allows your interface implementation to obtain a [`PropertyInfo`](https://docs.microsoft.com/en-US/dotnet/api/system.reflection.propertyinfo) instance for one of the properties of the mapped .net type. Properties in `TypeDescriptor` class are represented by the list of [`TypePropertyDescriptor`](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/ApiDescriptors/TypePropertyDescriptor.cs) instances. You may use the third method parameter to get `PropertyInfo` instance for some of the properties of the type if `TypePropertyDescriptor` does not contain all the data you need. For example, you'll need to get `PropertyInfo` object to retrieve attributes applied to a property.

You should return the `TypeDescriptor` instance that will serve as an input for the code generation process instead of the original one provided as a second argument. The `TypeDescriptor` class is immutable, so you have to create a new instance of it if you want to modify it in some way. You can also return the original `TypeDescriptor` instance if you do not want to make any changes to this type descriptor.

By implementing the `ITypeDescriptorProvider` interface you can add, remove or modify the list of type's properties before passing the type to code generation. You can also replace a base type for the type in question for the purposes of code generation. But note that you can alter only properties that are directly contained in the type described by `TypeDescriptor`. You can't in any way modify the TypeScript representation for the nested objects contained in the properties of this type.

### Add custom code to generated api client modules

TSClientGen allows you to add any custom TypeScript code to the generated api client modules. This may be used for extending api client classes with some additional functionality or for instance for registering api client instances in some global registry (like a DI container). You can extend api client modules by implementing the [IApiClientWriter](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/IApiClientWriter.cs) interface in the plugin assembly.

The [IApiClientWriter](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/IApiClientWriter.cs) interface contains several methods, each of which accepts two parameters of the types [IIndentedStringBuilder](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/IIndentedStringBuilder.cs) and [ApiClientModule](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/ApiDescriptors/ApiClientModule.cs). The first parameter is used for writing TypeScript code into the generated module and is a thin wrapper around `StringBuilder` that takes care of the indentations. The second one is the descriptor providing all the info about the api client module being generated. The methods of the interface are the following:	

- `WriteImports` - appends code right after all the built-in imports of the api client module. If you import some additional modules into api client module, they probably should go here;
- `WriteCodeBeforeApiClientClass`- appends code right before the api client class declaration. This is effectively the same point as for `WriteImports`, but after the empty line separating import statements from the api client class declaration.
- `WriteCodeAfterApiClientClass` - appends code to the very end of the module (after the api client class definition and default export).
- `ExtendApiClientConstructor`- appends code to the api client class constructor (by default is has an empty body). Use this to add some custom initialization for the api client class.
- `ExtendApiClientClass` - appends code to the end of the api client class definition, after all methods have been defined. Use this to add custom members to the api client class.

### Expose server-side resources to client-side codebase

Maybe your application has some server-side resource files with localized strings that you would like to reuse in the client-side codebase. This could be for instance some localized entity or enum names that should be the same in your web application and in emails sent out by the server. TSClientGen can generate client-side localization resource files from .net resources. It relies upon [ResourceManager](https://docs.microsoft.com/en-US/dotnet/api/system.resources.resourcemanager) BCL class to retrieve localized strings from assembly resources. This feature also works best with strongly-typed classes for resource files generated by [*Resgen.exe*](https://docs.microsoft.com/en-US/dotnet/framework/tools/resgen-exe-resource-file-generator) tool and contained in *.Designer.cs* files usually accompanying *.resx* files in Visual Studio projects.

Let's assume you have a *Messages.resx* file containing string resources in the server-side .net project, and you have a generated class for this resource file contained in *Messages.designer.cs* file. First, apply the [TSExposeResx](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSExposeResxAttribute.cs) attribute to any assembly that you feed into TSClientGen, and specify `typeof(Messages)` (the type of the class contained in *Messages.designer.cs*) as a single parameter for the attribute's constructor:
```csharp
[assembly: TSExposeResx(typeof(Messages))]
```
If you don't use generated classes in *Designer.cs* files or even if you use something other that resx files for storing server-side string resources - that is not a problem. As long as your string resources are retrievable via `ResourceManager` in runtime - TSClientGen can handle them. Just create a dummy class with a single static property `ResourceManager` of type `ResourceManager` and feed it to the `TSExposeResx` attribute's constructor.

So far we have told TSClientGen what server-side string resources we want to have a copy of on the client side. But this is only one part of the equation, and the other is the form of resource files in client-side codebase. Modern web frontend ecosystem is quite versatile and there are plenty of ways for storing localized strings in JavaScript or TypeScript code. JSON and PO files are often used, however their structure varies and they are not the only options. Webpack loader plugins allow you to use any format. TSClientGen is not opinionated about the format of your client-side localization files. It provides you with the [IResourceModuleWriterFactory](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/IResourceModuleWriterFactory.cs) interface that you have to implement in plugin assembly in order to generate client-side resource files. The implementations of [IResourceModuleWriterFactory](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/IResourceModuleWriterFactory.cs) and accompanying [IResourceModuleWriter](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/IResourceModuleWriter.cs) interfaces take care of creating a resource file with the appropriate extension and filling it with localized string values.

### Expose server-side enum value localizations to client-side codebase

Localized enum values are a common case for reusing string resources between server-side and client-side codebases. TSClientGen has a special support for this scenario - the [TSEnumLocalization](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSEnumLocalizationAttribute.cs) attribute. This attribute results in a static method `localize` appended to the generated TypeScript enum definition. The `localize` method takes enum value as parameter and returns its localized name.

This attribute comes in two flavors - `TSClientGen.TSEnumLocalizationAttribute` is for applying to enum type and `TSClientGen.ForAssembly.TSEnumLocalizationAttribute` is for applying to an assembly. The second one is intended for the case when you for some reason can't or don't want to apply an attribute to the target enum type directly. The usage and effect of both attributes are the same, you only have to provide an additional parameter to the constructor of the assembly-intended attribute - the target enum type.

Let's have a look at the attribute's constructor parameters:
* `resxType` - is the only required parameter. It is a .net type that has a `ResourceManager` property of type `System.Resources.ResourceManager`. This is typically a code-behind class generated by Visual Studio for any _.resx_ file. The same approach is used for exposing arbitrary string resources by the [TSExposeResx](#Expose-server-side-resources-to-client-side-codebase) attribute.
* `usePrefix` - has the boolean type and the default value of false. This parameter determines the format of keys in server-side resource file. When true, TSClientGen will expect resource key for the enum value to be enum type name (without namespace) and value name separated by underscore, like `WeekDay_Monday`. When false, TSClientGen will expect the resource key to contain just value name - `Friday`.
  Please note that this parameter deals with the server-side resource files only and does not affect the resource keys generated by the TSClientGen in client-side resource files - they are always written with the enum name prefix, like `WeekDay_Monday`.
* `additionalSets` - this is a string array that supports the case of having several localized value sets for a single enum type. For example, an additional set for a `Weekday` enum could be a set of string resources containing first letters of weekday names.
  TSClientGen expects the server-side resource file  to contain an additional set of localized enum values for each of the additional set names. These resource keys should contain the set name in addition to enum value and (if `usePrefix` is true) to the name of the enum type. So, given the additional set name is "short", the resource keys for this set should look like `short_Monday` (if `usePrefix` is false) or `WeekDay_short_Monday` (if `usePrefix` is true). TSClientGen then writes additional resource sets to the generated client-side resource file.
  The client-side generated `getResource` method also gets an additional optional parameter denoting the set name to get localized enum value for. The localized short name for Monday can then be retrieved in TypeScript code like this: `WeekDay.localize(WeekDay.Monday, 'short')`.

TSClientGen has no assumptions about the format of your client-side resource files, so you'll have to tell it how to create client-side resource files as well as how to retrieve strings from them at runtime if you use [TSEnumLocalization](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Contract/TSEnumLocalizationAttribute.cs) attribute. [IResourceModuleWriterFactory](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/IResourceModuleWriterFactory.cs) and the accompanying  [IResourceModuleWriter](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/IResourceModuleWriter.cs) interfaces are responsible for generating resource files, and you should provide a plugin with their implementation in order to use `TSEnumLocalization` attribute in your api assemblies.

You also have to pass the `get-resource-module` command-line parameter to the TSClientGen tool. This parameter should contain a path to your TypeScript module responsible for retrieving localized strings from client-side localization resources. This module should export a function named `getResource` that takes a resource name as its only parameter and returns a localized string value. The `getResource` static method of TypeScript enum type makes use of this method to retrieve localized enum values from the resource file. Please note that you should specify module path relative to the output folder of the code generation, because this path will be imported in generated enums module.

### Provide a custom mechanism for discovering api from .net assembly

If altering api methods definitions via [`IMethodDescriptorProvider`](#Alter-api-methods-descriptions-before-code-generation) is not enough for you, the plugin system offers one even more powerful option. You can completely replace the api discovery mechanism that processes assemblies and tells TSClientGen what api client modules should be generated and what methods should they contain.

This can be done by implementing the [`IApiDiscovery`](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/IApiDiscovery.cs) interface in your TSClientGen plugin. The interface is pretty simple and contains only one method that is named `GetModules`. It takes an assembly as a parameter and should return a collection of [`ApiClientModule`](https://github.com/smartcatai/ts-client-gen/blob/develop/TSClientGen.Extensibility/ApiDescriptors/ApiClientModule.cs) instances. An instance of `ApiClientModule` class completely describes one api client module with all its methods.

Note that you don't necessarily have to use existing classes as a source of api modules. You can hardcode api modules list in your implementation, read it from some xml or json file or take it wherefrom you like. One of the constructors of `ApiClientModule` class takes api controller .net type as a parameter, but it uses it only for getting [`TSRequireType`](#Include-additional-types-to-generated-code), [`TSIgnore`](#exclude-specific-api-controllers-methods-parameters-or-type-properties-from-code-generation) and [`TSSupportsExternalHost`](#Issue-requests-to-an-external-host-instead-of-a-page-host) custom attributes. You can use another constructor that doesn't require controller type as a parameter whenever you need to.
