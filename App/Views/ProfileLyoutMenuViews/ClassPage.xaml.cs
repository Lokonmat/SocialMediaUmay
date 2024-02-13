using App.Models;
using App.Views.PopUpPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.ProfileLyoutMenuViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClassPage : ContentPage
    {
        string kullanici_id = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        string mail = Preferences.Get("Mail", "");
        string phoneNumber = Preferences.Get("PhoneNumber", "");
        string tableName = Preferences.Get("Type", "");
        public ObservableCollection<Sinif> Classes { get; set; }
        public class Sinif
        {
            public int Class_Id { get; set; }
            public string Teacher_Id { get; set; }
            public string Parent_Id { get; set; }
            public string Class_Name { get; set; }
            public string Class_Code { get; set; }
            public string Post_Name { get; set; }
        }
        public ClassPage()
        {
            InitializeComponent();

            // Initialize the ObservableCollection
            Classes = new ObservableCollection<Sinif>();

            // Set the BindingContext
            BindingContext = this;

            UpdateClassAsync();
        
        }        
        private async Task UpdateClassAsync()
        {
            List<Sinif> sinifs = await GetSinifsFromDatabase();

            // Mevcut verileri temizle
            Classes.Clear();

            foreach (var post in sinifs)
            {
                Classes.Add(post);
            }
            OnPropertyChanged(nameof(Classes));
        }
        private async Task<List<Sinif>> GetSinifsFromDatabase()
        {
            List<Sinif> sinifs = new List<Sinif>();

            try
            {
                string query = $"select `sinif_id`, `sinif_adi`, `sinif_kod`, `paylasim_adi` FROM `sinif` WHERE `ogretmen_id` = {kullanici_id}";
                string results = WebServis.TestKBM(query);

                string[] classInfos = results.Split('\n');
                foreach (var classInfo in classInfos)
                {
                    if (string.IsNullOrWhiteSpace(classInfo))
                        continue;

                    string[] infoArray = classInfo.Split('|');

                    if (classInfo.Length >= 4)
                    {
                        int classId = int.Parse(infoArray[0].Trim());
                        string class_name = infoArray[1].Trim();
                        string class_code = infoArray[2].Trim();
                        string post_name = infoArray[3].Trim();

                        Sinif sinif = new Sinif
                        {
                            Class_Id = classId,
                            Class_Name = class_name,
                            Class_Code = class_code,
                            Post_Name = post_name
                        };

                        sinifs.Add(sinif);
                    }
                }
            }
            catch(Exception ex)
            {

            }            

            return sinifs;
        }
        private async void DeleteClass_Clicked(object sender, EventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.BindingContext is Sinif sinif)
            {
                string selectedId = sinif.Class_Code;
                string query = $"DELETE FROM `sinif` WHERE `sinif_kod`= '{selectedId}'";
                string query2 = $"DELETE FROM `veli_sinif` WHERE `sinif_kod`= '{selectedId}'";
                string result = WebServis.TestKBM(query);
                string result2 = WebServis.TestKBM(query2);
            }
            await UpdateClassAsync();
        }
        private async void AddClass_Clicked(object sender, EventArgs e)
        {
            string sinifAdi = classNameEntry.Text;

            // Rakamlar ve harfler içeren 6 karakterli rastgele kod oluşturma
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string sinif_kod = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());

            string sorgu = $"INSERT INTO `sinif`(`ogretmen_id`,`ogretmen_adi`, `sinif_adi`, `sinif_kod` ) VALUES ({kullanici_id},'{fullName}','{sinifAdi}','{sinif_kod}')";
            WebServis.TestKBM(sorgu);

            sinifAdi = "";

            await UpdateClassAsync();
        }
        private void OnLabelTapped(object sender, EventArgs e)
        {
            if (sender is Label studentButton && studentButton.BindingContext is Sinif sinif)
            {
                string selectedCode = sinif.Class_Code;
                Preferences.Set("ClassCode", $"{selectedCode}");
                Navigation.PushModalAsync(new StudentPage());
            }
        }
        private void OnLabelTapped2(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            Clipboard.SetTextAsync(label.Text);
        }
    }
}