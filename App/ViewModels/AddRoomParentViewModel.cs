using App.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;

namespace App.ViewModels
{
    class AddRoomParentViewModel
    {
        string userId = Preferences.Get("Id", "");
        public List<OgrenciSinif> Ogretmenler { get; set; }
        public AddRoomParentViewModel()
        {
            Ogretmenler = GetOgretmens().Result;
        }
        private async Task<List<OgrenciSinif>> GetOgretmens()
        {
            List<OgrenciSinif> ogretmens = new List<OgrenciSinif>();
            try
            {
                string query1 = $"select `ogrenci_id` FROM `ogrenci_sinif` WHERE `veli_id` = {userId}";
                string ogrenciIds = WebServis.TestKBM(query1);
                string[] SinifInfos = ogrenciIds.Split('\n');
                foreach (var ogrenciId in SinifInfos)
                {
                    string query2 = $"select `ogrenci_id`, `ogrenci_ad`, `ogretmen_id`, `ogretmen_ad`, `sinif_ad` FROM `ogrenci_sinif` WHERE `ogrenci_id`= {ogrenciId}";
                    string result = WebServis.TestKBM(query2);
                    string[] OgretmenInfos = result.Split('\n');
                    foreach (var OgretmenInfo in OgretmenInfos)
                    {
                        string[] infoArray = OgretmenInfo.Split('|');
                        if (infoArray.Length >= 5)
                        {
                            int ogrenciIdd = int.Parse(infoArray[0]);
                            string ogrenciAd = infoArray[1];
                            int ogretmenId = int.Parse(infoArray[2]);
                            string ogretmenAd = infoArray[3];
                            string sinifAd = infoArray[4];
                            OgrenciSinif ogretmen = new OgrenciSinif
                            {
                                Ogrenci_Id = ogrenciIdd,
                                Ogrenci_Ad = ogrenciAd,
                                Ogretmen_Id = ogretmenId,
                                Ogretmen_Ad = ogretmenAd,
                                Sinif_Ad = sinifAd,
                            };
                            ogretmens.Add(ogretmen);
                        }
                    }
                }
            }
            catch
            {

            }
            return ogretmens;
        }
    }
}
