using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.PopUpPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PopupPage2 : ContentPage
	{
		public PopupPage2 (View content)
		{
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.3); // Arkaplan rengi (Semi-transparent siyah)
            Padding = new Thickness(40);

            // Ekran boyutunu al
            var screenHeight = Application.Current.MainPage.Height;
            var popupHeight = screenHeight / 3; // Ekranın üçte biri kadar yükseklik

            // Ana içerik
            var mainLayout = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center, // Dikey olarak ortala
                HeightRequest = popupHeight // Yükseklik belirle
            };
            var frame = new Frame
            {
                BackgroundColor = Color.White,
                CornerRadius = 10,
                Padding = new Thickness(20),
                Content = content // İçerik
            };

            // Frame üzerine eklenen tıklama işlemi
            var frameTapGestureRecognizer = new TapGestureRecognizer();
            frameTapGestureRecognizer.Tapped += async (s, e) =>
            {
                // Popup'ı kapatarak tıklama olayını yakalama
                await Navigation.PopModalAsync();
            };
            frame.GestureRecognizers.Add(frameTapGestureRecognizer);

            // Siyah arkaplan üzerine eklenen tıklama işlemi
            var backgroundTapGestureRecognizer = new TapGestureRecognizer();
            backgroundTapGestureRecognizer.Tapped += async (s, e) =>
            {
                // Popup'ı kapatarak tıklama olayını yakalama
                await Navigation.PopModalAsync();
            };
            mainLayout.GestureRecognizers.Add(backgroundTapGestureRecognizer);

            mainLayout.Children.Add(frame);
            Content = mainLayout;
        }
	}
}