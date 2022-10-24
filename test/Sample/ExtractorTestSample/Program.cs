using System.Drawing;

using GetText;

namespace ExtractorTestSample
{
    // this code is not intendend to do anything useful, but serves as test input for GetText.Extractor tests only.
    // be careful when changing anything such as adding or removing lines, as line numbers are used to map test results,
    // but are hardcoded in GetText.Extractor.Tests.Engine.ParserTests.cs
    internal class Program
    {
        static void Main(string[] args)
        {
            Catalog catalog = CatalogManager.Catalog;

            catalog.GetString("Simple literal string");//Expected: line 16 : "Simple literal string"
            catalog.GetString("Simple literal string with param {0}", 123456);//Expected: line 17 : "Simple literal string with param"
            catalog.GetString($"Simple interpolated string", 123456);//Expected: line 18 : "Simple interpolated string"
            catalog.GetString($"Simple {123456} interpolated string");//Expected: line 19 : "Simple {0} interpolated string"
            catalog.GetString($"{1} and {2} params interpolated string");//Expected: line 20 : "{0} and {1} params interpolated string"
            catalog.GetString($"Nested interpolated string {"Literal string"} literal param");//Expected: line 21 : "Nested interpolated string {0} literal param"
            catalog.GetString($"0 - Nested interpolated string {catalog.GetString("1 - Literal string")} param");//Expected: line 22 : "0 - Nested interpolated string {0} param", "1 - Literal string"
            catalog.GetString($"0 - Another nested interpolated string with {catalog.GetString("1 - one literal string")} and {catalog.GetString("2 - another literal string")} param");//Expected: line 23 : "0 - Another nested interpolated string with {0} and {1} param", "1 - one literal string", "2 - another literal string"
            catalog.GetString($"0 - Triple nested {catalog.GetString($"1 - double nested {catalog.GetString($"2 - single nested {catalog.GetString("3 - Inner literal string")}")}")}");//Expected: line 24 : "0 - Triple nested {0}", "1 - double nested {0}", "2 - single nested {0}", "3 - Inner literal string"
            catalog.GetString($"Simple" + " concatenation");//Expected: line 25 : "Simple concatenation"
            catalog.GetString($"first part" + " and " + "another part");//Expected: line 26 : "first part and another part"
            //Test case to cover issue #44 https://github.com/perpetualKid/GetText.NET/issues/44
            string Specifier = "Specifier";
            catalog.GetString($"{Specifier}login {"username".Color(Color.White)} {"password".Color(Color.Green)} - Logs in using your username and password.");//Expected: line 30 : "{0}login {1} {2} - Logs in using your username and password."
        }

    }

    static class StringExtension
    {
        public static string Color(this string value, Color color)
        {
            return value;
        }
    }

}