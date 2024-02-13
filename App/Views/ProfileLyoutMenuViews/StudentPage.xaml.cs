using App.Models;
using App.Views.ChatPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.ProfileLyoutMenuViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StudentPage : ContentPage
    {
        string kullanici_id = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        string class_code = Preferences.Get("ClassCode", "");
        public ObservableCollection<OgrenciSinif> Students = new ObservableCollection<OgrenciSinif>();       
        public StudentPage()
        {
            InitializeComponent();
            collectionView.ItemsSource = Students;
            UpdateClassAsync();
        }
        private async Task UpdateClassAsync()
        {
            List<OgrenciSinif> ogrenciler = await GetOgrenciFromUserClasses();
            foreach (var post in ogrenciler)
            {
                Students.Add(post);
            }
        }
        private async Task<List<OgrenciSinif>> GetOgrenciFromUserClasses()
        {
            List<OgrenciSinif> ogrenciler = new List<OgrenciSinif>();
            try
            {
                string query = $"select `ogrenci_sinif_id`, `ogrenci_id`, `ogrenci_ad`, `veli_id`, `veli_ad`, `sinif_id`, `sinif_ad`, `sinif_kod` FROM `ogrenci_sinif` WHERE `sinif_kod` = '{class_code}'";
                string results = WebServis.TestKBM(query);
                string[] ogrenciBilgiler = results.Split('\n');
                foreach(var ogrenciBilgi in ogrenciBilgiler)
                {
                    string[] infoArray = ogrenciBilgi.Split('|');
                    if(ogrenciBilgi.Length >= 8)
                    {
                        int ogrenci_sinif_id = int.Parse(infoArray[0].Trim());
                        int ogrenci_id = int.Parse(infoArray[1].Trim());
                        string ogrenci_ad = infoArray[2].Trim();
                        int veli_id = int.Parse(infoArray[3].Trim());
                        string veli_ad = infoArray[4].Trim();
                        int sinif_id = int.Parse(infoArray[5].Trim());
                        string sinif_ad = infoArray[6].Trim();
                        string sinif_kod = infoArray[7].Trim();

                        OgrenciSinif ogrenci = new OgrenciSinif
                        {
                            Ogrenci_Sinif_Id = ogrenci_sinif_id,
                            Ogrenci_Id = ogrenci_id,
                            Ogrenci_Ad = ogrenci_ad,
                            Veli_Id = veli_id,
                            Veli_Ad = veli_ad,
                            Sinif_Id = sinif_id,
                            Sinif_Ad = sinif_ad,
                            Sinif_Kod = sinif_kod
                        };
                        ogrenciler.Add(ogrenci);
                    }
                }
            }
            catch
            {

            }
            return ogrenciler;
        }       
        private void messageButton_Clicked(object sender, EventArgs e)
        {            
            if (sender is Button messageButton && messageButton.BindingContext is OgrenciSinif ogrenci)
            {
                int secilen_ogrenci_id = ogrenci.Ogrenci_Id;
                string secilen_ogrenci_isim = ogrenci.Ogrenci_Ad;
                Preferences.Set("OgrenciId", $"{secilen_ogrenci_id}");
                Preferences.Set("OgrenciAd", $"{secilen_ogrenci_isim}");
                Navigation.PushAsync(new TeacherToParentChatPage());
            }
        }
    }
}