using System;
using System.Collections.Generic;
using System.Text;

namespace App.Models
{
    public class OgrenciSinif
    {
        public int Ogrenci_Sinif_Id { get; set; }
        public int Ogrenci_Id { get; set; }
        public string Ogrenci_Ad { get; set; }
        public int Veli_Id { get; set; }
        public string Veli_Ad { get; set; }
        public string Veli_Token { get; set; }
        public int Ogretmen_Id { get; set; }
        public string Ogretmen_Ad { get; set; }
        public int Sinif_Id { get; set; }
        public string Sinif_Ad { get; set; }
        public string Sinif_Kod { get; set; }
    }
}
