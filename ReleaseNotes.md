# Release Notes

### 11.x (upcoming changes)

The current development branch is versioned `11.1-dev`. The following changes belong to the 11.x release line; this list includes the pending WPF fix and does not imply that every change is already published.

* Add `GetText.NET.Wpf` with logical/visual-tree localization, original-text storage and reversion, the `Gettext` XAML markup extension (including context and plural support), and `CatalogConverter` for bound strings.
* Add XAML extraction for literal text attributes, authored `TextBlock`, `Run` and `AccessText` text, and `Gettext` markup extensions, including context and plural message IDs.
* Fix WPF localization and reversion invalidating realized text-document enumerators. Preserve access-key inlines, authored inline formatting, bindings and template-owned values; translate generated menu and content text only through its owner. Literal template values now remain template-managed; use a localization binding or markup extension for them.
* Fix concurrent extractor entry updates and remove duplicate source references in generated templates ([#59](https://github.com/perpetualKid/GetText.NET/pull/59)).
* Preserve WPF ComboBox selection and editable input during localization and reversion. Skip selection/input-owned `ComboBox.Text` and its `PART_EditableTextBox`, including text previously captured in an original-text store, while continuing to localize surrounding labels and tooltips. This prevents language-picker preference feedback caused by restoring a previous selection's text.
* Correct built-in plural rules for Arabic (modulo-100 ranges), Icelandic (numbers ending in 1 except those ending in 11), and Tachelhit (0 and 1 share the first form) ([#60](https://github.com/perpetualKid/GetText.NET/pull/60)).
* Move the extractor tool to .NET 10 and Windows Forms integration to `net10.0-windows`, retaining its .NET Framework 4.6.1 target. WPF integration targets .NET Framework 4.6.1 and .NET 10 for Windows; the core library remains .NET Standard 2.0.
* Update Roslyn, System.CommandLine and build/test dependencies, and migrate the extractor to the newer command-line API.
* Add WPF and Windows Forms samples sharing translation catalogs. Include the wiki as a Git submodule and migrate the repository solution to `GetText.NET.slnx`.

### 8.0
* Replace the extractor's TPL Dataflow pipeline with `Parallel.ForEachAsync` and remove the Dataflow dependency.

### 1.10
* Fix extraction of literal strings in description attribute arguments.
* Honor `--unixstyle` when exporting C# source-reference paths on Windows ([#55](https://github.com/perpetualKid/GetText.NET/issues/55), [#56](https://github.com/perpetualKid/GetText.NET/pull/56)).
* Update the extractor and modern Windows Forms target to .NET 8, retaining .NET Framework support for Windows Forms and .NET Standard 2.0 for the core library.

### 1.9
* Include [PR49](https://github.com/perpetualKid/GetText.NET/pull/49) improving CatalogManager to support multiple Cultures and make it thread-safe
* Bugfix for wrongly extracted string when multiple concatenated function calls where one of them is a GetString call https://github.com/perpetualKid/GetText.NET/issues/50

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
