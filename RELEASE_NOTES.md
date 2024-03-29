# Release Notes

## 9.2.6 - 2024-01-12
- Add headers in transport-contracts.ts

## 9.2.5 - 2023-08-09
- Add net 7.0 to FWTs

## 9.2.4 - 2023-03-02
- Fix generating bug when type without public fields

## 9.2.3 - 2023-03-01
- Support custom types in query parameters
- Fix incorrect `/` symbols in url

## 9.2.1 - 2022-12-12
- Make getAbortFunc in transport-contracts.ts optional.

## 9.2.0 - 2022-11-25
- Add function for getting localization key
- Allow to disable generating common enum module

## 9.1.0 - 2022-11-16
- Allow to specify enum imports for TSExtendEnumAttribute inheritors

## 9.0.1 - 2022-10-31
- Add net 5.0 to FWTs

## 9.0.0 - 2022-10-27
- Generate enums and their localization in separate files

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
