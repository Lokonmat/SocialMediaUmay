using App.Views.OgretmenPages.Paylasimlar.Resimler;
using App.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.OgretmenPages.Paylasimlar
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaylasimTuruSecme : ContentPage
    {
        public PaylasimTuruSecme()
        {
            InitializeComponent();
        }

        private async void fotografSecim_clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ResimPaylasimSayfasi());
        }

        private async void pdfSecim_clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PDFPaylasimSayfasi());
        }

        private async void videoSecimi_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConstructionPage());
        }
    }
}