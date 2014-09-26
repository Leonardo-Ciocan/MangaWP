using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MangaApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MangaPage : Page
    {
        public MangaPage()
        {
            this.InitializeComponent();
            this.Loaded += MangaPage_Loaded;
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView()
   .SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            
        }

        async void MangaPage_Loaded(object sender, RoutedEventArgs e)
        {
            StatusBar.GetForCurrentView().BackgroundOpacity = 0;
            StatusBar.GetForCurrentView().BackgroundColor = Windows.UI.Color.FromArgb(50, 0, 0, 1);
            StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            StatusBar.GetForCurrentView().ProgressIndicator.Text = model.CurrentManga.Name;
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = 0;

            intro.Begin();

            if (!loaded)
            {
                loaded = true;
                var tr = new TranslateTransform();
                header.RenderTransform = tr;
                imgGrid.RenderTransform = tr;
                var scroller = chapters.GetFirstDescendantOfType<ScrollViewer>();

                double max = 0.6;
                scroller.ViewChanged += (a, b) =>
                {
                    Debug.WriteLine(scroller.VerticalOffset);
                    var i = -scroller.VerticalOffset / 2.5;
                    tr.Y = i;
                    tr.Y = i;
                    //img.Opacity = 0.6-(scroller.VerticalOffset / 400.0) * 0.6;
                    // (header.RenderTransform as CompositeTransform).Rotation = -scroller.VerticalOffset / 3.5;
                    //header.Opacity = 1-Math.Max(0.1 , scroller.VerticalOffset)/400;
                };

                chapters.SelectionMode = ListViewSelectionMode.None;
                chapters.IsItemClickEnabled = true;
                chapters.ItemClick += (a, b) =>
                {
                    Frame.Navigate(typeof(ReaderPage), new object[] { model.CurrentManga.Chapters.IndexOf(b.ClickedItem as Chapter), model.CurrentManga });
                };
            }


        }

        bool loaded = false;
        AppModel model;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!loaded)
            {
                //loaded = true;
                model = e.Parameter as AppModel;
                DataContext = e.Parameter;
                Manga m = model.CurrentManga;
                //img.Source = new BitmapImage(new Uri(m.Image));

                if (!m.Saved)
                {
                    // TODO maybe chapters here?
                }
                /*chapters.SelectionMode = ListViewSelectionMode.None;
                chapters.IsItemClickEnabled = true;
                chapters.ItemClick += (a, b) =>
                {
                
                    Frame.Navigate(typeof(ReaderPage), new object[] { (a as ListView).SelectedIndex, model.CurrentManga });
                };*/

                
            }
        }



        public async Task<string> DownloadPageStringAsync(string url)
        {
            HttpClientHandler handler = new HttpClientHandler() { UseDefaultCredentials = true, AllowAutoRedirect = true };

            HttpClient client = new HttpClient(handler);
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }


        IList<object> elements = new List<object>();
        private void chapters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (model.Selecting != ListViewSelectionMode.Multiple)
            {
                Frame.Navigate(typeof(ReaderPage), new object[]{ (sender as ListView).SelectedIndex , model.CurrentManga});
            }
            elements = ((ListView)sender).SelectedItems;
        }

        private void chooseClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AppBarToggleButton bt = sender as AppBarToggleButton;
            if (bt.IsChecked.Value)
            {
                model.Selecting = ListViewSelectionMode.Multiple;
            }
            else
            {
                model.Selecting = ListViewSelectionMode.Single;
            }
        }


        private void download(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            foreach (object c in elements)
            {
                var chapter = (Chapter)c;
                //TODO implement this
                //model.Provider.SaveChapter(chapter, model.CurrentManga);
            }
        }
    }

    public static class VisualTreeHelperExtensions
    {
        public static T GetFirstDescendantOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetDescendantsOfType<T>().First();
        }

        public static IEnumerable<T> GetDescendantsOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            var queue = new Queue<DependencyObject>();
            var count = VisualTreeHelper.GetChildrenCount(start);

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(start, i);

                if (child is T)
                    yield return (T)child;

                queue.Enqueue(child);
            }

            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();
                var count2 = VisualTreeHelper.GetChildrenCount(parent);

                for (int i = 0; i < count2; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);

                    if (child is T)
                        yield return (T)child;

                    queue.Enqueue(child);
                }
            }
        }
    }
}
