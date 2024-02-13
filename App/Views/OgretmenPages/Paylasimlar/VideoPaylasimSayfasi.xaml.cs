using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
using App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using UmayAppNew;
using System.Collections.ObjectModel;
using App.ViewModels;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace App.Views.OgretmenPages.Paylasimlar
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPaylasimSayfasi : ContentPage
    {
        string id = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        private string selectedVideoName;
        private string selectedVideoPath;
        private bool _allStudentsSelected;
        private PicSelectPageViewModel viewModel;
        private ObservableCollection<Ogrenci> Ogrencis = new ObservableCollection<Ogrenci>();
        List<string> tokens = new List<string>();

        public VideoPaylasimSayfasi()
        {
            InitializeComponent();
            viewModel = new PicSelectPageViewModel();
            BindingContext = viewModel;
            studentCollectionView.ItemsSource = Ogrencis;
            LoadSinifPicker();
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
        private void download_switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (download_switch.IsToggled == true)
            {
                download_switch_label.Text = "Evet";
            }
            else
            {
                download_switch_label.Text = "Hayır";
            }
        }
        private void comment_switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (comment_switch.IsToggled == true)
            {
                comment_switch_label.Text = "Evet";
            }
            else
            {
                comment_switch_label.Text = "Hayır";
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
        private async void PickVideo(object sender, EventArgs e)
        {
            try
            {
                FileResult fileResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Videos
                });

                if (fileResult != null)
                {
                    var videoStream = await fileResult.OpenReadAsync();
                    long fileSizeInBytes = videoStream.Length;
                    // Dosya boyutunu megabyte cinsine dönüştürme
                    double fileSizeInMB = fileSizeInBytes / (1024 * 1024.0);

                    if (fileSizeInMB > 50)
                    {
                        // Dosya boyutu 50MB'den büyük olduğunda uyarı ver
                        await DisplayAlert("Uyarı","Dosya boyutu 50MB'den büyük. Lütfen daha küçük bir dosya seçin.","Tamam");
                    }
                    else
                    {
                        selectedVideoPath = fileResult.FullPath;

                        // Seçilen videoyu MediaElement'e atama
                        videoPlayer.Source = selectedVideoPath;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }
        private async void SaveVideoButton(object sender, EventArgs e)
        {
            try
            {
                string yorum_avaible = comment_switch.IsToggled ? "true" : "false";
                string download_durum = download_switch.IsToggled ? "true" : "false";
                string paylasimTuru = "video";
                string selectedItem = picker1.SelectedItem as string;
                string aciklama = aciklamaEntry.Text;
                string selectedVideoPathh = this.selectedVideoPath;
                string selectedVideoName = Path.GetFileName(selectedVideoPath);

                bool result = Utils.Dosya.PaylasimGonder(selectedVideoPathh, selectedItem);

                // Paylaşım verisini veritabanına ekleyin
                DateTime now = DateTime.Now;
                string nowString = now.ToString("yyyy-MM-dd HH:mm:ss");
                WebServis.TestKBM($"INSERT INTO `paylasim` (`kullanici_id`, `kullanici_adi`, `paylasim_adi`, `begeni_sayisi`, `aciklama`, `yorum_avaible`, `downloadable`, `paylasim_turu`, `paylasilan_sinif`, `zaman`) VALUES ({id}, '{fullName}', '{selectedVideoName}', 0, '{aciklama}', '{yorum_avaible}', '{download_durum}', '{paylasimTuru}', '{selectedItem}', '{nowString}')");

                List<Ogrenci> selectedStudents = new List<Ogrenci>();
                foreach (var ogrenci in Ogrencis)
                {
                    if (ogrenci.Selected)
                        selectedStudents.Add(ogrenci);
                }

                // Seçili öğrenciler için insert işlemi yapma
                foreach (var ogrenci in selectedStudents)
                {
                    string query = $"select `post_id` FROM `paylasim` WHERE `kullanici_id` = {id} AND `kullanici_adi` = '{fullName}' AND `paylasim_adi` = '{selectedVideoName}' AND `aciklama` = '{aciklama}' AND `paylasilan_sinif` = '{selectedItem}'";
                    string paylasimId = WebServis.TestKBM(query);
                    string selectedOgrenciAd = ogrenci.Ogrenci_Ad;
                    int selectedOgrenciId = ogrenci.Ogrenci_Id;
                    string selectedOgrenciToken = ogrenci.Veli_Token;

                    string insertPaylasimOgrenciQuery = $"INSERT INTO `paylasim_ogrenci`(`paylasim_id`, `paylasim_ad`, `ogrenci_id`, `ogrenci_ad`, `sinif_ad`, `ogretmen_id`, `ogretmen_ad`, `aciklama`, `yorum_avaible`, `downloadable`, `paylasim_turu`, `zaman`) VALUES ('{paylasimId}','{selectedVideoName}','{selectedOgrenciId}', '{selectedOgrenciAd}', '{selectedItem}', {id}, '{fullName}', '{aciklama}','{yorum_avaible}','{download_durum}', '{paylasimTuru}', '{nowString}') ";
                    WebServis.TestKBM(insertPaylasimOgrenciQuery);

                    tokens.Add(selectedOgrenciToken);

                    string yeniBaslik = "Bir Yeni Bildirim";
                    string yeniIcerik = $"{fullName} bir VİDEO paylaştı.";

                    await SendFCMNotification(yeniBaslik, yeniIcerik, tokens);
                }
                    await Navigation.PushAsync(new OgretmenAnaSayfa());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }
        private async Task SendFCMNotification(string title, string body, IEnumerable<string> registrationIds)
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

                Console.WriteLine("jsonData: " + jsonData);

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
                    await UpdateStudentList();
                }
            };

            // Başlangıçta öğrenci listesini güncelle
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