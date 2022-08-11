# Release Notes

## 8.0.0 - 2022-08-11
- Removed useAxiosInstanceTransformer function for customization axios requests
- Added timeout param for axios upload requests
- Updated dependencies versions

## 7.2.0 - 2021-11-15
- Added useAxiosInstanceTransformer function for customization axios requests

## 7.1.3 - 2021-10-04
- Set axios default put header to application/json

## 7.1.2 - 2021-08-10
- Set axios default post header to application/json

## 7.1.1 - 2021-08-09
- Fix getUri function of axios transport

## 7.1.0 - 2021-08-09
- Allow to import multiple ITypeDescriptorProvider implementations

## 7.0.0 - 2021-06-30
- Add a bunch of options for generating nullable/optional properties, even for reference types.

## 6.0.1 - 2021-05-23

- Fix enum resource generation

## 6.0.0 - 2019-04-24

* Support generating clients for ASP.NET Core 3.1 projects
* Drop support for non-Core ASP.NET Web API projects
* Support custom IApiDiscovery implemenation in a plugin
* Support TSEnumLocalization and TSExtendEnum attributes on enum types as well as on assembly
* Enum value localization - renamed additional contexts to additional sets

## 5.2.0 - 2019-10-03

* Command-line option for generating string enums
* Allow using TSClientGen without having a reference to TSClientGen.Contract in webapi project
* Process multiple TSRequireDescendantTypes attributes applied to a base type
* Added TSIgnoreAttribute for excluding type properties, api methods and api method parameters from code generation
* Builtin transport modules for axios, jQuery, SuperAgent and Fetch API
* Simplified support for aborting xhr requests (got rid of axios specifics in the feature implementation)

## 5.1.0 - 2019-09-16

* Implemented an extensibility point to add some custom code to generated API clients