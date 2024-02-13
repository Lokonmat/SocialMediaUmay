
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace App.Models
{
    public class ImagePost : INotifyPropertyChanged
    {
        public int id { get; set; }
        public int kullanici_id { get; set; }
        public int Ogrenci_Id { get; set; }
        public int Ogretmen_Id { get; set; }
        public int paylasim_Id { get; set; }
        public string kullanici_adi { get; set; }
        public string Ogrenci_Ad { get; set; }
        public string Ogretmen_Ad { get; set; }
        public string paylasim_adi { get; set; }
        public string begeni_durum { get; set; }
        public string aciklama { get; set; }
        public string Yorum_Mevcut { get; set; }
        public string Download_Durum { get; set; }
        public string paylasim_turu { get; set; }
        public string sinif_adi { get; set; }
        public string PostImage { get; set; }
        public ImageSource PostImage2 { get; set; }
        public MediaElement PostImage3 { get; set; }
        public DateTime zaman { get; set; }

        private int _begeni_sayisi;
        public int begeni_sayisi
        {
            get { return _begeni_sayisi; }
            set
            {
                if (_begeni_sayisi != value)
                {
                    _begeni_sayisi = value;
                    OnPropertyChanged(nameof(begeni_sayisi));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
