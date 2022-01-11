using System;
using Gettext;

//
// Usage:
//		using static Example.GettextShortSyntax;
//		
//		_("Hello, World!"); // GetString
//		_n("You have {0} apple.", "You have {0} apples.", count, count); // GetPluralString
//		_p("Context", "Hello, World!"); // GetParticularString
//		_pn("Context", "You have {0} apple.", "You have {0} apples.", count, count); // GetParticularPluralString
//
namespace Example
{
	internal static class GettextShortSyntax
	{
		private static readonly ICatalog catalog = new Catalog("Example", "./locale");


		public static string _(string text)
		{
			return catalog.GetString(text);
		}

		public static string _(string text, params object[] args)
		{
			return catalog.GetString(text, args);
		}

		public static string _n(string text, string pluralText, long n)
		{
			return catalog.GetPluralString(text, pluralText, n);
		}

		public static string _n(string text, string pluralText, long n, params object[] args)
		{
			return catalog.GetPluralString(text, pluralText, n, args);
		}

		public static string _p(string context, string text)
		{
			return catalog.GetParticularString(context, text);
		}

		public static string _p(string context, string text, params object[] args)
		{
			return catalog.GetParticularString(context, text, args);
		}

		public static string _pn(string context, string text, string pluralText, long n)
		{
			return catalog.GetParticularPluralString(context, text, pluralText, n);
		}

		public static string _pn(string context, string text, string pluralText, long n, params object[] args)
		{
			return catalog.GetParticularPluralString(context, text, pluralText, n, args);
		}
	}
}