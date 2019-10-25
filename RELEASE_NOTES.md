# Release Notes

## 5.3.0 - 2019-10-27

- Tool for .net core
- Support custom IApiDiscovery implemenation in a plugin
- Support TSEnumLocalization and TSExtendEnum attributes on enum types as well as on assembly
- Enum value localization - renamed additional contexts to additional sets

## 5.2.0 - 2019-10-03

* Command-line option for generating string enums
* Allow using TSClientGen without having a reference to TSClientGen.Contract in webapi project
* Process multiple TSRequireDescendantTypes attributes applied to a base type
* Added TSIgnoreAttribute for excluding type properties, api methods and api method parameters from code generation
* Builtin transport modules for axios, jQuery, SuperAgent and Fetch API
* Simplified support for aborting xhr requests (got rid of axios specifics in the feature implementation)

## 5.1.0 - 2019-09-16

* Implemented an extensibility point to add some custom code to generated API clients