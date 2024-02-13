using App.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App.ViewModels
{
    class LikeNoti : INotifyPropertyChanged
    {
        private int _begeniSayisi;

        public int BegeniSayisi
        {
            get { return _begeniSayisi; }
            set
            {
                if (_begeniSayisi != value)
                {
                    _begeniSayisi = value;
                    OnPropertyChanged(nameof(BegeniSayisi));
                }
            }
        }

        public ICommand LikeCommand => new Command(LikeButtonClicked);

        public void LikeButtonClicked()
        {
            // Burada beğeni sayısını artırabilirsiniz
            BegeniSayisi++;

            // OnPropertyChanged metoduyla beğeni sayısının güncellendiğini bildirebilirsiniz
            OnPropertyChanged(nameof(BegeniSayisi));
        }

        public void UnLikeButtonClicked()
        {
            // Burada beğeni sayısını artırabilirsiniz
            BegeniSayisi--;

            // OnPropertyChanged metoduyla beğeni sayısının güncellendiğini bildirebilirsiniz
            OnPropertyChanged(nameof(BegeniSayisi));
        }

        // INotifyPropertyChanged arabirimini uygulayan bir sınıfın bildirimlerini yönetmek için bir metod
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

