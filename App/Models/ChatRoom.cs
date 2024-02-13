using System;
using System.Collections.Generic;
using System.Text;

namespace App.Models
{
    public class ChatRoom
    {
        public string Name { get; set; }
        public string Ogretmen_Id { get; set; }
        public string Ogretmen_Ad { get; set; }
        public string Ogrenci_Id { get; set; }
        public string Ogrenci_Ad { get; set; }
        public string Veli_Id { get; set; }
        public string Veli_Ad { get; set; }
        public string Sinif_Ad { get; set; }
        public string Key { get; set; }
    }
}
