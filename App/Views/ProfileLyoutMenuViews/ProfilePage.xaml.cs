using App.Models;
using App.Views.Login;
using App.Views.OgretmenPages.Paylasimlar.Resimler;
using App.Views.OgretmenPages.Paylasimlar;
using App.Views.PopUpPages;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using App.Views.ChatPages;

namespace App.Views.ProfileLyoutMenuViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        string userId = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        string mail = Preferences.Get("Mail", "");
        string tableName = Preferences.Get("Type", "");
        string ftpServerUrl = App.ftpServerUrl;
        string ftpUserName = App.ftpUserName;
        string ftpPassword = App.ftpPassword;
        private string profilePicName;
        private string localFilePath;
        private string appFolderPath;
        private string mediaFolderPath;
        private string profilePictureFolderPath;
        private string postFolderPath;
        private string remoteProfileFilePath;        
        private int currentLoadedCount = 3;
        private int offset = 5;

        public ObservableCollection<ImagePost> Posts { get; set; }
        public ProfilePage()
        {
            InitializeComponent();
            // App, Media ve ProfilePicture klasörlerinin yollarını alın
            appFolderPath = Path.Combine(FileSystem.AppDataDirectory, "App");
            mediaFolderPath = Path.Combine(appFolderPath, "Media");
            profilePictureFolderPath = Path.Combine(mediaFolderPath, "ProfilePicture");
            postFolderPath = Path.Combine(mediaFolderPath, "PostPhoto");
            // Klasörleri oluşturun
            Directory.CreateDirectory(appFolderPath);
            Directory.CreateDirectory(mediaFolderPath);
            Directory.CreateDirectory(profilePictureFolderPath);
            Directory.CreateDirectory(postFolderPath);
            DownloadProfilePictureIfNeeded();           
            profileImageButton.Source = ImageSource.FromFile(localFilePath);
            userNameLabel.Text = fullName;            
        }
        private async void DownloadProfilePictureIfNeeded()
        {
            profilePicName = WebServis.TestKBM($"select `ProfilePic` FROM `{tableName}` WHERE `Id`={userId}");
            remoteProfileFilePath = $"/ProfilPicture/{tableName}/{mail}_{profilePicName}";
            localFilePath = Path.Combine(profilePictureFolderPath, profilePicName);

            // Cihazda profil resmi yoksa veya güncel değilse sunucudan indirin
            if (string.IsNullOrEmpty(profilePicName) || !File.Exists(localFilePath))
            {
                await DownloadProfilePictureFromFtp(ftpServerUrl, ftpUserName, ftpPassword, remoteProfileFilePath);
            }
        }
        private async void ProfileImageButton(object sender, EventArgs e)
        {
            try
            {
                // Galeriden bir fotoğraf seçmek için fotoğraf seçiciyi açın
                FileResult fileResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Fotoğraf Seç"
                });

                if (fileResult != null)
                {
                    // Seçilen fotoğrafın dosya yolunu alın
                    string selectedImagePath = fileResult.FullPath;
                    string selectedImageName = fileResult.FileName;

                    // FTP sunucusuna fotoğrafı yollayın
                    bool result = Utils.Dosya.Gonder(selectedImagePath, mail, tableName);

                    if (result)
                    {
                        await DisplayAlert("Başarılı", "Fotoğraf başarıyla yüklendi.", "Tamam");

                        // Eski profil resmini silin
                        DeleteOldProfilePicture();

                        // Yeni profil resmini kaydedin
                        string newProfilePicPath = Path.Combine(profilePictureFolderPath, selectedImageName);
                        File.Copy(selectedImagePath, newProfilePicPath, true);

                        // Yeni resmi ekranda gösterin
                        UpdateProfilePicture(newProfilePicPath, selectedImageName);

                        // Resim adını güncelleyin
                        profilePicName = selectedImageName;

                        // Sunucudaki profil resmini de güncelleyin
                        UpdateProfileOnServer();
                    }
                    else
                    {
                        await DisplayAlert("Hata", "Fotoğraf yüklenirken bir hata oluştu.", "Tamam");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }
        private void DeleteOldProfilePicture()
        {
            // Eski profil resmini silin
            if (!string.IsNullOrEmpty(profilePicName))
            {
                string oldProfilePicPath = Path.Combine(profilePictureFolderPath, profilePicName);
                File.Delete(oldProfilePicPath);
            }
        }
        private void UpdateProfilePicture(string newProfilePicPath, string selectedImageName)
        {
            // Yeni resmi ekranda gösterin
            localFilePath = newProfilePicPath;
            profileImageButton.Source = ImageSource.FromFile(localFilePath);
        }
        private void UpdateProfileOnServer()
        {
            // Sunucudaki profil resmini de güncelleyin
            WebServis.TestKBM($"UPDATE `{tableName}` SET `ProfilePic`='{profilePicName}' WHERE `Id`='{userId}'");
        }
        private async void BtnArti_Clicked(object sender, EventArgs e)
        {
            var popupContent = new StackLayout
            {
                Children =
                {
                    new Button { Text = "Resim", Command = new Command(() => HandleContentClick("İçerik 1")) },
                    new Button { Text = "PDF", Command = new Command(() => HandleContentClick("İçerik 2")) },
                    new Button { Text = "Video", Command = new Command(() => HandleContentClick("İçerik 3")) },
                    new Button { Text = "Word", Command = new Command(() => HandleContentClick("İçerik 4")) },
                    new Button { Text = "Excel", Command = new Command(() => HandleContentClick("İçerik 5")) },
                    new Button { Text = "Tartışma", Command = new Command(() => HandleContentClick("İçerik 6")) }
                }
            };

            var popupPage = new PopupPage(popupContent);
            await Navigation.PushModalAsync(popupPage);
        }
        private async void HandleContentClick(string content)
        {
            Page nextPage = null;

            switch (content)
            {
                case "İçerik 1":
                    nextPage = new ResimPaylasimSayfasi();
                    break;
                case "İçerik 2":
                    nextPage = new PDFPaylasimSayfasi();
                    break;
                case "İçerik 3":
                    nextPage = new VideoPaylasimSayfasi();
                    break;
                case "İçerik 4":
                    nextPage = new WordPaylasimSayfasi();
                    break;
                case "İçerik 5":
                    nextPage = new ExelPaylasimSayfasi();
                    break;
                case "İçerik 6":
                    nextPage = new SohbetPaylasimSayfasi();
                    break;
                default:
                    break;
            }
            if (nextPage != null)
            {
                await Navigation.PushModalAsync(nextPage);
            }
        }
        private void ProfilPage_Clicked(object sender, EventArgs e)
        {
            
        }
        private void OgretmenAnaSayfa_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
        private void ClassButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new ClassPage());
        }
        private void MessageButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new MessagesPage());
        }
        private void settingButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new ProfileSetting());
        }
        private void Logout_Clicked(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new LoginPage());
        }
        private async Task DownloadProfilePictureFromFtp(string ftpServerUrl, string ftpUserName, string ftpPassword, string remoteFilePath)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                    // Sunucudan resmi indirin ve cihazda geçici bir dosyaya kaydedin
                    string tempFilePath = FileSystem.CacheDirectory + "temp_profile_pic.png";
                    await client.DownloadFileTaskAsync(new Uri(ftpServerUrl + remoteFilePath), tempFilePath);

                    // Eski profil resmini silin
                    if (!string.IsNullOrEmpty(profilePicName))
                    {
                        string oldProfilePicPath = Path.Combine(profilePictureFolderPath, profilePicName);
                        File.Delete(oldProfilePicPath);
                    }

                    // Yeni profil resmini kaydedin ve güncel resim adını ayarlayın
                    File.Copy(tempFilePath, localFilePath, true);
                    profilePicName = Path.GetFileName(localFilePath);

                    // Resmi ekranda gösterin
                    profileImageButton.Source = ImageSource.FromFile(localFilePath);
                }
            }
            catch (Exception ex)
            {
                // Hata yönetimi yapabilirsiniz
            }
        }

    }
}