using App.Models;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.OgretmenPages.Paylasimlar
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SohbetPaylasimSayfasi : ContentPage
    {
        string id = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        private bool _allStudentsSelected;
        List<string> tokens = new List<string>();
        private ObservableCollection<Ogrenci> Ogrencis = new ObservableCollection<Ogrenci>();
        public SohbetPaylasimSayfasi()
        {
            InitializeComponent();
            studentCollectionView.ItemsSource = Ogrencis;
            selectAllStudent.IsVisible = false;
            selectAllStudentLabel.IsVisible = false;

            LoadSinifPicker();
        }
        private async void LoadSinifPicker()
        {
            List<Sinif> sinifList = await GetSinifsFromDatabase();

            foreach (Sinif sinif in sinifList)
            {
                picker1.Items.Add(sinif.sinif_adi);
            }
            picker1.SelectedIndexChanged += async (sender, args) =>
            {
                if (picker1.SelectedIndex != -1)
                {
                    selectAllStudent.IsVisible = true;
                    selectAllStudentLabel.IsVisible = true;
                    await UpdateStudentList();
                }
            };
            await UpdateStudentList();
        }
        private async Task UpdateStudentList()
        {
            Ogrencis.Clear();

            if (picker1.SelectedIndex != -1)
            {
                List<Ogrenci> ogrenciler = await GetOgrencisAsync();
                foreach (var ogrenci in ogrenciler)
                {
                    Ogrencis.Add(ogrenci);
                }
            }
        }
        private async Task<List<Sinif>> GetSinifsFromDatabase()
        {
            List<Sinif> sinifs = new List<Sinif>();

            string sinifSorgu = $"select `sinif_adi` FROM `sinif` WHERE `ogretmen_id` = {id}";
            string results = WebServis.TestKBM(sinifSorgu);

            string[] sinifAdlar = results.Split('\n');

            foreach (var sinifAd in sinifAdlar)
            {
                string[] infoArray = sinifAd.Split('|');

                if (infoArray.Length >= 1)
                {
                    string sinif_adi = infoArray[0];

                    Sinif sinif = new Sinif
                    {
                        sinif_adi = sinif_adi
                    };

                    sinifs.Add(sinif);
                }
            }
            return sinifs;
        }
        private async Task<List<Ogrenci>> GetOgrencisAsync()
        {
            List<Ogrenci> ogrenciler = new List<Ogrenci>();

            string selectedItem = picker1.SelectedItem as string;
            string ogrenciSorgu = $"select `ogrenci_id`, `ogrenci_ad`,`veli_token` FROM `ogrenci_sinif` WHERE `sinif_ad` = '{selectedItem}'";
            string sonuclar = WebServis.TestKBM(ogrenciSorgu);
            string[] ogrenciAdlar = sonuclar.Split('\n');

            foreach (var ogrenciAd in ogrenciAdlar)
            {
                string[] infoArray = ogrenciAd.Split('|');

                if (infoArray.Length >= 3)
                {
                    int ogrenci_id = int.Parse(infoArray[0].Trim());
                    string ogrenci_ad = infoArray[1];
                    string ogrenci_token = infoArray[2];

                    Ogrenci ogrenci = new Ogrenci
                    {
                        Ogrenci_Id = ogrenci_id,
                        Ogrenci_Ad = ogrenci_ad,
                        Veli_Token = ogrenci_token
                    };
                    ogrenciler.Add(ogrenci);
                }
            }
            return ogrenciler;
        }
        private async Task SendFCMNotification(string title, string body, List<string> registrationIds)
        {
            try
            {
                string serverKey = "AAAAmdOfkLw:APA91bELODJ91sx7tz0i5R29Z6ROzSZYInszcXLveETOCXWrOd3Qa7RFf1RNSX46Mb2h4fbGJKwkddgmdIYzebMGjvIcVHyU-Wtu8h0-2b-HqtWilJZgZ6RYXvohyTKGw9SZlU_P3kdW"; // FCM sunucu anahtarınızı buraya yerleştirin
                string fcmUrl = "https://fcm.googleapis.com/fcm/send";

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + serverKey);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                var data = new
                {
                    notification = new
                    {
                        title,
                        body
                    },
                    registration_ids = registrationIds
                };

                var jsonData = JsonConvert.SerializeObject(data);

                HttpResponseMessage response = await client.PostAsync(fcmUrl, new StringContent(jsonData, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    // Bildirim başarıyla gönderildiğinde yapılacak işlemler
                    Console.WriteLine("Bildirim gönderildi.");
                }
                else
                {
                    // Bildirim gönderme başarısız olduğunda yapılacak işlemler
                    Console.WriteLine("Bildirim gönderme hatası: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata oluştu: " + ex.Message);
            }
        }
        private void UpdateStudentsSelection()
        {
            foreach (var item in studentCollectionView.ItemsSource)
            {
                if (item is Ogrenci ogrenci)
                {
                    ogrenci.Selected = AllStudentsSelected;
                }
            }
        }
        private void chatEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Giriş metni uzunluğunu alın
            int currentLength = e.NewTextValue.Length;

            // Max karakter sayısı
            int maxLength = 400;

            // Geriye kalan karakter sayısını hesapla
            int remainingCharacters = maxLength - currentLength;

            // Etiketin metnini güncelle
            charCountLabel.Text = $"{remainingCharacters}/{maxLength}";

            // Eğer sınırlama aşıldıysa, girişi kırpın
            if (currentLength > maxLength)
            {
                string trimmedText = e.NewTextValue.Substring(0, maxLength);
                chatEntry.Text = trimmedText;
            }
        }
        private void SelectAllStudent_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            bool isChecked = e.Value;

            foreach (var item in studentCollectionView.ItemsSource)
            {
                if (item is Ogrenci ogrenci)
                {
                    ogrenci.Selected = isChecked;
                }
            }
        }
        private void SaveChatButton(object sender, EventArgs e)
        {
            string yorum_avaible = "true";
            string download_durum = "false";
            string paylasim_turu = "chat";
            string selectedItem = picker1.SelectedItem as string;
            string aciklama = chatEntry.Text;
            string tartisma = chatTopicEntry.Text;
            // Paylaşım verisini veritabanına ekleme kısmı.
            DateTime now = DateTime.Now;
            string nowString = now.ToString("yyyy-MM-dd HH:mm:ss");
            WebServis.TestKBM($"INSERT INTO `paylasim` (`kullanici_id`, `kullanici_adi`, `paylasim_adi`, `begeni_sayisi`, `aciklama`, `yorum_avaible`, `downloadable`, `paylasim_turu`, `paylasilan_sinif`, `zaman`) VALUES ({id}, '{fullName}', '{aciklama}', 0, '{tartisma}','{yorum_avaible}','{download_durum}', '{paylasim_turu}', '{selectedItem}', '{nowString}')");

            List<Ogrenci> selectedStudents = new List<Ogrenci>();
            foreach (var ogrenci in Ogrencis)
            {
                if (ogrenci.Selected)
                    selectedStudents.Add(ogrenci);
            }
            // Seçili öğrenciler için insert işlemi yapma.
            foreach (var ogrenci in selectedStudents)
            {
                string query = $"select `post_id` FROM `paylasim` WHERE `kullanici_id` = {id} AND `kullanici_adi` = '{fullName}' AND `paylasim_adi` = '{aciklama}' AND `aciklama` = '{tartisma}' AND `paylasilan_sinif` = '{selectedItem}'";
                string paylasimId = WebServis.TestKBM(query);
                string selectedOgrenciAd = ogrenci.Ogrenci_Ad;
                int selectedOgrenciId = ogrenci.Ogrenci_Id;
                string selectedOgrenciToken = ogrenci.Veli_Token;
                tokens.Add(selectedOgrenciToken);
                string insertPaylasimOgrenciQuery = $"INSERT INTO `paylasim_ogrenci`(`paylasim_id`, `paylasim_ad`, `ogrenci_id`, `ogrenci_ad`, `sinif_ad`, `ogretmen_id`, `ogretmen_ad`, `aciklama`, `yorum_avaible`, `downloadable`, `paylasim_turu`, `zaman`) VALUES ('{paylasimId}','{aciklama}','{selectedOgrenciId}', '{selectedOgrenciAd}', '{selectedItem}', {id}, '{fullName}', '{tartisma}','{yorum_avaible}','{download_durum}', '{paylasim_turu}', '{nowString}') ";
                WebServis.TestKBM(insertPaylasimOgrenciQuery);

                string yeniBaslik = "Bir Yeni Bildirim";
                string yeniIcerik = $"{fullName} bir BİLDİRİ paylaştı.";

                SendFCMNotification(yeniBaslik, yeniIcerik, tokens);
            }
            Navigation.PushModalAsync(new OgretmenAnaSayfa());
        }
        public bool AllStudentsSelected
        {
            get { return _allStudentsSelected; }
            set
            {
                if (_allStudentsSelected != value)
                {
                    _allStudentsSelected = value;
                    OnPropertyChanged(nameof(AllStudentsSelected));

                    // Tüm öğrencilerin durumunu güncelle
                    UpdateStudentsSelection();
                }
            }
        }
    }
}