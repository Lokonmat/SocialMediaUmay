using App.Views.Login;
using App.Views.ProfileLyoutMenuViews;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.VeliPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VeliProfilSayfa : ContentPage
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

        public VeliProfilSayfa()
        {
            InitializeComponent();
            appFolderPath = Path.Combine(FileSystem.AppDataDirectory, "App");
            mediaFolderPath = Path.Combine(appFolderPath, "Media");
            profilePictureFolderPath = Path.Combine(mediaFolderPath, "ProfilePicture");
            postFolderPath = Path.Combine(mediaFolderPath, "PostPhoto");
            // Klasörleri oluşturun
            Directory.CreateDirectory(appFolderPath);
            Directory.CreateDirectory(mediaFolderPath);
            Directory.CreateDirectory(profilePictureFolderPath);
            Directory.CreateDirectory(postFolderPath);
            // Profil resmini cihaza indirin sadece ilk açılışta yapın
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
        private void LogoutButton(object sender, EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new LoginPage());
        }
        private void ListClass_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new VeliOgrenciSecmeSayfa());
        }
        private void OgrenciButton(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new VeliOgrenciSecmeSayfa());
        }
        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new ProfileSetting());
        }
        private void ProfilPage_Clicked(object sender, EventArgs e)
        {
            
        }
        private void VeliAnaSayfa_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
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