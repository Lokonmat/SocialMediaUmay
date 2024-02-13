using System;
using System.Collections.Generic;
using System.Text;

namespace App.Models
{
    public class Yorum
    {
        public int Yorum_Id { get; set; }
        public int Paylasim_Id { get; set; }
        public int Kullanici_Id { get; set; }
        public string Kullanici_Ad { get; set; }
        public string Yorum_Metin { get; set; }
        public DateTime Tarih { get; set; }
    }
}
