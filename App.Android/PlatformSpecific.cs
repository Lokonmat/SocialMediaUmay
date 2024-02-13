using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using App.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace App.Droid
{
    public class PlatformSpecific : IPlatformSpecific
    {
        public void CloseApp()
        {
            Process.KillProcess(Process.MyPid());
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