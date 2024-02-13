using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace App.Utils
{
    public class User
    {
        private static string uID;

        public static string userName
        {
            get { return uID; }
            set { uID = value; }
        }
        private User()
        {

        }
    }
}
