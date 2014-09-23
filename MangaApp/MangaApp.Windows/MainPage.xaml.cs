using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MangaApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public  MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
               
            
        }

        AppModel model = new AppModel();
        bool loaded = false;
        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {

                DataContext = model;

                
                await model.Provider.getLatest();

                loaded = true;
                

                
            }
        }




        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void latest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Manga m = (sender as ListView).SelectedValue as Manga;
            model.CurrentManga = m;
            if (m == null) return;
            Frame.Navigate(typeof(MangaPage), model);
        }

        private void saved_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Manga m = (sender as ListView).SelectedValue as Manga;
            model.CurrentManga = m;
            if (m == null) return;
            Frame.Navigate(typeof(MangaPage), model);
        }

        private async void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                (DataContext as AppModel).Provider.Search((sender as TextBox).Text);
            }
        }

        private void showDownloads(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
           // Frame.Navigate(typeof(SavedManga));
        }

        
    }
}
