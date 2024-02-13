using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Select : ContentPage
    {
        public Select()
        {
            InitializeComponent();
        }

        private void TeacherRegistorNavigateButton(object sender, EventArgs e)
        {
            Navigation.PushAsync(new TeacherRegisterPage());
        }

        private void ParentRegistorNavigateButton(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ParentRegisterPage());
        }
    }
}