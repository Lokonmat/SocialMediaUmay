using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace App.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private ImageSource selectedImageSource;
        public ImageSource SelectedImageSource
        {
            get { return selectedImageSource; }
            set
            {
                selectedImageSource = value;
                OnPropertyChanged(nameof(SelectedImageSource));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }
    }
}
