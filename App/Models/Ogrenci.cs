using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace App.Models
{
    public class Ogrenci : INotifyPropertyChanged
    {
        public int Ogrenci_Id { get; set; }
        public string Ogrenci_Ad { get; set; }
        public int Veli_Id { get; set; }
        public string Veli_Ad { get; set; }
        public string Veli_Token { get; set; }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged(nameof(Selected));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
