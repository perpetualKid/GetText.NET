using System;

using GetText;

namespace Examples.OtherLibrary
{
    /// <summary>
    /// Sample code only to show working across multiple assemblies with separate catalogs each
    /// </summary>
    public static class ProcessDetails
    {
        public static string GetProcessIdText()
        {
            return CatalogManager.Catalog.GetString($"This program is running as process number \"{System.Diagnostics.Process.GetCurrentProcess().Id}\".");
        }

    }
}
