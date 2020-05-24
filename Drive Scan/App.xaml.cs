using System;
using System.Windows;
using System.Runtime.InteropServices;
using Drive_Scan.Config;

namespace Drive_Scan
{
    /// <summary>
    /// Code Behind App.xaml
    /// </summary>
    public partial class App : Application
    {
        //Stops multiple scans from running at once

        #region Console
        // To create a console so we can see debug information
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        #endregion

        private void OnStartup(object sender, StartupEventArgs e)
		{
            // Show window console if debug mode is enabled
            if (Convert.ToBoolean(Convert.ToInt16(ConfigHandler.readValue("debug"))))
            {
                AllocConsole();
            }

			// Create the startup window
			DriveScanWindow wnd = new DriveScanWindow();
            
			// Show the window
			wnd.Show();
		}
    }
}
