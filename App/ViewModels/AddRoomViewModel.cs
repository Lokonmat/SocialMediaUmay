using App.Models;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UmayAppNew;
using Xamarin.Essentials;

namespace App.ViewModels
{
    class AddRoomViewModel
    {
        string userId = Preferences.Get("Id", "");

        public List<OgrenciSinif> OgrenciSiniflar { get; set; }
        public AddRoomViewModel() 
        {
            OgrenciSiniflar = GetOgrencis().Result;
        }

        private async Task<List<OgrenciSinif>> GetOgrencis()
        {
            List<OgrenciSinif> ogrenciSinifs = new List<OgrenciSinif>();

            try
            {
                string query1 = $"select `sinif_id` FROM `sinif` WHERE `ogretmen_id` = {userId}";
                string sinifIds = WebServis.TestKBM(query1);
                string[] SinifInfos = sinifIds.Split('\n');

                foreach (var sinifId in SinifInfos)
                {
                    string query2 = $"select `ogrenci_id`, `ogrenci_ad`, `sinif_ad` FROM `ogrenci_sinif` WHERE `sinif_id`= {sinifId}";
                    string result = WebServis.TestKBM(query2);
                    string[] OgrenciInfos = result.Split('\n');

                    foreach (var OgrenciInfo in OgrenciInfos)
                    {
                        string[] infoArray = OgrenciInfo.Split('|');
                        if (infoArray.Length >= 3)
                        {
                            int ogrenciId = int.Parse(infoArray[0]);
                            string ogrenciAd = infoArray[1];
                            string sinifAd = infoArray[2];

                            OgrenciSinif ogrenciSinif = new OgrenciSinif
                            {
                                Ogrenci_Id = ogrenciId,
                                Ogrenci_Ad = ogrenciAd,
                                Sinif_Ad = sinifAd,
                            };
                            ogrenciSinifs.Add(ogrenciSinif);
                        }
                    }
                }
            }
            catch
            {

            }
            return ogrenciSinifs;
        }

    }
}
