# Release Notes

### 1.8
* Signing assembly GetText assemblies with strong name signing key [Issue #47](https://github.com/perpetualKid/GetText.NET/issues/47)
* Upgrading all projects from .NET Core 3.1 to .Net 6.0, removing EOL .NET Core 3.1 [Issue #46](https://github.com/perpetualKid/GetText.NET/issues/46)

### 1.7
* Fixing an issue with nested string interpolation and GetString* calls [Bug #44](https://github.com/perpetualKid/GetText.NET/issues/44)
* Improving nested string handling and resolving of GetString* calls 
* adding test cases for Extractor both to enable regression testing and to help clarifying supported Extractor/GetString* cases. 
See [Extractor documentation](https://github.com/perpetualKid/GetText.NET/wiki/Extractor%20Intro) for details

### 1.5
* Customizable aliases (such as shorthand _(), _p(), _n(), _pn()) added by [Owlblocks](https://github.com/Owlblocks) in [PR #39](https://github.com/perpetualKid/GetText.NET/pull/39)

### 1.3
* Checking argument count as simple way to exclude non-GetText `GetString*` calls in Extractor [PR #37](https://github.com/perpetualKid/GetText.NET/pull/37)
* Include Razor-based source files files (*.razor, *.cshtml) for Extractor ([see Discussion](https://github.com/perpetualKid/GetText.NET/discussions/35))

### 1.1
* Fix Unhandled ArgumentException when `--order` parameter not passed[Bug #33](https://github.com/perpetualKid/GetText.NET/issues/33)

### 0.12
* Adding CatalogManager to deal with multiple catalogs in a project, ie. separate catalogs per C# project (libraries, executables)
* Extending sample to show working with multiple catalogs (one per domain/assembly)
* Adding Enum Description examples

### 0.11
* implementing Nerdbank.GitVersioning to simplify Versioning workflow
