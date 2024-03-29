﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Frontend
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(defaultValue: false);
            Application.Run(new Window());
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}