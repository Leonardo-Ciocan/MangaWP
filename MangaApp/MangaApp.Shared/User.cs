using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;

namespace MangaApp
{
    public class AppModel : INotifyPropertyChanged
    {

        public Mangareader Provider{get;set;}
        public Manga _current;
        public Manga CurrentManga { get { return _current; } set { _current = value; RaisePropertyChanged(); } }

        public ListViewSelectionMode _selecting = ListViewSelectionMode.Single;
        public ListViewSelectionMode Selecting { get { return _selecting; } set { _selecting = value; RaisePropertyChanged(); } }
        public AppModel()
        {
            Provider = new Mangareader();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
    }
}
