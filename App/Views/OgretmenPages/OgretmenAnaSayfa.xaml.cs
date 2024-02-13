using App.Models;
using App.Views.ChatPages;
using App.Views.OgretmenPages.Paylasimlar;
using App.Views.OgretmenPages.Paylasimlar.Resimler;
using App.Views.OrtakSayfalar;
using App.Views.PopUpPages;
using App.Views.ProfileLyoutMenuViews;
using Plugin.FirebasePushNotification;
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

namespace App.Views.OgretmenPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OgretmenAnaSayfa : ContentPage
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
        private string remoteVideoPath;
        string localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        private int currentLoadedCount = 3;
        private int offset = 5; // Başlangıç OFFSET değeri
        private int likedCount = 0;
        private ObservableCollection<ImagePost> userImages = new ObservableCollection<ImagePost>();
        private Dictionary<string, string> imageCache = new Dictionary<string, string>();
        private HashSet<int> likedPosts = new HashSet<int>();
        private HashSet<int> loadedImageIds = new HashSet<int>(); // Daha önce yüklenmiş resimlerin ID'lerini içerecek set
        public OgretmenAnaSayfa()
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
            this.IsBusy = false;
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
        private void Messages_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new MessagesPage());
        }
        private void LikeButton_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton likeButton && likeButton.BindingContext is ImagePost imagePost)
            {
                int selectedId = imagePost.id;
                string selectedName = imagePost.paylasim_adi;
                int selectedLike = imagePost.begeni_sayisi;

                Preferences.Set("SelectedId", selectedId);
                string query = $"select * FROM `begeni` WHERE `begenen_kisi_ad` = '{fullName}' AND `paylasim_id` = {selectedId}";
                string result = WebServis.TestKBM(query);

                if (result == "")
                {
                    string query2 = $@"
                                        START TRANSACTION; 

                                        INSERT INTO `begeni`(`begenen_kisi_id`, `begenen_kisi_ad`, `paylasim_id`, `paylasim_ad`, `paylasim_icon`) 
                                        VALUES({userId}, '{fullName}', {selectedId}, '{selectedName}', 'icon_like.png');

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
                    likeButton.Source = "icon_unlike.png";
                    UpdateLikeCount(imagePost, false);
                    likedPosts.Remove(selectedId);
                };
            }
        }        
        private void likeCountLabel_Tapped(object sender, EventArgs e)
        {
            if (sender is Label lbl && lbl.BindingContext is ImagePost imagePost)
            {
                int selectedId = imagePost.id;
                string selectedName = imagePost.paylasim_adi;
                int selectedLike = imagePost.begeni_sayisi;

                if (selectedLike == 0)
                {                    
                    // DisplayAlert ile versiyon notlarını göster
                    DisplayAlert("Uyarı", "Paylaşımı beğenen kişi bulunmamakta", "Tamam");
                }
                else
                {
                    Navigation.PushAsync(new BegenenlerListesi(selectedId, selectedName));
                }
            }

        }
        private void CommentButton_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton commentButton && commentButton.BindingContext is ImagePost imagePost)
            {
                string comment = imagePost.Yorum_Mevcut;
                if (comment == "true")
                {
                    int selectedId = imagePost.id;
                    string selectedName = imagePost.paylasim_adi;
                    Preferences.Set("PostId", $"{selectedId}");
                    Preferences.Set("PostName", $"{selectedName}");
                    Navigation.ShowPopup(new Yorumlar());
                }
                else if (comment == "false")
                {
                    DisplayAlert("UYARI", "Bu içeriği yoruma kapattınız.", "Tamam");
                }
            }
        }
        private void EditButton_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton editButton && editButton.BindingContext is ImagePost imagePost)
            {
                ImageSource imageSource2 = imagePost.PostImage2;
                string imageDetail = imagePost.aciklama;
                int imageId = imagePost.id;
                string postDetail = imagePost.paylasim_turu;
                string postName = imagePost.paylasim_adi;

                if (postDetail == "Chat")
                {
                    Navigation.PushAsync(new OgretmenResimYazıDuzenleme(postName, imageDetail, imageId));
                }
                else
                {
                    Navigation.PushAsync(new OgretmenResimDuzenleme(imageSource2, imageDetail, imageId));
                }
            }
        }
        private void OgretmenAnaSayfa_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new OgretmenAnaSayfa();
            AdjustButtonSizes("AnaSayfa");
        }
        private void ProfilPage_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new ProfilePage());
            AdjustButtonSizes("Profil");
        }
        private void AdjustButtonSizes(string activeButton)
        {
            // Tüm butonları varsayılan boyuta getir
            btnAnaSayfa.Scale = 1;
            btnArti.Scale = 1;
            btnProfil.Scale = 1;

            // Aktif butona göre boyutları ayarla
            switch (activeButton)
            {
                case "AnaSayfa":
                    btnAnaSayfa.Scale = 1.05;
                    break;
                case "Profil":
                    btnProfil.Scale = 1.05;
                    break;
            }
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
        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton deleteButton && deleteButton.BindingContext is ImagePost imagePost)
            {
                bool answer = await DisplayAlert("Onay", "Bu silme işlemi geri alınamaz. Silmek istediğinize emin misiniz?", "Sil", "İptal");

                if (answer)
                {
                    int selectedId = imagePost.id;
                    string selectedName = imagePost.paylasim_adi;
                    string selectedSinif = imagePost.sinif_adi;
                    string combinedQuery = $" DELETE FROM `paylasim` WHERE `post_id` = {selectedId}";
                    string combinedQuery2 = $" DELETE FROM `paylasim_ogrenci` WHERE `paylasim_ad` = '{selectedName}' AND `sinif_ad` = '{selectedSinif}'";
                    string combinedQuery3 = $" DELETE FROM `begeni` WHERE `paylasim_ad` = '{selectedName}' AND `paylasim_id` = {selectedId}";
                                            
                    string resyltx = WebServis.TestKBM(combinedQuery);
                    string resyltx2 = WebServis.TestKBM(combinedQuery2);
                    string resyltx3 = WebServis.TestKBM(combinedQuery3);
                    string fullRemoteImagePath = $"{ftpServerUrl}/Post/{selectedSinif}_{selectedName}"; // FTP sunucusundaki dosyanın yolu ve adı

                    try
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(fullRemoteImagePath));
                        request.Method = WebRequestMethods.Ftp.DeleteFile;
                        request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                        string statusDescription = response.StatusDescription;
                        response.Close();

                        if (statusDescription == "File deleted")
                        {
                            Navigation.PushModalAsync(new ProfilePage());
                        }
                        else
                        {
                            // FTP sunucusundaki dosya silinemedi, gerekli işlemleri yapabilirsiniz.
                        }
                    }
                    catch (Exception ex)
                    {
                        // Hata yönetimi için gerekli işlemleri yapabilirsiniz.
                        Console.WriteLine("Hata: " + ex.Message);
                    }
                }
                await UpdateUserImagesAsync();
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
        private async Task UpdateUserImagesAsync()
        {
            List<ImagePost> newImagePosts = await GetImagePostsFromDatabase();
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
                string query = $"select `post_id`,`kullanici_adi`, `paylasim_adi`, `begeni_sayisi`, `aciklama`,`yorum_avaible`, `downloadable`, `paylasim_turu`, `paylasilan_sinif`, `zaman` FROM `paylasim` WHERE `kullanici_id`={userId} ORDER BY `zaman` DESC LIMIT 5";
                string results = WebServis.TestKBM(query);
                if(results == "")
                {
                    DisplayAlert("Uyarı", "Paylaşım bulunmamakta", "Tamam");
                    loadMore_Button.IsVisible = false;
                }
                else
                {
                    string[] imageInfos = results.Split('\n');
                    foreach (var imageInfo in imageInfos)
                    {
                        if (string.IsNullOrWhiteSpace(imageInfo))
                            continue;

                        string[] infoArray = imageInfo.Split('|');

                        if (infoArray.Length >= 10)
                        {
                            int postId = int.Parse(infoArray[0].Trim());
                            string username = infoArray[1].Trim();
                            string imageName = infoArray[2].Trim();
                            int likes = int.Parse(infoArray[3].Trim());
                            string description = infoArray[4].Trim();
                            string commentAvailable = infoArray[5].Trim();
                            string downloadAvailable = infoArray[6].Trim();
                            string paylasim_tur = infoArray[7].Trim();
                            string className = infoArray[8].Trim();
                            DateTime time = DateTime.Parse(infoArray[9].Trim());
                            string tamAd = $"{className}_{imageName}";
                            string remoteImagePath = $"{ftpServerUrl}Post/{tamAd}";
                            string localImagePath = Path.Combine(postFolderPath, tamAd);
                            string begeniDurumSorgu = $"select * FROM `begeni` WHERE `begenen_kisi_ad` = '{fullName}' AND `paylasim_id` = {postId}";
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

                            if (paylasim_tur == "chat" || paylasim_tur == "pdf" || paylasim_tur == "word" || paylasim_tur == "excel")
                            {
                                ImagePost imagePost = new ImagePost
                                {
                                    id = postId,
                                    paylasim_adi = imageName,
                                    begeni_sayisi = likes,
                                    begeni_durum = begeniDurumu,
                                    kullanici_adi = username,
                                    aciklama = description,
                                    sinif_adi = className,
                                    Yorum_Mevcut = commentAvailable,
                                    Download_Durum = downloadAvailable,
                                    paylasim_turu = paylasim_tur,
                                    zaman = time,
                                };
                                imagePosts.Add(imagePost);
                            }
                            else if (paylasim_tur == "video")
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

                                ImagePost imagePost = new ImagePost
                                {
                                    id = postId,
                                    paylasim_adi = imageName,
                                    begeni_sayisi = likes,
                                    begeni_durum = begeniDurumu,
                                    kullanici_adi = username,
                                    aciklama = description,
                                    sinif_adi = className,
                                    Yorum_Mevcut = commentAvailable,
                                    Download_Durum = downloadAvailable,
                                    paylasim_turu = paylasim_tur,
                                    PostImage = localFilePath,
                                    zaman = time,
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
                                        id = postId,
                                        paylasim_adi = imageName,
                                        begeni_sayisi = likes,
                                        begeni_durum = begeniDurumu,
                                        kullanici_adi = username,
                                        aciklama = description,
                                        sinif_adi = className,
                                        Yorum_Mevcut = commentAvailable,
                                        Download_Durum = downloadAvailable,
                                        paylasim_turu = paylasim_tur,
                                        PostImage2 = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64Image))),
                                        zaman = time,
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
                                                id = postId,
                                                paylasim_adi = imageName,
                                                begeni_sayisi = likes,
                                                begeni_durum = begeniDurumu,
                                                kullanici_adi = username,
                                                aciklama = description,
                                                sinif_adi = className,
                                                Yorum_Mevcut = commentAvailable,
                                                Download_Durum = downloadAvailable,
                                                paylasim_turu = paylasim_tur,
                                                PostImage2 = ftpImageSource,
                                                zaman = time,
                                            };
                                            imagePosts.Add(imagePost);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                // Hata yönetimi
            }
            return imagePosts;
        }
        private async Task<List<ImagePost>> GetNextImagePostsFromDatabase(int currentOffset)
        {
            List<ImagePost> imagePosts = new List<ImagePost>();
            try
            {
                // İstenen aralıktaki resimleri almak için startIndex ve count kullan
                string query = $"select `post_id`,`kullanici_adi`, `paylasim_adi`, `begeni_sayisi`, `aciklama`,`yorum_avaible`, `downloadable`, `paylasim_turu`, `paylasilan_sinif`, `zaman` FROM `paylasim` WHERE `kullanici_id`={userId} ORDER BY `zaman` DESC LIMIT 5 OFFSET {currentOffset}";
                string results = WebServis.TestKBM(query);
                string[] imageInfos = results.Split('\n');
                foreach (var imageInfo in imageInfos)
                {
                    if (string.IsNullOrWhiteSpace(imageInfo))
                        continue;
                    string[] infoArray = imageInfo.Split('|');
                    if (infoArray.Length >= 10)
                    {
                        int postId = int.Parse(infoArray[0].Trim());
                        string username = infoArray[1].Trim();
                        string imageName = infoArray[2].Trim();
                        int likes = int.Parse(infoArray[3].Trim());
                        string description = infoArray[4].Trim();
                        string commentAvailable = infoArray[5].Trim();
                        string downloadAvailable = infoArray[6].Trim();
                        string paylasim_tur = infoArray[7].Trim();
                        string className = infoArray[8].Trim();
                        DateTime time = DateTime.Parse(infoArray[9].Trim());
                        string tamAd = $"{className}_{imageName}";
                        string remoteImagePath = $"{ftpServerUrl}Post/{tamAd}";
                        string localImagePath = Path.Combine(postFolderPath, tamAd);
                        string begeniDurumSorgu = $"select * FROM `begeni` WHERE `begenen_kisi_ad` = '{fullName}' AND `paylasim_id` = {postId}";
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

                        if (paylasim_tur == "chat" || paylasim_tur == "pdf" || paylasim_tur == "word" || paylasim_tur == "excel")
                        {
                            ImagePost imagePost = new ImagePost
                            {
                                id = postId,
                                paylasim_adi = imageName,
                                begeni_sayisi = likes,
                                begeni_durum = begeniDurumu,
                                kullanici_adi = username,
                                aciklama = description,
                                sinif_adi = className,
                                Yorum_Mevcut = commentAvailable,
                                Download_Durum = downloadAvailable,
                                paylasim_turu = paylasim_tur,
                                zaman = time,
                            };
                            imagePosts.Add(imagePost);
                        }
                        else if (paylasim_tur == "video")
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
                                id = postId,
                                paylasim_adi = imageName,
                                begeni_sayisi = likes,
                                begeni_durum = begeniDurumu,
                                kullanici_adi = username,
                                aciklama = description,
                                sinif_adi = className,
                                Yorum_Mevcut = commentAvailable,
                                Download_Durum = downloadAvailable,
                                paylasim_turu = paylasim_tur,
                                PostImage = localFilePath,
                                zaman = time,
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
                                    id = postId,
                                    paylasim_adi = imageName,
                                    begeni_sayisi = likes,
                                    begeni_durum = begeniDurumu,
                                    kullanici_adi = username,
                                    aciklama = description,
                                    sinif_adi = className,
                                    Yorum_Mevcut = commentAvailable,
                                    Download_Durum = downloadAvailable,
                                    paylasim_turu = paylasim_tur,
                                    PostImage2 = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64Image))),
                                    zaman = time,
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
                                            id = postId,
                                            paylasim_adi = imageName,
                                            begeni_sayisi = likes,
                                            begeni_durum = begeniDurumu,
                                            kullanici_adi = username,
                                            aciklama = description,
                                            sinif_adi = className,
                                            Yorum_Mevcut = commentAvailable,
                                            Download_Durum = downloadAvailable,
                                            paylasim_turu = paylasim_tur,
                                            PostImage2 = ftpImageSource,
                                            zaman = time,
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
                // Hata yönetimi
            }
            return imagePosts;
        }

    }
}