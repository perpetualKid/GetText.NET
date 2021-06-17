GetText.NET 
========

A cross-platform .NET implementation of the GNU Gettext library, initially forked from [NGettext](https://github.com/VitaliiTsilnyk/NGettext).  
GetText.NET supports [**string interpolation**](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated) and [**FormattableString**](https://docs.microsoft.com/en-us/dotnet/api/system.formattablestring?redirectedfrom=MSDN&view=netcore-3.1).  
GetText.NET.Extractor extracts string values from C# source code in  GetText.ICatalog method calls to Get\*String(), and also supports localizing Windows.Forms.Control properties showing text, such as .Text, .HeaderText or .ToopTipText.

GetText.NET has simplified usage over NGetText and original GNU Gettext tools, i.e. removed Get\*String**Fmt**() methods and pass parameters directly to Get\*String() overloads, and also removed the need for LC_MESSAGES subfolder to place translation files.    

This fully managed library targets **Microsoft .NET Standard 2.0** to support a wide range of .NET implementations including **.NET Framework** >=4.6.1, **.NET Core** >=2.0, **Mono** and [more](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md).
It is fully **COM** and **CLS** compatible.

Translations are loaded directly from gettext *.mo files. Multiple translation domains and multiple locales can be loaded in one application instance. There is no need to compile satellite assemblies. GetText.NET supports both little-endian and big-endian MO files, automatic (header-based) encoding detection and (optional) plural form rules parsing.

Documentation and Samples
---------------
For details on each product, examples and FAQ please check the [Wiki](https://github.com/perpetualKid/GetText.NET/wiki) pages.

Version History
---------------

Please check [Releases Notes](ReleaseNotes.md) for version history

Status
---------------

[![Build Status](https://dev.azure.com/perpetualKid/GetText.NET/_apis/build/status/Master%20CI?branchName=main)](https://dev.azure.com/perpetualKid/GetText.NET/_build/latest?definitionId=5&branchName=main)

| Package | Status |
| ----------- | ----------- |
|GetText.NET|[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/gettext.net?color=brightgreen&style=plastic)](https://www.nuget.org/packages/gettext.net)|
|GetText.NET.PluralCompile|[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/gettext.net.pluralcompile?color=brightgreen&style=plastic)](https://www.nuget.org/packages/GetText.NET.PluralCompile/)|
|GetText.NET.WindowsForms|[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/GetText.NET.WindowsForms?color=brightgreen&style=plastic)](https://www.nuget.org/packages/GetText.NET.WindowsForms/)|
|GetText.NET.Extractor|[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/gettext.net.extractor?color=brightgreen&style=plastic)](https://www.nuget.org/packages/GetText.NET.Extractor/)|



For further details on each product, also check the [Wiki](https://github.com/perpetualKid/GetText.NET/wiki) pages.
