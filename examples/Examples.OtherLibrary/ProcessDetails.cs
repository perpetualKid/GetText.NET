using System;

using GetText;

namespace Examples.OtherLibrary
{
    public static class ProcessDetails
    {
        public static string GetProcessIdText(ICatalog catalog)
        {
            return catalog.GetString($"This program is running as process number \"{System.Diagnostics.Process.GetCurrentProcess().Id}\".");
        }

    }
}
