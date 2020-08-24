using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GetText.Extractor.Tests")]

namespace GetText.Extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Hello World!", args);
            Console.WriteLine(new Template.Catalog(@"C:\Temp\messages.pot").ToString());
        }
    }
}
