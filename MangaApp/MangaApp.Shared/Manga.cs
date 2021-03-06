﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System.Globalization;
using Windows.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using Windows.UI.Xaml.Media;
namespace MangaApp
{
    public class Manga : INotifyPropertyChanged
    {
        public bool Saved;
        public string Name { get; set; }
        public string Url { get; set; }

        public bool Favorite
        {
            get { return AppModel.Current.FavoriteManga.Contains(Url); }
            set
            {
                if (value) AppModel.Current.FavoriteManga.Add(Url);
                else AppModel.Current.FavoriteManga.Remove(Url);
            }
        }

        public string _image;
        public string Image
        {
            get { return _image; }
            set { _image = value; RaisePropertyChanged(); }
        }

        private ImageSource source;
        public ImageSource ImageSource
        {
            get { return source ?? new BitmapImage(new Uri(Image, UriKind.Absolute)); }
        }

        public string _description;
        public string Description { get { return _description; } set { _description = value; RaisePropertyChanged(); } }

        public string _updated;
        public string Updated { get { return _updated; } set { _updated = value; RaisePropertyChanged(); } }

        public string AlternateNames { get; set; }
        public string Status { get; set; }
        public string Author { get; set; }
        public string Artist { get; set; }
        public string Year { get; set; }

        public ObservableCollection<Chapter> Chapters{get;set;}

        public Manga()
        {
            Chapters = new ObservableCollection<Chapter>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
    }

    public class Chapter : INotifyPropertyChanged
    {

        public bool Saved;
        public string Name { set; get; }
        public string Url { get; set; }
        public int ImageCount { get; set; }


        public Visibility ReadBadge
        {
            get { return AppModel.Current.ReadChapters.Contains(Url) ? Visibility.Visible : Visibility.Collapsed; }
        }

        public ObservableCollection<string> Images { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
        public Chapter()
        {
            Images = new ObservableCollection<string>();
        }

        /*public async Task<List<string>> getImages()
        {
            List<string> ls = new List<string>();
            HtmlDocument htmlDocument2 = new HtmlDocument();
            htmlDocument2.OptionFixNestedTags = true;
            htmlDocument2.LoadHtml(await Utils.DownloadPageStringAsync("http://www.mangareader.net" + Url));

            List<string> holder = new List<string>();

            var items = htmlDocument2.DocumentNode.Descendants("option").Where((x =>
                    x.ParentNode.Id == "pageMenu"));
            

            int count = 0;
            foreach (HtmlNode link in items)
            {

                HtmlDocument htmlDocument3 = new HtmlDocument();
                htmlDocument3.OptionFixNestedTags = true;
                htmlDocument3.LoadHtml(await Utils.DownloadPageStringAsync(("http://www.mangareader.net" + link.Attributes["value"].Value)));

                foreach (HtmlNode link2 in htmlDocument3.DocumentNode.Descendants("img").Where(x => x.Id == "img"))
                {
                    ls.Add(link2.Attributes["src"].Value);
                    count++;
                    break;
                }
            }
            return ls;
        }*/
    }

    
}
