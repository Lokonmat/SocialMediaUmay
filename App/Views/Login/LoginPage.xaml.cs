using App.Utils;
using App.Views.OgretmenPages;
using App.Views.VeliPages;
using Plugin.FirebasePushNotification;
using System;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);

            userMailEntry.Text = Preferences.Get("RememberMail", string.Empty);
            passwordEntry.Text = Preferences.Get("RememberPassword", string.Empty);
            rememberSwitch.IsToggled = Preferences.Get("RememberSwitch", false);

            ogretmenCheckbox.CheckedChanged += (sender, e) =>
            {
                if (ogretmenCheckbox.IsChecked)
                {
                    // Öğretmen seçildiyse, veli seçeneğini iptal et
                    veliCheckbox.IsChecked = false;
                }
            };
            veliCheckbox.CheckedChanged += (sender, e) =>
            {
                if (veliCheckbox.IsChecked)
                {
                    // Veli seçildiyse, öğretmen seçeneğini iptal et
                    ogretmenCheckbox.IsChecked = false;
                }
            };
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Versiyon notlarını burada yerleştirin
            string versionNotes = "Bu sürümün notları:\n\n+ Uygulamada tekli olarak resim yüklemeye, PDF , Word belgesi, Excel belgesi paylaşmaya imkan tanır.\n+ Twitter gibi bir yazışma başlatma mevcut \n+ Öğretmenler paylaşım yaparken kendi sınıflarından öğrenci seçebilir.\n+ Gerçek zamanlı mesajlaşmaya imkan sağlar.\n+ Resimlere yorum yapılabilir.\n- Videoları yüklediğinde zaman sınırı olayı sorun çıkardığı için 50MB sınırlaması getirdim.\n- Bildirim sistemi servis sağlayıcının düzensiz çalışmasından dolayı çalışmıyor çalışma devam ediyor.";

            // DisplayAlert ile versiyon notlarını göster
            await DisplayAlert("Yeni Sürüm", versionNotes, "Tamam");
        }
        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            // Kullanıcının girdiği mail ve şifre bilgilerini al
            string Mail = userMailEntry.Text;
            string Password = passwordEntry.Text;
            string tableName = "";

            string deviceModel = DeviceInfo.Model;
            string deviceManufacturer = DeviceInfo.Manufacturer;
            string deviceVersionString = DeviceInfo.VersionString;
            DateTime now = DateTime.Now;
            string loginDate = now.ToString("yyyy-MM-dd HH:mm:ss");
            string token = CrossFirebasePushNotification.Current.Token;

            // Web servisi için connection string ve tablo adı
            string ogretmenTableName = "ogretmen";
            string veliTableName = "veli";

            // Checkbox durumlarını al
            bool ogretmenChecked = ogretmenCheckbox.IsChecked;
            bool veliChecked = veliCheckbox.IsChecked;
            if (ogretmenChecked)
            {
                tableName = ogretmenTableName;
            }
            else if (veliChecked)
            {
                tableName = veliTableName;
            }
            else
            {
                await DisplayAlert("Hata", "Lütfen durumunuzu seçin.", "Tamam");
                return;
            }

            // Tablo seçildi, giriş yapma denemesi
            string sorgu = $"select `Id`, `FullName`, `Mail`, `Password`, `PhoneNumber`, `ProfilePic`, `ogrenci_id`" +
                $"FROM {tableName} WHERE Mail = '" + userMailEntry.Text + "' AND Password = '" + passwordEntry.Text + "' ";
            string kullanici = WebServis.TestKBM(sorgu);

            if (!string.IsNullOrEmpty(kullanici))
            {
                string[] kullaniciBilgileri = kullanici.Split('|');

                Preferences.Set("Id", kullaniciBilgileri[0]);
                Preferences.Set("FullName", $"{kullaniciBilgileri[1]}");
                Preferences.Set("Mail", $"{kullaniciBilgileri[2]}");
                Preferences.Set("Password", $"{kullaniciBilgileri[3]}");
                Preferences.Set("PhoneNumber", $"{kullaniciBilgileri[4]}");
                Preferences.Set("ProfilPicture", $"{kullaniciBilgileri[5]}");
                Preferences.Set("ogrenci_id", $"{kullaniciBilgileri[6]}");
                Preferences.Set("Type", tableName);
                User.userName = kullaniciBilgileri[1];

                if (ogretmenChecked)
                {
                    string combinedQuery = $@"
                                            INSERT INTO `giris_tarihce` (
                                                `GirisYapanMail`, 
                                                `GirisYapanTelNo`, 
                                                `GirisYapanTelFirma`, 
                                                `GirisYapanTelModel`, 
                                                `GirisYapanVersiyonNo`,
                                                `GirisYapanToken`, 
                                                `GirisTarih`
                                            ) 
                                            SELECT 
                                                '{kullaniciBilgileri[2]}',
                                                '{kullaniciBilgileri[4]}',
                                                '{deviceManufacturer}',
                                                '{deviceModel}',
                                                '{deviceVersionString}',
                                                '{token}',
                                                '{loginDate}'
                                            FROM DUAL
                                            ON DUPLICATE KEY UPDATE
                                                `GirisYapanMail` = VALUES(`GirisYapanMail`),
                                                `GirisYapanTelNo` = VALUES(`GirisYapanTelNo`),
                                                `GirisYapanTelFirma` = VALUES(`GirisYapanTelFirma`),
                                                `GirisYapanTelModel` = VALUES(`GirisYapanTelModel`),
                                                `GirisYapanVersiyonNo` = VALUES(`GirisYapanVersiyonNo`),
                                                `GirisYapanToken` = VALUES(`GirisYapanToken`),
                                                `GirisTarih` = VALUES(`GirisTarih`);

                                            UPDATE `ogretmen` 
                                            SET `ActiveToken`='{token}' 
                                            WHERE `Mail` = '{kullaniciBilgileri[2]}' AND `Password`= '{kullaniciBilgileri[3]}';
                                        ";
                    WebServis.TestKBM(combinedQuery);
                    App.Current.MainPage = new NavigationPage(new OgretmenAnaSayfa());
                }
                else if (veliChecked)
                {
                    string combinedQuery = $@"
                                            INSERT INTO `giris_tarihce` (
                                                `GirisYapanMail`, 
                                                `GirisYapanTelNo`, 
                                                `GirisYapanTelFirma`, 
                                                `GirisYapanTelModel`, 
                                                `GirisYapanVersiyonNo`,
                                                `GirisYapanToken`, 
                                                `GirisTarih`
                                            ) 
                                            VALUES (
                                                '{kullaniciBilgileri[2]}',
                                                '{kullaniciBilgileri[4]}',
                                                '{deviceManufacturer}',
                                                '{deviceModel}',
                                                '{deviceVersionString}',
                                                '{token}',
                                                '{loginDate}'
                                            );

                                            UPDATE `veli` 
                                            SET `ActiveToken`='{token}' 
                                            WHERE `Mail` = '{kullaniciBilgileri[2]}' AND `Password`= '{kullaniciBilgileri[3]}';
                                        ";

                    WebServis.TestKBM(combinedQuery);

                    if (kullaniciBilgileri[6] == "")
                    {
                        // OgrenciKayit ekranını aç
                        App.Current.MainPage = new NavigationPage(new VeliOgrenciSecmeSayfa());
                    }
                    else
                    {
                        // ogrenci_id değeri dolu ise HomeTabbedPage aç
                        App.Current.MainPage = new NavigationPage(new VeliAnaSayfa());
                    }
                }
            }
            else
            {
                await DisplayAlert("Hata", "Mail veya şifre hatalı.", "Tamam");
            }
        }
        private void RegisterSelectButton(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Select());
        }
        private void RememberSwitch(object sender, ToggledEventArgs e)
        {
            if (rememberSwitch.IsToggled == true)
            {
                Preferences.Set("RememberMail", userMailEntry.Text);
                Preferences.Set("RememberPassword", passwordEntry.Text);
                Preferences.Set("RememberSwitch", rememberSwitch.IsToggled);
            }
            else
            {
                Preferences.Remove("RememberedMail");
                Preferences.Remove("RememberedPassword");
                Preferences.Remove("RememberSwitch");
            }
        }
        private void OnLabelTapped(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("http://www.sekizdesekiz.com"));
        }
        private void PasswordEye(object sender, EventArgs e)
        {
            if (passwordEntry.IsPassword == true)
            {
                passwordEye.Source = "eye.png";
                passwordEntry.IsPassword = false;
            }
            else
            {
                passwordEye.Source = "closedeye.png";
                passwordEntry.IsPassword = true;
            }

        }       
    }
}
