using System;
using System.Windows.Forms;

namespace Examples.HelloForms
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if NETCOREAPP3_1
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (Form form = new Form1())
                Application.Run(form);
        }
    }
}
