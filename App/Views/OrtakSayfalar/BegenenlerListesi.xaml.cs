using App.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views.OrtakSayfalar
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BegenenlerListesi : ContentPage
    {
        int SelectedId;
        string SelectedName;
        private ObservableCollection<BegenenListe> begenenKisiler = new ObservableCollection<BegenenListe>();
        
        public BegenenlerListesi(int selectedId, string selectedName)
        {
            InitializeComponent();
            begenenKisilerListesi.ItemsSource = begenenKisiler;
            SelectedId = selectedId;
            SelectedName = selectedName;
            GetBegenenKisiLerListesi(selectedId, selectedName);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            GetList();
        }
        private async Task GetList()
        {
            begenenKisiler.Clear();
            List<BegenenListe> begenenListes = await GetBegenenKisiLerListesi(SelectedId, SelectedName);
            foreach(var  begenen in begenenListes)
            {
                begenenKisiler.Add(begenen);
            }
        }
        private async Task<List<BegenenListe>> GetBegenenKisiLerListesi(int SelectedId, string SelectedName)
        {
            List<BegenenListe> begenenListes = new List<BegenenListe>();
            try
            {
                string begenenKisilerListesiSorgusu = $"select `begenen_kisi_id`, `begenen_kisi_ad`, `paylasim_id`, `paylasim_ad` FROM `begeni` WHERE `paylasim_id` = {SelectedId} AND `paylasim_ad` = '{SelectedName}'";
                string begenenKisilerListesiSonuc = WebServis.TestKBM(begenenKisilerListesiSorgusu);
                string[] begenenKisilerBilgileri = begenenKisilerListesiSonuc.Split('\n');
                foreach (var begenenKisi in begenenKisilerBilgileri)
                {
                    if (string.IsNullOrWhiteSpace(begenenKisi))
                        continue;

                    string[] infoArray = begenenKisi.Split('|');
                    if (infoArray.Length >= 4)
                    {
                        int begenenKisiId = int.Parse(infoArray[0]);
                        string begenenKisiAd = infoArray[1];
                        int paylasimId = int.Parse(infoArray[2]);
                        string paylasimAd = infoArray[3];

                        BegenenListe begenenListe = new BegenenListe
                        {
                            BegenenKisiId = begenenKisiId,
                            BegenenKisiAd = begenenKisiAd,
                            PaylasimId = paylasimId,
                            PaylasimAd = paylasimAd,
                        };
                        begenenListes.Add(begenenListe);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return begenenListes;
        }
    }
}