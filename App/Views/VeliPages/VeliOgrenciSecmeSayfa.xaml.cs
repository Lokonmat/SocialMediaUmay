using App.Models;
using Plugin.FirebasePushNotification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.VeliPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VeliOgrenciSecmeSayfa : ContentPage
    {
        string userId = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        string mail = Preferences.Get("Mail", "");
        string phoneNumber = Preferences.Get("PhoneNumber", "");
        string tableName = Preferences.Get("Type", ""); 
        private ObservableCollection<OgrenciSinif> OS = new ObservableCollection<OgrenciSinif>();

        public VeliOgrenciSecmeSayfa()
        {
            InitializeComponent();
            ogrenciSinifCollectionView.ItemsSource = OS;
            UpdateOgrenciSinifsAsync();
        }
        private void CocukEkle_Clicked(object sender, EventArgs e)
        {
            string cocukTamAdı = cocukTamAd.Text;
            string sinifKodu = sinifKod.Text;

            if(cocukTamAdı != "")
            {
                // Öğrenci kaydının varlığını kontrol et
                string checkExistingStudentQuery = $"select COUNT(*) FROM ogrenci WHERE ogrenci_ad = '{cocukTamAdı}' AND veli_id = {userId}";
                string existingStudentCount = WebServis.TestKBM(checkExistingStudentQuery);
                int count = int.Parse(existingStudentCount);

                if (count == 0)
                {
                    string token = CrossFirebasePushNotification.Current.Token;
                    // Öğrenci kaydı yok, yeni öğrenci kaydını ekleyebilirsiniz
                    string insertStudentQuery = $"INSERT INTO `ogrenci`(`ogrenci_ad`, `veli_id`, `veli_ad`, `veli_token`) VALUES ('{cocukTamAdı}', {userId}, '{fullName}', '{token}')";
                    WebServis.TestKBM(insertStudentQuery);

                    string insertStudentToClassQuery = $@"INSERT INTO `ogrenci_sinif` (ogrenci_id, ogrenci_ad, veli_id, veli_ad, veli_token, ogretmen_id, ogretmen_ad, sinif_id, sinif_ad, sinif_kod)
                            select
                                o.ogrenci_id AS ogrenci_id,
                                '{cocukTamAdı}' AS ogrenci_ad,
                                {userId} AS veli_id,
                                '{fullName}' AS veli_ad,
                                o.veli_token AS veli_token,
                                s.ogretmen_id,
                                s.ogretmen_adi,
                                s.sinif_id,
                                s.sinif_adi AS sinif_ad,
                                s.sinif_kod
                            FROM
                                ogrenci o,
                                sinif s
                            WHERE
                                s.sinif_kod = '{sinifKodu}'
                                AND o.ogrenci_ad = '{cocukTamAdı}'
                                AND o.veli_id = {userId}
                                AND veli_token = '{token}';
                        ";
                    WebServis.TestKBM(insertStudentToClassQuery);

                    // Öğrenci siniflarını güncelle
                    UpdateOgrenciSinifsAsync();
                }
                else
                {
                    string token = CrossFirebasePushNotification.Current.Token;
                    // Öğrenci kaydı zaten var, sadece ogrenci_sinif tablosuna kaydı ekle
                    string insertStudentToClassQuery = $"INSERT INTO ogrenci_sinif (ogrenci_id, ogrenci_ad, veli_id, veli_ad, veli_token, ogretmen_id, ogretmen_ad, sinif_id, sinif_ad, sinif_kod) SELECT o.ogrenci_id AS ogrenci_id, '{cocukTamAdı}' AS ogrenci_ad, {userId} AS veli_id, '{fullName}' AS veli_ad, o.veli_token AS veli_token, s.ogretmen_id, s.ogretmen_adi, s.sinif_id, s.sinif_adi AS sinif_ad, s.sinif_kod FROM ogrenci o, sinif s WHERE s.sinif_kod = '{sinifKodu}' AND o.ogrenci_ad = '{cocukTamAdı}' AND o.veli_id = '{userId}' AND veli_token = '{token}'";
                    WebServis.TestKBM(insertStudentToClassQuery);

                    // Öğrenci siniflarını güncelle
                    UpdateOgrenciSinifsAsync();
                }
            }
            else
            {
                DisplayAlert("UYARI", "Lütfen çocuğunuzu adını ve soyadını giriniz.", "Tamam");
            }
        }    
        private void Button_Clicked(object sender, EventArgs e)
        {
            if (sender is Button sellectButton && sellectButton.BindingContext is OgrenciSinif ogrenciSinifları)
            {
                int selectedStudentId = ogrenciSinifları.Ogrenci_Id;
                int selectedOgretmenId = ogrenciSinifları.Ogretmen_Id;
                string selectedVeliToken = ogrenciSinifları.Veli_Token;
                Preferences.Set("Ogretmen_Id", selectedOgretmenId.ToString());
                Preferences.Set("Ogrenci_Id", selectedStudentId.ToString());
                WebServis.TestKBM($"UPDATE `veli` SET `ogrenci_id`= {selectedStudentId} WHERE `Id` = {userId}");
                WebServis.TestKBM($"UPDATE `veli` SET `ogrenci_id`= {selectedStudentId} WHERE `Id` = {userId}");
                WebServis.TestKBM($"UPDATE `ogrenci` SET `veli_token`='{selectedVeliToken}' WHERE `ogrenci_id` = {selectedStudentId}");
                Application.Current.MainPage = new VeliAnaSayfa();
            }
        }
        private async Task UpdateOgrenciSinifsAsync()
        {
            List<OgrenciSinif> ogrenciSinifları = await GetOgrenciSinifsFromDataBase();
            OS.Clear();
            foreach(var ogrenciSinif in ogrenciSinifları)
            {
                OS.Add(ogrenciSinif);
            }
        }        
        private async Task<List<OgrenciSinif>> GetOgrenciSinifsFromDataBase()
        {
            List<OgrenciSinif> ogrenciSinifları = new List<OgrenciSinif>();

            string sorgu = $"select `ogrenci_sinif_id`, `ogrenci_id`, `ogrenci_ad`, `veli_id`, `veli_ad`, `veli_token`, `ogretmen_id`, `sinif_id`, `sinif_ad`, `sinif_kod` FROM `ogrenci_sinif` WHERE `veli_id`= {userId} AND `veli_Ad`= '{fullName}'";
            string sonuclar = WebServis.TestKBM(sorgu);

            string[] ogrenciSinifBilgileri = sonuclar.Split('\n');
            foreach(var ogrenciSinifBilgi in ogrenciSinifBilgileri)
            {
                string[] infoArray = ogrenciSinifBilgi.Split('|');
                if(infoArray.Length >= 10)
                {
                    int ogrenci_sinif_id = int.Parse(infoArray[0].Trim());
                    int ogrenci_id = int.Parse(infoArray[1].Trim());
                    string ogrenci_ad = infoArray[2].Trim();
                    int veli_id = int.Parse(infoArray[3].Trim());
                    string veli_ad = infoArray[4].Trim();
                    string veli_token = infoArray[5].Trim();
                    int ogretmen_id = int.Parse(infoArray[6].Trim());
                    int sinif_id = int.Parse(infoArray[7].Trim());
                    string sinif_ad = infoArray[8].Trim();
                    string sinif_kod = infoArray[9].Trim();
                    OgrenciSinif ogrenciSinif = new OgrenciSinif
                    {
                        Ogrenci_Sinif_Id = ogrenci_sinif_id,
                        Ogrenci_Id = ogrenci_id,
                        Ogrenci_Ad = ogrenci_ad,
                        Veli_Id = veli_id,
                        Veli_Ad = veli_ad,
                        Veli_Token = veli_token,
                        Ogretmen_Id = ogretmen_id,
                        Sinif_Id = sinif_id,
                        Sinif_Ad = sinif_ad,
                        Sinif_Kod = sinif_kod
                    };
                    ogrenciSinifları.Add(ogrenciSinif);
                }
            }
            return ogrenciSinifları;
        }
    }
}