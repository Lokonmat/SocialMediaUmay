using App.Utils;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UIKit;

namespace App.iOS
{
    public class PlatformSpecific : IPlatformSpecific
    {
        public void CloseApp()
        {
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
        }
        public static string GetCacheFolderPath()
        {
            string cachePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            string specificFolder = Path.Combine(cachePath, "Temp");

            if (!Directory.Exists(specificFolder))
            {
                Directory.CreateDirectory(specificFolder);
            }

            return specificFolder;
        }
    }
}