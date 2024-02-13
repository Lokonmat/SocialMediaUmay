using App.Models;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
	public partial class ExelPaylasimSayfasi : ContentPage
	{
        string id = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        private string selectedExelPath;
        private string selectedExelName;
        private bool _allStudentsSelected;
        private ObservableCollection<Ogrenci> Ogrencis = new ObservableCollection<Ogrenci>();
        List<string> tokens = new List<string>();
        public ExelPaylasimSayfasi ()
		{
			InitializeComponent ();
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
        private async void SaveExcelButton(object sender, EventArgs e)
        {
            string yorum_avaible = comment_switch.IsToggled ? "true" : "false";
            string paylasim_turu = "excel";
            string downloadabell = "true";
            string selectedItem = picker1.SelectedItem as string;
            string aciklama = aciklamaEntry.Text;
            string fileName = selectedExelName;
            string destinationPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            string fullPath = selectedExelPath;

            bool result = Utils.Dosya.PdfGonder(fullPath, selectedItem);
            DateTime now = DateTime.Now;
            string nowString = now.ToString("yyyy-MM-dd HH:mm:ss");
            WebServis.TestKBM($"INSERT INTO `paylasim` (`kullanici_id`, `kullanici_adi`, `paylasim_adi`, `begeni_sayisi`, `aciklama`, `yorum_avaible`, `downloadable`, `paylasim_turu`, `paylasilan_sinif`, `zaman`) VALUES ({id}, '{fullName}', '{fileName}', 0, '{aciklama}','{yorum_avaible}', '{downloadabell}', '{paylasim_turu}', '{selectedItem}', '{nowString}')");

            List<Ogrenci> selectedStudents = new List<Ogrenci>();
            foreach (var ogrenci in Ogrencis)
            {
                if (ogrenci.Selected)
                    selectedStudents.Add(ogrenci);
            }

            // Seçili öğrenciler için insert işlemi yapma
            foreach (var ogrenci in selectedStudents)
            {
                string query = $"select `post_id` FROM `paylasim` WHERE `kullanici_id` = {id} AND `kullanici_adi` = '{fullName}' AND `paylasim_adi` = '{fileName}' AND `aciklama` = '{aciklama}' AND `paylasilan_sinif` = '{selectedItem}'";
                string paylasimId = WebServis.TestKBM(query);
                string selectedOgrenciAd = ogrenci.Ogrenci_Ad;
                int selectedOgrenciId = ogrenci.Ogrenci_Id;
                string selectedOgrenciToken = ogrenci.Veli_Token;
                string insertPaylasimOgrenciQuery = $"INSERT INTO `paylasim_ogrenci`(`paylasim_id`, `paylasim_ad`, `ogrenci_id`, `ogrenci_ad`, `sinif_ad`, `ogretmen_id`, `ogretmen_ad`, `aciklama`, `yorum_avaible`, `downloadable`, `paylasim_turu`, `zaman`) VALUES ('{paylasimId}','{fileName}','{selectedOgrenciId}', '{selectedOgrenciAd}', '{selectedItem}', {id}, '{fullName}', '{aciklama}','{yorum_avaible}','{downloadabell}', '{paylasim_turu}', '{nowString}') ";
                WebServis.TestKBM(insertPaylasimOgrenciQuery);

                string yeniBaslik = "Bir Yeni Bildirim";
                string yeniIcerik = $"{fullName} bir EXCEL paylaştı.";
                tokens.Add(selectedOgrenciToken);
                await SendFCMNotification(yeniBaslik, yeniIcerik, tokens);
            }
            await Navigation.PushModalAsync(new OgretmenAnaSayfa());
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
        private async void PickExcel_Clicked(object sender, EventArgs e)
        {
            FileResult fileResult = await FilePicker.PickAsync();

            if (fileResult != null)
            {
                // Seçilen dosyanın uzantısını kontrol edin
                string extension = Path.GetExtension(fileResult.FileName).ToLower();

                if (extension == ".xlsx" || extension == ".xls")
                {
                    // Word belgesi seçildi, dosya yolu üzerinden işlemleri gerçekleştirin
                    selectedExelPath = fileResult.FullPath;
                    selectedExelName = fileResult.FileName;
                    exelNameLabel.Text = $"{fileResult.FileName}";
                    wordFrame.IsVisible = true;
                }
                else
                {
                    // Geçerli bir Word belgesi seçilmediği durumunda kullanıcıya bilgi verin
                    await DisplayAlert("Hata", "Lütfen bir Excel belgesi seçin.", "Tamam");
                }
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