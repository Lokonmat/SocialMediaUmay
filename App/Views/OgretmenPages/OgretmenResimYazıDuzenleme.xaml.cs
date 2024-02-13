using App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.OgretmenPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OgretmenResimYazıDuzenleme : ContentPage
    {
        string ChatDetail;
        string ImageDetail;
        int ImageId;
        public OgretmenResimYazıDuzenleme(string chatDetail , string imageDetail, int imageId)
        {
            InitializeComponent();
            ChatDetail = chatDetail;
            ImageDetail = imageDetail;
            ImageId = imageId;
        }
    }
}