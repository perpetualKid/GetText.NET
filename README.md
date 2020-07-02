GetText.NET 
========

[![Build Status](https://dev.azure.com/perpetualKid/GetText.NET/_apis/build/status/Master%20CI?branchName=master)](https://dev.azure.com/perpetualKid/GetText.NET/_build/latest?definitionId=5&branchName=master)

| Package | Status |
| ----------- | ----------- |
|GetText.NET|[![NuGet version](https://badge.fury.io/nu/gettext.net.svg)](https://badge.fury.io/nu/gettext.net)|
|GetText.NET.PluralCompile|[![NuGet version](https://badge.fury.io/nu/gettext.net.svg)](https://badge.fury.io/nu/gettext.net)|


A cross-platform .NET implementation of the GNU/Gettext library, largely based on [NGettext](https://github.com/VitaliiTsilnyk/NGettext). 

GetText.NET has simplified usage (i.e. removed the need for LC_MESSAGES subfolder to place translation files), and focuses on more recent .NET implementations such as .NET framework 4.8 and .NET Core 3.1. To allow independent nuget publishing, it has been renamed to GetText.NET and repackaged.

This fully managed library targets **Microsoft .NET Standard 2.0** to support a wide range of .NET implementations including **.NET Framework** 4.6.1, **.NET Core** 2.0, **Mono** and [more](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md).
It is fully **COM** and **CLS** compatible.

This implementation loads translations directly from gettext *.mo files and can handle multiple translation domains and multiple locales in one application instance. There is no need to compile satellite assemblies. GetText.NET supports both little-endian and big-endian MO files, automatic (header-based) encoding detection and (optional) plural form rules parsing.

By default, GetText.NET uses pre-compiled plural form rules for most known locales. Additionally, plural form rules can be parsed from *.mo file headers (see `MoCompilingPluralLoader` description below) or custom plural rules can be passed to the Catalog instance through API.

Why GetText.NET?
---------------

While there are other GNU/Gettext implementations for C#, each has its own scope, i.e.

[**Mono.Unix.Catalog**](http://docs.go-mono.com/?link=T%3aMono.Unix.Catalog)
Mono's Catalog is merely a binding to three native functions (bindtextdomain, gettext, and ngettext). It does not support multiple domains/locales and contexts. It is not cross-patform, and may have issues under Windows OS.

[**GNU.Gettext**](https://www.gnu.org/software/gettext/manual/html_node/C_0023.html)
Gettext uses satellite assemblies for translation files and does not support multiple locales in one application instance.
It's hard to build and maintain translation files and change locale inside an application. A .NET based implementation can be found [here](https://github.com/arbinada-com/gettext-dotnet), but there seems no active development.

[**NGettext**](https://github.com/VitaliiTsilnyk/NGettext)
GetText.NET is largely a clone of NGettext and has the same ICatalog interface. It uses a simplified file structure (no need for LC_MESSAGES subfolder in each translation) and provides default constructor for standard use cases.

**GetText.NET**
* GetText.NET is fully cross-platform, no need for other native or managed 3rd-party libraries
* GetText.NET supports multiple localization domains. Translation files can be separated for each of an application's module or plugin
* GetText.NET supports multiple locales in one application instance and has a really simple API to choose the locale of an application
* GetText.NET loads translations directly from *.mo files in standard .NET localization folder structure. Furthermore, translations can be loaded from other specified file or stream
* GetText.NET supports message contexts
* GetText.NET uses a nice and simple API, compatible with any type of application (console, GUI, web...)
* GetText.NET.WindowsForms (separate package) allows localization of Windows Forms standard properties


Installation and usage
----------------------

Install as [NuGet package](https://www.nuget.org/packages/GetText.NET/)
from the package manager console:
```
PM> Install-Package GetText.NET
```
or through .NET CLI utility:
```
$ dotnet add package GetText.NET
```

Using GetText.NET:
```csharp
	using GetText;
```
```csharp
	// This will load translations from "./locale/<CurrentUICulture>/Example.mo"
	ICatalog catalog = new Catalog("Example", "./locale");
	
	// or
	
	// This will load translations from "./locale/ru_RU/Example.mo"
	ICatalog catalog = new Catalog("Example", "./locale", new CultureInfo("ru-RU"));
```
```csharp
	Console.WriteLine(catalog.GetString("Hello, World!")); // will translate "Hello, World!" using loaded translations
	Console.WriteLine(catalog.GetString("Hello, {0}!", "World")); // String.Format support
```

### .NET CoreCLR

To use this library under CoreCLR for encodings different from UTF-8 in *.mo files, [System.Text.Encoding.CodePages](https://www.nuget.org/packages/System.Text.Encoding.CodePages/) package needs to be included into the application and initialized like this:
```csharp
	#if NETCOREAPP1_0
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	#endif
```

### Culture-specific message formatting

```csharp
	// All translation methods support String.Format optional arguments
	catalog.GetString("Hello, {0}!", "World");
	
	// Catalog's current locale will be used to format messages correctly
	catalog.GetString("Here's a number: {0}!", 1.23);
	// Will return "Here's a number: 1.23!" for en_US locale
	// But something like this will be returned for ru_RU locale with Russian translation: "А вот и номер: 1,23!"
```

### Plural forms

```csharp
	catalog.GetPluralString("You have {0} apple.", "You have {0} apples.", count);
	// Returns (for en_US locale):
	//     "You have {0} apple." for count = 1
	//     "You have {0} apples." otherwise


	catalog.GetPluralString("You have {0} apple.", "You have {0} apples.", 5, 5);
	// Returns translated plural massage: "You have 5 apples." (for en_US locale)
	// First “5” used in plural forms determination; second — in String.Format method


	// Example plural forms usage for fractional numbers:
	catalog.GetPluralString("You have {0} apple.", "You have {0} apples.", (long)1.23, 1.23);
	// Internal String.Format will be used in context of catalog's locale and formats objects respectively
```

### Contexts

```csharp
	catalog.GetParticularString("Menu|File|", "Open"); // will translate message "Open" using context "Menu|File|"
	catalog.GetParticularString("Menu|Project|", "Open"); // will translate message "Open" using context "Menu|Project|"
```


### Multiple locales and domains in one application instance

```csharp
	// "./locale/en_US/Example.mo"
	ICatalog example_en = new Catalog("Example", "./locale", new CultureInfo("en-US"));

	// "./locale/fr/LC_MESSAGES/Example.mo"
	ICatalog example_fr = new Catalog("Example", "./locale", new CultureInfo("fr"));

	// "./locale/<CurrentUICulture>/AnotherDomainName.mo"
	ICatalog anotherDomain = new Catalog("AnotherDomainName", "./locale");
```

### Direct MO file loading

```csharp
	Stream moFileStream = File.OpenRead("path/to/domain.mo");
	ICatalog catalog = new Catalog(moFileStream, new CultureInfo("en-US"));
```

### Parsing plural rules from the *.mo file header

GetText.NET can parse plural rules directly from the *.mo file header and compile it to a dynamic method in runtime.
To enable this option pass the `MoCompilingPluralLoader` from the [GetText.NET.PluralCompile](https://www.nuget.org/packages/GetText.NET.PluralCompile) package to the Catalog constructor:
```csharp
	ICatalog catalog = new Catalog(new MoCompilingPluralLoader("Example", "./locale"));
```
This loader will parse plural formula from the *.mo file header and compile it to plural rule for the Catalog instance at runtime, just when the *.mo file loads.
The Catalog's *PluralString methods performance will be the same as if using GetText.NET default precompiled plural rules, only *.mo file loading will be slightly slower.

This feature requires enabled JIT compiler in the runtime. MoCompilingPluralLoader can not be used in an full-AOT environment.
This is why MoCompilingPluralLoader moved to a separate library.

For hosts without enabled JIT, use `MoAstPluralLoader` which will only parse plural formulas to an abstract syntax tree
and interpret it every time a call to *PluralString method is made from the catalog, without compiling.
Please note that this solution is slightly slower than MoCompilingPluralLoader even it's pretty well optimized.

### Custom plural formulas

```csharp
	catalog.PluralRule = new PluralRule(numPlurals, n => ( n == 1 ? 0 : 1 ));
```
Custom plural rule generator can be created by implementing IPluralRuleGenerator interface, which will create a PluralRule for any culture.

Debugging
---------

Debug version of the GetText.NET binary outputs debug messages to System.Diagnostics.Trace.
Register trace listeners to see GetText.NET debug messages.
Please note that Release version of the GetText.NET binary does not produse any trace messages.

```csharp
	Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
```



Shorter syntax
--------------

In `doc/examples/T.cs` an example of shorter syntax creation for GetText.NET is shown:
```csharp
	T._("Hello, World!"); // GetString
	T._n("You have {0} apple.", "You have {0} apples.", count, count); // GetPluralString
	T._p("Context", "Hello, World!"); // GetParticularString
	T._pn("Context", "You have {0} apple.", "You have {0} apples.", count, count); // GetParticularPluralString
```



Poedit compatibility
--------------------

For [Poedit](http://www.poedit.net/) compatibility, the plural form needs to be specified in the *.pot file header, also for english language:
```
	"Plural-Forms: nplurals=2; plural=n != 1;\n"
```

And a keywords list:
```
	"X-Poedit-KeywordsList: GetString;GetPluralString:1,2;GetParticularString:1c,2;GetParticularPluralString:1c,2,3;_;_n:1,2;_p:1c,2;_pn:1c,2,3\n"
```


