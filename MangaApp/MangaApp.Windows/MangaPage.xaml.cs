using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
            
            
        }

        async void MangaPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }


        AppModel model;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            model = e.Parameter as AppModel;
            DataContext = e.Parameter;
            Manga m = model.CurrentManga;
            

            if (!m.Saved)
            {
                /*m.Chapters.Clear();
                HtmlDocument htmlDocument2 = new HtmlDocument();
                htmlDocument2.OptionFixNestedTags = true;
                htmlDocument2.LoadHtml(await DownloadPageStringAsync(m.Url));

                m.Image = htmlDocument2.DocumentNode.Descendants("div").Where(x => x.Id == "mangaimg").First().Descendants().First().Attributes["src"].Value;

                foreach (HtmlNode link in htmlDocument2.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href") && x.ParentNode.OriginalName == "td" && x.ParentNode.Descendants("div").Any(y => y.Attributes["class"].Value == "chico_manga")))
                {
                    m.Chapters.Insert(0, new Chapter { Name = link.ParentNode.InnerText, Url = link.Attributes["href"].Value });
                }

                m.Description = htmlDocument2.DocumentNode.Descendants("div").Where(x => x.Id == "readmangasum").First().Descendants("p").First().InnerText;*/
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
                //model.Provider.SaveChapter(chapter, model.CurrentManga);
            }
        }
    }
}
