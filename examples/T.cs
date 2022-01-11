<<<<<<< HEAD:examples/T.cs
using System;
using GetText;

//
// Usage:
//		T._("Hello, World!"); // GetString
//		T._n("You have {0} apple.", "You have {0} apples.", count, count); // GetPluralString
//		T._p("Context", "Hello, World!"); // GetParticularString
//		T._pn("Context", "You have {0} apple.", "You have {0} apples.", count, count); // GetParticularPluralString
//
namespace Example
{
	internal class T
	{
		private static readonly ICatalog _Catalog = new Catalog("Example", "./locale");


		public static string _(string text)
		{
			return _Catalog.GetString(text);
		}

		public static string _(string text, params object[] args)
		{
			return _Catalog.GetString(text, args);
		}

		public static string _n(string text, string pluralText, long n)
		{
			return _Catalog.GetPluralString(text, pluralText, n);
		}

		public static string _n(string text, string pluralText, long n, params object[] args)
		{
			return _Catalog.GetPluralString(text, pluralText, n, args);
		}

		public static string _p(string context, string text)
		{
			return _Catalog.GetParticularString(context, text);
		}

		public static string _p(string context, string text, params object[] args)
		{
			return _Catalog.GetParticularString(context, text, args);
		}

		public static string _pn(string context, string text, string pluralText, long n)
		{
			return _Catalog.GetParticularPluralString(context, text, pluralText, n);
		}

		public static string _pn(string context, string text, string pluralText, long n, params object[] args)
		{
			return _Catalog.GetParticularPluralString(context, text, pluralText, n, args);
		}
	}
||||||| d8a130a:doc/examples/T.cs
using System;
using NGettext;

//
// Usage:
//		T._("Hello, World!"); // GetString
//		T._n("You have {0} apple.", "You have {0} apples.", count, count); // GetPluralString
//		T._p("Context", "Hello, World!"); // GetParticularString
//		T._pn("Context", "You have {0} apple.", "You have {0} apples.", count, count); // GetParticularPluralString
//
namespace Example
{
	internal class T
	{
		private static readonly ICatalog _Catalog = new Catalog("Example", "./locale");


		public static string _(string text)
		{
			return _Catalog.GetString(text);
		}

		public static string _(string text, params object[] args)
		{
			return _Catalog.GetString(text, args);
		}

		public static string _n(string text, string pluralText, long n)
		{
			return _Catalog.GetPluralString(text, pluralText, n);
		}

		public static string _n(string text, string pluralText, long n, params object[] args)
		{
			return _Catalog.GetPluralString(text, pluralText, n, args);
		}

		public static string _p(string context, string text)
		{
			return _Catalog.GetParticularString(context, text);
		}

		public static string _p(string context, string text, params object[] args)
		{
			return _Catalog.GetParticularString(context, text, args);
		}

		public static string _pn(string context, string text, string pluralText, long n)
		{
			return _Catalog.GetParticularPluralString(context, text, pluralText, n);
		}

		public static string _pn(string context, string text, string pluralText, long n, params object[] args)
		{
			return _Catalog.GetParticularPluralString(context, text, pluralText, n, args);
		}
	}
=======
using System;
using NGettext;

//
// Usage:
//		using static Example.NGettextShortSyntax;
//		
//		_("Hello, World!"); // GetString
//		_n("You have {0} apple.", "You have {0} apples.", count, count); // GetPluralString
//		_p("Context", "Hello, World!"); // GetParticularString
//		_pn("Context", "You have {0} apple.", "You have {0} apples.", count, count); // GetParticularPluralString
//
namespace Example
{
	internal static class NGettextShortSyntax
	{
		private static readonly ICatalog _Catalog = new Catalog("Example", "./locale");


		public static string _(string text)
		{
			return _Catalog.GetString(text);
		}

		public static string _(string text, params object[] args)
		{
			return _Catalog.GetString(text, args);
		}

		public static string _n(string text, string pluralText, long n)
		{
			return _Catalog.GetPluralString(text, pluralText, n);
		}

		public static string _n(string text, string pluralText, long n, params object[] args)
		{
			return _Catalog.GetPluralString(text, pluralText, n, args);
		}

		public static string _p(string context, string text)
		{
			return _Catalog.GetParticularString(context, text);
		}

		public static string _p(string context, string text, params object[] args)
		{
			return _Catalog.GetParticularString(context, text, args);
		}

		public static string _pn(string context, string text, string pluralText, long n)
		{
			return _Catalog.GetParticularPluralString(context, text, pluralText, n);
		}

		public static string _pn(string context, string text, string pluralText, long n, params object[] args)
		{
			return _Catalog.GetParticularPluralString(context, text, pluralText, n, args);
		}
	}
>>>>>>> upstream/master:samples/NGettextShortSyntax.cs
}