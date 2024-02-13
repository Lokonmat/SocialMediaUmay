using System;
using System.Collections.Generic;
using System.Text;

namespace App.Models
{
    public class BegenenListe
    {
        public int begeni_id { get; set; }
        public int BegenenKisiId { get; set; }
        public string BegenenKisiAd { get; set; }
        public int PaylasimId { get; set; }
        public string PaylasimAd { get; set; }
    }
}
