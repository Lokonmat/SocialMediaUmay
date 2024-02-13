using App.Models;
using App.Views.ChatPages;
using App.Views.OrtakSayfalar;
using App.Views.PopUpPages;
using Plugin.FirebasePushNotification;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.VeliPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VeliAnaSayfa : ContentPage
    {
        string userId = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        string ftpServerUrl = App.ftpServerUrl;
        string ftpUserName = App.ftpUserName;
        string ftpPassword = App.ftpPassword;
        private string appFolderPath;
        private string mediaFolderPath;
        private string profilePictureFolderPath;
        private string postFolderPath;
        private string selectedPDFPath;
        private int currentLoadedCount = 3;
        private int offset = 5; // Başlangıç OFFSET değeri
        
        private ObservableCollection<ImagePost> userImages = new ObservableCollection<ImagePost>();
        private Dictionary<string, string> imageCache = new Dictionary<string, string>();
        private HashSet<int> likedPosts = new HashSet<int>();
        private HashSet<int> loadedImageIds = new HashSet<int>(); // Daha önce yüklenmiş resimlerin ID'lerini içerecek set

        public VeliAnaSayfa()
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
            // CollectionView'ın ItemsSource'unu userImages koleksiyonuna bağlayalım
            collectionView.ItemsSource = userImages;
            UpdateUserImagesAsync();
        }

        private void UpdateLikeCount(ImagePost imagePost, bool liked)
        {
            if (liked)
            {
                imagePost.begeni_sayisi++;
            }
            else
            {
                if (imagePost.begeni_sayisi > 0)
                {
                    imagePost.begeni_sayisi--;
                }
            }

            // PropertyChanged olayını tetikleme
            OnPropertyChanged(nameof(imagePost.begeni_sayisi));
        }
        private void CommentButton_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton commentButton && commentButton.BindingContext is ImagePost imagePost)
            {
                int selectedId = imagePost.paylasim_Id;
                string selectedName = imagePost.paylasim_adi;
                string comment = imagePost.Yorum_Mevcut;
                if(comment == "true")
                {
                    Preferences.Set("PostId", $"{selectedId}");
                    Preferences.Set("PostName", $"{selectedName}");
                    Navigation.ShowPopup(new Yorumlar());
                }
                else if(comment == "false")
                {
                    DisplayAlert("UYARI", "Öğretmeniniz içeriğe yorum yapılmasına onay vermedi.", "Tamam");
                }
                
            }
        }
        private void Messages_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new MessagesPageParent());
        }
        private void VeliAnaSayfa_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new VeliAnaSayfa();
        }
        private void ProfilPage_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new VeliProfilSayfa());
        }
        private void LikeButton_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton likeButton && likeButton.BindingContext is ImagePost imagePost)
            {
                int selectedId = imagePost.paylasim_Id;
                string selectedName = imagePost.paylasim_adi;

                string query = $"select * FROM `begeni` WHERE `begenen_kisi_ad` = '{fullName}' AND `paylasim_id` = {selectedId}";
                string result = WebServis.TestKBM(query);

                if (result == "")
                {
                    string query2 = $@"
                                        START TRANSACTION; 

                                        INSERT INTO `begeni`(`begenen_kisi_id`, `begenen_kisi_ad`, `paylasim_id`, `paylasim_ad`, `paylasim_icon`) 
                                        VALUES({userId}, '{fullName}', {selectedId}, '{selectedName}', 'LikeNew.png');

                                        UPDATE `paylasim` 
                                        SET `begeni_sayisi` = (SELECT COUNT(*) FROM `begeni` WHERE `paylasim_id` = {selectedId}) 
                                        WHERE `post_id` = {selectedId};
                                        UPDATE `paylasim_ogrenci` 
                                        SET `begeni_sayisi` = (SELECT COUNT(*) FROM `begeni` WHERE `paylasim_id` = {selectedId}) 
                                        WHERE `paylasim_id` = {selectedId};

                                        COMMIT;
                                    ";
                    string result2 = WebServis.TestKBM(query2);
                    likeButton.Source = "icon_like.png";
                    UpdateLikeCount(imagePost, true);
                    likedPosts.Add(selectedId);

                }
                else
                {
                    string query3 = $@"
                                        START TRANSACTION;

                                        DELETE FROM `begeni` WHERE `begenen_kisi_ad` = '{fullName}' AND `paylasim_id` = {selectedId};

                                        UPDATE `paylasim` 
                                        SET `begeni_sayisi` = (SELECT COUNT(*) FROM `begeni` WHERE `paylasim_id` = {selectedId}) 
                                        WHERE `post_id` = {selectedId};
                                        UPDATE `paylasim_ogrenci` 
                                        SET `begeni_sayisi` = (SELECT COUNT(*) FROM `begeni` WHERE `paylasim_id` = {selectedId}) 
                                        WHERE `paylasim_id` = {selectedId};

                                        COMMIT;
                                    ";
                    string result3 = WebServis.TestKBM(query3);
                    UpdateLikeCount(imagePost, false);
                    likeButton.Source = "icon_unlike.png";
                    likedPosts.Remove(selectedId);
                }

                // Change the icon based on like status
                var iconImage = likeButton.FindByName<Image>("likeIcon");
                if (iconImage != null)
                {
                    bool isLiked = likedPosts.Contains(selectedId);
                    iconImage.Source = isLiked ? "icon_like.png" : "icon_unlike.png";
                }
            }
        }
        private void likeCountLabel_Tapped(object sender, EventArgs e)
        {
            if (sender is Label lbl && lbl.BindingContext is ImagePost imagePost)
            {
                int selectedId = imagePost.id;
                string selectedName = imagePost.paylasim_adi;
                Navigation.PushModalAsync(new BegenenlerListesi(selectedId, selectedName));
            }

        }
        private async void OgretmenLabel_Tapped(object sender, EventArgs e)
        {
            if (sender is Label label && label.BindingContext is ImagePost teacher)
            {
                // Öğretmen bilgileri içeren yeni bir sayfa oluşturun
                var teacherProfilePage = new ConstructionPage();

                // Yeni sayfayı gezinme yığınına ekleyin
                await Navigation.PushModalAsync(teacherProfilePage);
            }
        }
        private async void LoadMore_Clicked(object sender, EventArgs e)
        {
            this.IsBusy = true;

            try
            {
                await Task.Run(async () =>
                {
                    // Arka planda uzun süren işlem
                    List<ImagePost> nextImagePosts = await GetNextImagePostsFromDatabase(offset);

                    // UI güncellemelerini ana iş parçacığına gönder
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        // Yeni resimleri ekle
                        foreach (var post in nextImagePosts)
                        {
                            if (!loadedImageIds.Contains(post.id))
                            {
                                userImages.Add(post);
                                loadedImageIds.Add(post.id);
                            }
                        }

                        // collectionView'e koleksiyonu güncelle
                        collectionView.ItemsSource = userImages;

                        // Her tıklamada OFFSET değerini artır
                        offset += 3; // Her seferinde 3 resim getir
                        this.IsBusy = false;
                    });
                });
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                Console.WriteLine("Hata: " + ex.Message);
                this.IsBusy = false;
            }
        }
        private async void DownloadButton_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton downloadButton && downloadButton.BindingContext is ImagePost imagePost)
            {                
                string download = imagePost.Download_Durum;
                if(download == "true")
                {
                    string selectedName = imagePost.paylasim_adi;
                    string selectedClassName = imagePost.sinif_adi;
                    string selectedType = imagePost.paylasim_turu;
                    string tamAd = $"{selectedClassName}_{selectedName}";
                    string ftpFilePath = $"{ftpServerUrl}Post/{tamAd}";

                    if (selectedType == "pdf")
                    {
                        string localFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{tamAd}.pdf");

                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
                        request.Method = WebRequestMethods.Ftp.DownloadFile;
                        request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                        try
                        {
                            FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();
                            Stream responseStream = response.GetResponseStream();

                            using (FileStream fileStream = File.Create(localFilePath))
                            {
                                responseStream.CopyTo(fileStream);
                            }

                            responseStream.Close();
                            response.Close();

                            OpenDocument(localFilePath);
                        }
                        catch (WebException ex)
                        {
                            // Hata durumunda işlemler
                        }
                    }
                    else if (selectedType == "word" || selectedType == "excel")
                    {
                        string localFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{tamAd}");

                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
                        request.Method = WebRequestMethods.Ftp.DownloadFile;
                        request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                        try
                        {
                            FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();
                            Stream responseStream = response.GetResponseStream();

                            using (FileStream fileStream = File.Create(localFilePath))
                            {
                                responseStream.CopyTo(fileStream);
                            }

                            responseStream.Close();
                            response.Close();

                            OpenDocument(localFilePath);
                        }
                        catch (WebException ex)
                        {
                            // Handle the exception
                        }
                    }
                    else if (selectedType == "image")
                    {
                        string localFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{tamAd}.jpg");

                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
                        request.Method = WebRequestMethods.Ftp.DownloadFile;
                        request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                        try
                        {
                            FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();
                            Stream responseStream = response.GetResponseStream();

                            using (FileStream fileStream = File.Create(localFilePath))
                            {
                                responseStream.CopyTo(fileStream);
                            }

                            responseStream.Close();
                            response.Close();

                            // Galeride resmi göstermek için kullanacağınız bir metodunuzu ekleyin
                            DisplayImageInGallery(localFilePath);
                        }
                        catch (WebException ex)
                        {
                            // Hata durumunu ele alın
                        }
                    }
                }
                else if(download == "false")
                {
                    await DisplayAlert("UYARI", "Öğretmeniniz içeriğin indirilmesine onay vermedi.",  "Tamam");
                }
            }
        }
        private async void OpenDocument(string filePath)
        {
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });            
        }
        private async void DisplayImageInGallery(string filePath)
        {
            try
            {
                // Dosyanın JPEG formatında olduğundan emin olun
                string extension = Path.GetExtension(filePath).ToLower();
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    // Dosyanın yerel depolama alanında bulunduğundan emin olun
                    if (File.Exists(filePath))
                    {
                        // Dosyayı galeride aç
                        await Launcher.OpenAsync(new OpenFileRequest
                        {
                            File = new ReadOnlyFile(filePath)
                        });
                    }
                    else
                    {
                        // Dosya yerel depolama alanında bulunamadı hatası
                        await DisplayAlert("Hata", "Dosya yerel depolama alanında bulunamadı.", "Tamam");
                    }
                }
                else
                {
                    // Desteklenmeyen dosya türü hatası
                    await DisplayAlert("Hata", "Desteklenmeyen dosya türü.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                // Genel bir hata durumu
                await DisplayAlert("Hata", $"Dosya açılırken bir hata oluştu: {ex.Message}", "Tamam");
            }
        }
        private async Task UpdateUserImagesAsync()
        {
            List<ImagePost> newImagePosts = await GetImagePostsFromDatabase();

            // Eğer koleksiyon daha önce boşaltılmadıysa boşaltın
            if (userImages.Count > 0)
                userImages.Clear();

            foreach (var post in newImagePosts)
            {
                userImages.Add(post);
                loadedImageIds.Add(post.id); // ID'yi listeye ekle
            }

            currentLoadedCount += 2;
        }
        private async Task<List<ImagePost>> GetImagePostsFromDatabase()
        {
            List<ImagePost> imagePosts = new List<ImagePost>();

            try
            {
                string selectedStudentId = WebServis.TestKBM($"select `ogrenci_id` FROM `veli` WHERE `Id` = {userId}");
                string query = $"select `paylasim_ogrenci_id`, `paylasim_id`, `paylasim_ad`, `ogrenci_id`, `ogrenci_ad`, `sinif_ad`, `ogretmen_id`, `ogretmen_ad`, `aciklama`, `begeni_sayisi`, `yorum_avaible`, `downloadable`, `paylasim_turu`, `zaman` FROM `paylasim_ogrenci` WHERE `ogrenci_id` = {selectedStudentId} ORDER BY `zaman` DESC LIMIT 5";

                string results = WebServis.TestKBM(query);

                string[] imageInfos = results.Split('\n');

                foreach (var imageInfo in imageInfos)
                {
                    if (string.IsNullOrWhiteSpace(imageInfo))
                        continue;

                    string[] infoArray = imageInfo.Split('|');

                    if (infoArray.Length >= 14)
                    {
                        int paylasim_student_id = int.Parse(infoArray[0].Trim());
                        int paylasim_id = int.Parse(infoArray[1].Trim());
                        string paylasim_ad = infoArray[2].Trim();
                        int ogrenci_id = int.Parse(infoArray[3].Trim());
                        string ogrenci_ad = infoArray[4].Trim();
                        string sinif_ad = infoArray[5].Trim();
                        int ogretmen_id = int.Parse(infoArray[6].Trim());
                        string ogretmen_ad = infoArray[7].Trim();
                        string aciklama = infoArray[8].Trim();
                        int begeni_sayis = int.Parse(infoArray[9].Trim());
                        string yorum_durumu = infoArray[10].Trim();
                        string indirme_durumu = infoArray[11].Trim();
                        string paylasim_turu = infoArray[12].Trim();
                        DateTime time = DateTime.Parse(infoArray[13].Trim());
                        string tamAd = $"{sinif_ad}_{paylasim_ad}";
                        string localImagePath = Path.Combine(postFolderPath, tamAd);
                        string remoteImagePath = $"{ftpServerUrl}Post/{tamAd}";
                        string begeniDurumSorgu = $"select * FROM `begeni` WHERE `begenen_kisi_ad` = '{fullName}' AND `paylasim_id` = {paylasim_id}";
                        string begeniDurum = WebServis.TestKBM(begeniDurumSorgu);
                        string begeniDurumu;

                        if (string.IsNullOrEmpty(begeniDurum))
                        {
                            begeniDurumu = "icon_unlike.png";
                        }
                        else
                        {
                            begeniDurumu = "icon_like.png";
                        }
                        if (paylasim_turu == "chat")
                        {
                            ImagePost imagePost = new ImagePost
                            {
                                id = paylasim_student_id,
                                paylasim_Id = paylasim_id,
                                paylasim_adi = paylasim_ad,
                                Ogrenci_Id = ogrenci_id,
                                Ogrenci_Ad = ogrenci_ad,
                                sinif_adi = sinif_ad,
                                Ogretmen_Id = ogretmen_id,
                                Ogretmen_Ad = ogretmen_ad,
                                aciklama = aciklama,
                                begeni_sayisi = begeni_sayis,
                                begeni_durum = begeniDurumu,
                                Yorum_Mevcut = yorum_durumu,
                                Download_Durum = indirme_durumu,
                                paylasim_turu = paylasim_turu,
                                zaman = time
                            };
                            imagePosts.Add(imagePost);
                        }
                        else if (paylasim_turu == "video")
                        {
                            // Uygulamanın özel depolama alanını alın
                            string localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                            // Tam lokal dosya yolunu oluşturun
                            string localFilePath = Path.Combine(localFolderPath, tamAd);

                            // FTP sunucusuna bağlanma işlemi
                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteImagePath);
                            request.Method = WebRequestMethods.Ftp.DownloadFile;
                            request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                            // İndirilen videoyu cihaza kaydetme işlemi
                            using (Stream ftpStream = request.GetResponse().GetResponseStream())
                            {
                                using (FileStream fileStream = File.Create(localFilePath))
                                {
                                    byte[] buffer = new byte[10240];
                                    int read;
                                    while ((read = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        fileStream.Write(buffer, 0, read);
                                    }
                                }
                            }
                            // ImagePost nesnesini oluşturun ve PostImage özelliğine resmi atayın
                            ImagePost imagePost = new ImagePost
                            {
                                id = paylasim_student_id,
                                paylasim_Id = paylasim_id,
                                paylasim_adi = paylasim_ad,
                                Ogrenci_Id = ogrenci_id,
                                Ogrenci_Ad = ogrenci_ad,
                                sinif_adi = sinif_ad,
                                Ogretmen_Id = ogretmen_id,
                                Ogretmen_Ad = ogretmen_ad,
                                aciklama = aciklama,
                                begeni_sayisi = begeni_sayis,
                                begeni_durum = begeniDurumu,
                                Yorum_Mevcut = yorum_durumu,
                                Download_Durum = indirme_durumu,
                                paylasim_turu = paylasim_turu,
                                zaman = time,
                                PostImage = localFilePath
                            };
                            imagePosts.Add(imagePost);
                        }
                        else
                        {
                            // Check if the local image file already exists
                            if (File.Exists(localImagePath))
                            {
                                // If it exists, use the local file instead of downloading from the FTP server
                                byte[] localImageBytes = File.ReadAllBytes(localImagePath);
                                string base64Image = Convert.ToBase64String(localImageBytes);

                                ImagePost imagePost = new ImagePost
                                {
                                    id = paylasim_student_id,
                                    paylasim_Id = paylasim_id,
                                    paylasim_adi = paylasim_ad,
                                    Ogrenci_Id = ogrenci_id,
                                    Ogrenci_Ad = ogrenci_ad,
                                    sinif_adi = sinif_ad,
                                    Ogretmen_Id = ogretmen_id,
                                    Ogretmen_Ad = ogretmen_ad,
                                    aciklama = aciklama,
                                    begeni_sayisi = begeni_sayis,
                                    begeni_durum = begeniDurumu,
                                    Yorum_Mevcut = yorum_durumu,
                                    Download_Durum = indirme_durumu,
                                    paylasim_turu = paylasim_turu,
                                    zaman = time,
                                    PostImage2 = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64Image))),
                                };
                                imagePosts.Add(imagePost);
                            }
                            else
                            {
                                // Eğer yoksa, FTP sunucusundan indir
                                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteImagePath);
                                request.Method = WebRequestMethods.Ftp.DownloadFile;
                                request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                                using (Stream ftpStream = response.GetResponseStream())
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        byte[] buffer = new byte[1024];
                                        int bytesRead;
                                        while ((bytesRead = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            ms.Write(buffer, 0, bytesRead);
                                        }
                                        string base64Image = Convert.ToBase64String(ms.ToArray());

                                        // İndirilen resmi yerel dosya sistemine kaydet
                                        File.WriteAllBytes(localImagePath, ms.ToArray());

                                        // FTP'den indirilen resmi ImagePost nesnesine atama yapın
                                        ImageSource ftpImageSource = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64Image)));

                                        // ImagePost nesnesini oluşturun ve PostImage2'ye resmi atayın
                                        ImagePost imagePost = new ImagePost
                                        {
                                            id = paylasim_student_id,
                                            paylasim_Id = paylasim_id,
                                            paylasim_adi = paylasim_ad,
                                            Ogrenci_Id = ogrenci_id,
                                            Ogrenci_Ad = ogrenci_ad,
                                            sinif_adi = sinif_ad,
                                            Ogretmen_Id = ogretmen_id,
                                            Ogretmen_Ad = ogretmen_ad,
                                            aciklama = aciklama,
                                            begeni_sayisi = begeni_sayis,
                                            begeni_durum = begeniDurumu,
                                            Yorum_Mevcut = yorum_durumu,
                                            Download_Durum = indirme_durumu,
                                            paylasim_turu = paylasim_turu,
                                            zaman = time,
                                            PostImage2 = ftpImageSource,
                                        };
                                        imagePosts.Add(imagePost);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata yönetimi yapabilirsiniz
            }

            return imagePosts;
        }
        private async Task<List<ImagePost>> GetNextImagePostsFromDatabase(int currentOffset)
        {
            List<ImagePost> imagePosts = new List<ImagePost>();
            try
            {
                string selectedStudentId = WebServis.TestKBM($"select `ogrenci_id` FROM `veli` WHERE `Id` = {userId}");
                string query = $"select `paylasim_ogrenci_id`, `paylasim_id`, `paylasim_ad`, `ogrenci_id`, `ogrenci_ad`, `sinif_ad`, `ogretmen_id`, `ogretmen_ad`, `aciklama`, `begeni_sayisi`, `yorum_avaible`, `downloadable`, `paylasim_turu`, `zaman` FROM `paylasim_ogrenci` WHERE `ogrenci_id` = {selectedStudentId}  ORDER BY `zaman` DESC LIMIT 5 OFFSET {currentOffset}";
                string results = WebServis.TestKBM(query);
                string[] imageInfos = results.Split('\n');
                foreach (var imageInfo in imageInfos)
                {
                    if (string.IsNullOrWhiteSpace(imageInfo))
                        continue;

                    string[] infoArray = imageInfo.Split('|');

                    if (infoArray.Length >= 14)
                    {
                        int paylasim_student_id = int.Parse(infoArray[0].Trim());
                        int paylasim_id = int.Parse(infoArray[1].Trim());
                        string paylasim_ad = infoArray[2].Trim();
                        int ogrenci_id = int.Parse(infoArray[3].Trim());
                        string ogrenci_ad = infoArray[4].Trim();
                        string sinif_ad = infoArray[5].Trim();
                        int ogretmen_id = int.Parse(infoArray[6].Trim());
                        string ogretmen_ad = infoArray[7].Trim();
                        string aciklama = infoArray[8].Trim();
                        int begeni_sayis = int.Parse(infoArray[9].Trim());
                        string yorum_durumu = infoArray[10].Trim();
                        string indirme_durumu = infoArray[11].Trim();
                        string paylasim_turu = infoArray[12].Trim();
                        DateTime time = DateTime.Parse(infoArray[13].Trim());
                        string tamAd = $"{sinif_ad}_{paylasim_ad}";
                        string localImagePath = Path.Combine(postFolderPath, tamAd);
                        string remoteImagePath = $"{ftpServerUrl}Post/{tamAd}";
                        string begeniDurumSorgu = $"select * FROM `begeni` WHERE `begenen_kisi_ad` = '{fullName}' AND `paylasim_id` = {paylasim_id}";
                        string begeniDurum = WebServis.TestKBM(begeniDurumSorgu);
                        string begeniDurumu;

                        if (string.IsNullOrEmpty(begeniDurum))
                        {
                            begeniDurumu = "icon_unlike.png";
                        }
                        else
                        {
                            begeniDurumu = "icon_like.png";
                        }
                        if (paylasim_turu == "chat")
                        {
                            ImagePost imagePost = new ImagePost
                            {
                                id = paylasim_student_id,
                                paylasim_Id = paylasim_id,
                                paylasim_adi = paylasim_ad,
                                Ogrenci_Id = ogrenci_id,
                                Ogrenci_Ad = ogrenci_ad,
                                sinif_adi = sinif_ad,
                                Ogretmen_Id = ogretmen_id,
                                Ogretmen_Ad = ogretmen_ad,
                                aciklama = aciklama,
                                begeni_sayisi = begeni_sayis,
                                begeni_durum = begeniDurumu,
                                Yorum_Mevcut = yorum_durumu,
                                Download_Durum = indirme_durumu,
                                paylasim_turu = paylasim_turu,
                                zaman = time
                            };
                            imagePosts.Add(imagePost);
                        }
                        else if (paylasim_turu == "video")
                        {
                            // Uygulamanın özel depolama alanını alın
                            string localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                            // Tam lokal dosya yolunu oluşturun
                            string localFilePath = Path.Combine(localFolderPath, tamAd);

                            // FTP sunucusuna bağlanma işlemi
                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteImagePath);
                            request.Method = WebRequestMethods.Ftp.DownloadFile;
                            request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                            // İndirilen videoyu cihaza kaydetme işlemi
                            using (Stream ftpStream = request.GetResponse().GetResponseStream())
                            {
                                using (FileStream fileStream = File.Create(localFilePath))
                                {
                                    byte[] buffer = new byte[10240];
                                    int read;
                                    while ((read = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        fileStream.Write(buffer, 0, read);
                                    }
                                }
                            }
                            // ImagePost nesnesini oluşturun ve PostImage özelliğine resmi atayın
                            ImagePost imagePost = new ImagePost
                            {
                                id = paylasim_student_id,
                                paylasim_Id = paylasim_id,
                                paylasim_adi = paylasim_ad,
                                Ogrenci_Id = ogrenci_id,
                                Ogrenci_Ad = ogrenci_ad,
                                sinif_adi = sinif_ad,
                                Ogretmen_Id = ogretmen_id,
                                Ogretmen_Ad = ogretmen_ad,
                                aciklama = aciklama,
                                begeni_sayisi = begeni_sayis,
                                begeni_durum = begeniDurumu,
                                Yorum_Mevcut = yorum_durumu,
                                Download_Durum = indirme_durumu,
                                paylasim_turu = paylasim_turu,
                                zaman = time,
                                PostImage = localFilePath
                            };
                            imagePosts.Add(imagePost);
                        }
                        else
                        {
                            // Check if the local image file already exists
                            if (File.Exists(localImagePath))
                            {
                                // If it exists, use the local file instead of downloading from the FTP server
                                byte[] localImageBytes = File.ReadAllBytes(localImagePath);
                                string base64Image = Convert.ToBase64String(localImageBytes);

                                ImagePost imagePost = new ImagePost
                                {
                                    id = paylasim_student_id,
                                    paylasim_Id = paylasim_id,
                                    paylasim_adi = paylasim_ad,
                                    Ogrenci_Id = ogrenci_id,
                                    Ogrenci_Ad = ogrenci_ad,
                                    sinif_adi = sinif_ad,
                                    Ogretmen_Id = ogretmen_id,
                                    Ogretmen_Ad = ogretmen_ad,
                                    aciklama = aciklama,
                                    begeni_sayisi = begeni_sayis,
                                    begeni_durum = begeniDurumu,
                                    Yorum_Mevcut = yorum_durumu,
                                    Download_Durum = indirme_durumu,
                                    paylasim_turu = paylasim_turu,
                                    zaman = time,
                                    PostImage2 = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64Image))),
                                };
                                imagePosts.Add(imagePost);
                            }
                            else
                            {
                                // Eğer yoksa, FTP sunucusundan indir
                                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteImagePath);
                                request.Method = WebRequestMethods.Ftp.DownloadFile;
                                request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                                using (Stream ftpStream = response.GetResponseStream())
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        byte[] buffer = new byte[1024];
                                        int bytesRead;
                                        while ((bytesRead = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            ms.Write(buffer, 0, bytesRead);
                                        }
                                        string base64Image = Convert.ToBase64String(ms.ToArray());

                                        // İndirilen resmi yerel dosya sistemine kaydet
                                        File.WriteAllBytes(localImagePath, ms.ToArray());

                                        // FTP'den indirilen resmi ImagePost nesnesine atama yapın
                                        ImageSource ftpImageSource = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64Image)));

                                        // ImagePost nesnesini oluşturun ve PostImage2'ye resmi atayın
                                        ImagePost imagePost = new ImagePost
                                        {
                                            id = paylasim_student_id,
                                            paylasim_Id = paylasim_id,
                                            paylasim_adi = paylasim_ad,
                                            Ogrenci_Id = ogrenci_id,
                                            Ogrenci_Ad = ogrenci_ad,
                                            sinif_adi = sinif_ad,
                                            Ogretmen_Id = ogretmen_id,
                                            Ogretmen_Ad = ogretmen_ad,
                                            aciklama = aciklama,
                                            begeni_sayisi = begeni_sayis,
                                            begeni_durum = begeniDurumu,
                                            Yorum_Mevcut = yorum_durumu,
                                            Download_Durum = indirme_durumu,
                                            paylasim_turu = paylasim_turu,
                                            zaman = time,
                                            PostImage2 = ftpImageSource,
                                        };
                                        imagePosts.Add(imagePost);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata yönetimi yapabilirsiniz
            }
            return imagePosts;
        }
    }
}
