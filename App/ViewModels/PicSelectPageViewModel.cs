using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using UmayAppNew;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App.ViewModels
{
    public class PicSelectPageViewModel : INotifyPropertyChanged
    {
        private string selectedImagePath;
        private string selectedVideoPath;
        private string _selectedClass;

        public string SelectedImagePath
        {
            get { return selectedImagePath; }
            set
            {
                if (selectedImagePath != value)
                {
                    selectedImagePath = value;
                    OnPropertyChanged(nameof(SelectedImagePath)); // PropertyChanged olayını tetikleyin
                }
            }
        }

        public string SelectedClass
        {
            get => _selectedClass;
            set
            {
                _selectedClass = value;
                OnPropertyChanged(nameof(SelectedClass));
            }
        }

        public string SelectedVideoPath
        {
            get { return selectedVideoPath; }
            set
            {
                if (selectedVideoPath != value)
                {
                    selectedVideoPath = value;
                    OnPropertyChanged(nameof(SelectedVideoPath));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
       
    }
}
