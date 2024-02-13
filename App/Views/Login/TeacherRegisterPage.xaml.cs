using App.Models;
using Plugin.FirebasePushNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TeacherRegisterPage : ContentPage
    {
        string Mail;
        string PhoneNumber;

        public TeacherRegisterPage()
        {
            InitializeComponent();
        }

        private void LoginPageButton(object sender, EventArgs e)
        {
            //Navigation.PushAsync(new LoginPage());
            Navigation.PopAsync();
        }

        async void Save_Ogretmen(Object sender, EventArgs e)
        {
            string tableName = "ogretmen";
            string Mail = TeacherMailEntry.Text;
            string PhoneNumber = TeacherPhoneEntry.Text;
            string SchoolCode = TeacherSchoolCodeEntry.Text;
            string deviceModel = DeviceInfo.Model;
            string deviceManufacturer = DeviceInfo.Manufacturer;
            string deviceVersionString = DeviceInfo.VersionString;
            DateTime now = DateTime.Now;
            string registerDate = now.ToString("yyyy-MM-dd HH:mm:ss");

            // Veritabanında e-posta kontrolü yapılıyor
            string emailQuery = $"select COUNT(*) FROM {tableName} WHERE `Mail` = '{Mail}'";
            string emailResult = WebServis.TestKBM(emailQuery);
            int emailCount = 0;
            int.TryParse(emailResult, out emailCount);

            // Veritabanında telefon numarası kontrolü yapılıyor
            string phoneQuery = $"select COUNT(*) FROM {tableName} WHERE `PhoneNumber` = '{PhoneNumber}'";
            string phoneResult = WebServis.TestKBM(phoneQuery);
            int phoneCount = 0;
            int.TryParse(phoneResult, out phoneCount);
             
            if (emailCount > 0)
            {
                // E-posta adresi veritabanında mevcutsa bildirim göster
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Hata", "Bu e-posta adresi zaten kayıtlıdır.", "Tamam");
                });
            }
            else if (phoneCount > 0)
            {
                // Telefon numarası veritabanında mevcutsa bildirim göster
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Hata", "Bu telefon numarası zaten kayıtlıdır.", "Tamam");
                });
            }
            else if(TeacherFullNameEntry.Text == null)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Hata", "Lütfen adınızı ve soyadınızı giriniz.", "Tamam");
                });
            }
            else if(TeacherPasswordEntry.Text == null)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Hata", "Lütfen şifre oluşturunuz.", "Tamam");
                });
            }
            else
            {
                // Hem e-posta hem de telefon numarası veritabanında mevcut değilse kayıt işlemini gerçekleştir
                string FullName = TeacherFullNameEntry.Text;
                string Password = TeacherPasswordEntry.Text;
                string token = CrossFirebasePushNotification.Current.Token;

                string insertQuery = $"INSERT INTO {tableName} (FullName, Mail, Password, PhoneNumber, SchoolCode, PhoneModel, RegisterDate, RegisterToken) VALUES ('{FullName}', '{Mail}', '{Password}', '{PhoneNumber}', '{SchoolCode}', '{deviceModel}', '{registerDate}', '{token}')";
                WebServis.TestKBM(insertQuery);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    var result = await DisplayAlert("Kayıt", "Kayıt Başarıyla Tamamlandı", "Tamam", "Çıkış");

                    if (result)
                    {
                        await Navigation.PushAsync(new LoginPage());
                    }
                });
            }
        }
    }
}