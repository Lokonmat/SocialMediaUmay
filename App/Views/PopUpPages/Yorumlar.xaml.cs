using App.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.PopUpPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Yorumlar : Popup
    {
        string userId = Preferences.Get("Id", "");
        string fullName = Preferences.Get("FullName", "");
        string postId = Preferences.Get("PostId", "");
        string postName = Preferences.Get("PostName", "");

        private ObservableCollection<Yorum> PostComment = new ObservableCollection<Yorum>();
        public Yorumlar()
        {
            InitializeComponent();
            // ObservableCollection'ı başlatın
            CommentCollectionview.ItemsSource = PostComment;
            // Yorumları güncelleyin
            UpdateYorumAsync();
        }
        private void CommentSendButton_Clicked(object sender, EventArgs e)
        {
            // Yorumu alın
            string yorum = yorumEntry.Text;
            string unicodeMessage = StringToUnicode(yorum);

            DateTime now = DateTime.Now;
            string formattedDate = now.ToString("yyyy-MM-dd HH:mm");

            // Yorumu veritabanına ekleyin
            string sorgu = $"INSERT INTO `yorum`(`Paylasim_Ad`, `Paylasim_Id`, `Kullanici_Id`, `Kullanici_Ad`, `Yorum_Metin`, `Tarih`) VALUES ('{postName}', {postId}, {userId},'{fullName}','{unicodeMessage}','{formattedDate}')";
            WebServis.TestKBM(sorgu);

            yorumEntry.Text = "";

            // Yorumları güncelleyin
            UpdateYorumAsync();
        }
        static string StringToUnicode(string input)
        {
            StringBuilder unicodeString = new StringBuilder();
            foreach (char c in input)
            {
                unicodeString.Append("\\u");
                unicodeString.Append(((int)c).ToString("X4"));
            }
            return unicodeString.ToString();
        }
        static string UnicodeToString(string input)
        {
            string[] unicodeChars = input.Split('\\', 'u');
            StringBuilder result = new StringBuilder();

            foreach (string unicodeChar in unicodeChars)
            {
                if (!string.IsNullOrEmpty(unicodeChar))
                {
                    int unicodeInt = Convert.ToInt32(unicodeChar, 16);
                    result.Append((char)unicodeInt);
                }
            }

            return result.ToString();
        }
        private async Task UpdateYorumAsync()
        {
            // Yorumları veritabanından alın
            List<Yorum> yorumlar = await GetYorumsFromDataBase();

            // Önceki yorumları temizleyin
            PostComment.Clear();

            foreach (var yorum in yorumlar)
            {
                PostComment.Add(yorum);
            }
        }
        private async Task<List<Yorum>> GetYorumsFromDataBase()
        {
            List<Yorum> yorumlar = new List<Yorum>();

            // Veritabanından yorumları alın
            string sorgu = $"select `Yorum_Id`, `Paylasim_Id`, `Kullanici_Id`, `Kullanici_Ad`, `Yorum_Metin`, `Tarih` FROM `yorum` WHERE `Paylasim_Id`={postId}";
            string sonuclar = WebServis.TestKBM(sorgu);

            string[] yorumBilgileri = sonuclar.Split('\n');
            foreach (var yorumBilgi in yorumBilgileri)
            {
                if (string.IsNullOrWhiteSpace(yorumBilgi))
                {
                    continue;
                }

                string[] bilgiDizisi = yorumBilgi.Split('|');
                if (bilgiDizisi.Length >= 6)
                {
                    int yorum_id = int.Parse(bilgiDizisi[0].Trim());
                    int paylasim_id = int.Parse(bilgiDizisi[1].Trim());
                    int kullanici_id = int.Parse(bilgiDizisi[2].Trim());
                    string kullanici_ad = bilgiDizisi[3].Trim();
                    string yorum_metin = bilgiDizisi[4].Trim();
                    string tarihStr = bilgiDizisi[5].Trim();
                    DateTime tarih = DateTime.ParseExact(tarihStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    string displayedMessage = UnicodeToString(yorum_metin);
                    Yorum yorum = new Yorum
                    {
                        Yorum_Id = yorum_id,
                        Paylasim_Id = paylasim_id,
                        Kullanici_Id = kullanici_id,
                        Kullanici_Ad = kullanici_ad,
                        Yorum_Metin = displayedMessage,
                        Tarih = tarih
                    };
                    yorumlar.Add(yorum);
                }
            }
            return yorumlar;
        }
        private void YorumEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                // İlk harfi büyük yaparak metni güncelle
                yorumEntry.Text = UppercaseFirst(e.NewTextValue);
            }
        }
        private string UppercaseFirst(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }
}