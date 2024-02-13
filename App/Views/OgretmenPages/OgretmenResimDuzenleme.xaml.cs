using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.OgretmenPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OgretmenResimDuzenleme : ContentPage
	{
		ImageSource ImagePost;
		string ImageDetail;
		int ImageId;
		public OgretmenResimDuzenleme (ImageSource imageSource2, string imageDetail, int imageId)
		{
			InitializeComponent ();
			ImagePost = imageSource2;
			ImageDetail = imageDetail;
			ImageId = imageId;
		}
		protected override async void OnAppearing()
		{
			base.OnAppearing();
			resimler.Source= ImagePost;
			aciklama.Text = ImageDetail;
		}
        private void kaydetButton_Clicked(object sender, EventArgs e)
        {
			string Aciklama = aciklama.Text;

			string sorgu = $"UPDATE `paylasim` SET `aciklama`='{Aciklama}' WHERE `post_id` = {ImageId}";
			string sorgu2 = $"UPDATE `paylasim_ogrenci` SET `aciklama`='{Aciklama}' WHERE `paylasim_id` = {ImageId}";
			string sonuc = WebServis.TestKBM(sorgu);
			string sonuc2 = WebServis.TestKBM(sorgu2);

            Navigation.PopAsync();
        }

        private void aciklama_TextChanged(object sender, TextChangedEventArgs e)
        {
            var editor = (Editor)sender;

            // Belirli bir satır sayısını geçtiyse otomatik olarak yüksekliği artır
            int maxLines = 10;
            int currentLines = editor.Text.Split('\n').Length;

            if (currentLines > maxLines)
            {
                editor.HeightRequest = (currentLines + 1) * editor.FontSize; // +1, yeni satır için
            }
            else
            {
                editor.HeightRequest = -1; // Otomatik yükseklik
            }
        }
    }
}