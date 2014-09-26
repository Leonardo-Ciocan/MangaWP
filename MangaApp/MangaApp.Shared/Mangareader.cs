using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml.Media.Imaging;

namespace MangaApp
{
    public class Mangareader: INotifyPropertyChanged , IMangaSource
    {
        #region Properties
        public event EventHandler DataChanged;

        public ObservableCollection<Manga> _saved = new ObservableCollection<Manga>();
        public ObservableCollection<Manga> Saved
        {
            get
            {
                LoadSaved();
                return _saved;
            }
        }

        public ObservableCollection<Manga> _latest;
        public ObservableCollection<Manga> Latest { get { return _latest; } }

        public ObservableCollection<Manga> _searchResults;
        public ObservableCollection<Manga> SearchResults
        {
            get
            {
                return _searchResults;
            }

            set
            {
                _searchResults = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        public async void LoadSaved()
        {
            _saved.Clear();
            var items = await localFolder.GetFoldersAsync();
            foreach (StorageFolder folder in items)
            {
                var image = (await folder.GetFilesAsync()).FirstOrDefault();
                if (image == null) continue;
                Manga m = new Manga {Name = image.Name ,  Image = image.Path , Saved = true , Chapters = new ObservableCollection<Chapter>()};
                _saved.Add(m);

                foreach (StorageFolder cfolder in await folder.GetFoldersAsync())
                {
                    Chapter c = new Chapter { Name = cfolder.Name  , Images = new ObservableCollection<string>() , Saved = true};
                    foreach (StorageFile file in await cfolder.GetFilesAsync())
                    {
                        c.Images.Add(file.Path);
                    }
                    m.Chapters.Add(c);
                }
            }
        }
        

        public Mangareader()
        {
            _latest = new ObservableCollection<Manga>();
            _searchResults = new ObservableCollection<Manga>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        public async void GetLatest(int n = 0)
        {
            Latest.Clear();
            var htmlDocument = new HtmlDocument();
            htmlDocument.OptionFixNestedTags = true;
            htmlDocument.LoadHtml(await Utils.DownloadPageStringAsync("http://www.mangareader.net/"));

            var htmlDocument2 = new HtmlDocument();

            var mangas = new List<Manga>();

            foreach (HtmlNode cont in htmlDocument.DocumentNode.Descendants("tr").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "c3"))
            {
                var link = cont.Descendants("a").First();
                //Debug.WriteLine(link.Descendants("strong").ElementAt(0).InnerText);
                var m = new Manga
                {
                    Name = link.FirstChild.InnerText,
                    Url = "http://www.mangareader.net" + link.Attributes["href"].Value,
                    Updated = cont.Descendants("td").ElementAt(2).InnerText
                };
                mangas.Add(m);
                //m.Chapters.Clear();

                
                htmlDocument2.OptionFixNestedTags = true;
                htmlDocument2.LoadHtml(await Utils.DownloadPageStringAsync(m.Url));

                m.Image = htmlDocument2.DocumentNode.Descendants("div").Where(x => x.Id == "mangaimg").First().Descendants().First().Attributes["src"].Value;
                /*
                foreach (HtmlNode link2 in htmlDocument2.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href") && x.ParentNode.OriginalName == "td"))
                {
                    //Debug.WriteLine("Manga=" + m.Name + " Inner = " + link.InnerText);

                    m.Chapters.Add(new Chapter { Name = link2.InnerText, Url = link2.Attributes["href"].Value });
                }
                */
                /*if (mangas.Count == 15)
                {
                    foreach (Manga mx in mangas) Latest.Add(mx);
                    mangas.Clear();
                    //DataChanged(this, null);
                    //return;
                    
                }*/

               //if (Latest.Count > 4) return;
                Latest.Add(m);
            }
            //foreach (Manga mx in mangas) Latest.Add(mx);
            if(DataChanged !=null)DataChanged(this, null);
        }

        public  async void Search(string name)
        {
            string query = String.Format("http://www.mangareader.net/search/?w={0}&rd=0&status=0&order=0&genre=0000000000000000000000000000000000000&p=0", name);

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.OptionFixNestedTags = true;
            htmlDocument.LoadHtml(await Utils.DownloadPageStringAsync(query));


            foreach (HtmlNode link in htmlDocument.DocumentNode.Descendants("div").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "mangaresultinner"))
            {
                    var lk = link.Descendants("a").First();
                    SearchResults.Add(new Manga { Name = lk.InnerText, Url = "http://www.mangareader.net" + lk.Attributes["href"].Value });

                    Manga m = SearchResults.Last();
                    m.Chapters.Clear();
                    HtmlDocument htmlDocument2 = new HtmlDocument();
                    htmlDocument2.OptionFixNestedTags = true;
                    htmlDocument2.LoadHtml(await Utils.DownloadPageStringAsync(m.Url));

                    var txt = link.Descendants("div").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "imgsearchresults").First();
                    var tt = txt.Attributes["style"].Value.Replace("background-image:url('", "").Replace("')", "");
                    m.Image = tt;

                    foreach (HtmlNode link2 in htmlDocument2.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href") && x.ParentNode.OriginalName == "td"))
                    {
                        //Debug.WriteLine("Manga=" + m.Name + " Inner = " + link.InnerText);
                        m.Chapters.Add(new Chapter { Name = link2.InnerText.Replace(":" , "\n"), Url = link2.Attributes["href"].Value });
                    }
                
            }
        }

        /*async Task<List<string>> GetImages(Chapter c)
        {
            List<string> urls = new List<string>();

            return urls;
        }*/

        StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        public async void SaveChapter(Chapter c , Manga m)
        {
            /*string manga_url = m.Url.Replace("/", "").Replace(".", "").Replace(":", "").Replace("-", "").Replace("?","");
            string chapter_url = c.Name.Replace("/", "").Replace(".", "").Replace(":", "").Replace("-", "").Replace("\n" , "");
            StorageFolder folder =  (await localFolder.CreateFolderAsync(manga_url , CreationCollisionOption.OpenIfExists));
            StorageFolder chapterFolder = await folder.CreateFolderAsync(chapter_url , CreationCollisionOption.OpenIfExists);


            string manga_name = m.Name.Replace("/", "").Replace(".", "").Replace(":", "").Replace("-", "").Replace(".","");
            var file = await folder.CreateFileAsync(manga_name, CreationCollisionOption.ReplaceExisting);
            var downloader = new BackgroundDownloader();
            var download = downloader.CreateDownload(new Uri(m.Image), file);
            
            var res = await download.StartAsync();

            BackgroundTransferGroup mangaGroup = BackgroundTransferGroup.CreateGroup(manga_url);
            downloader.TransferGroup = mangaGroup;

            List<string> urls = await c.getImages();
            foreach (string s in urls)
            {
                string filename = s.Split('/').Last().ToString().Replace("-", "");
                try
                {
                    var cfile = await chapterFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                    var cdownload = downloader.CreateDownload(new Uri(s), cfile);
                    Utils.downloads.Add(cdownload);
                    await cdownload.StartAsync();
                    Debug.WriteLine("saving " + s);
                }
                catch
                {
                    Debug.WriteLine(filename + " +");
                }
            }

            */
        }


        public async void GetChapters(Manga manga)
        {
            manga.Chapters.Clear();
            HtmlDocument htmlDocument2 = new HtmlDocument();
            htmlDocument2.OptionFixNestedTags = true;
            htmlDocument2.LoadHtml(await Utils.DownloadPageStringAsync(manga.Url));

            manga.Image = htmlDocument2.DocumentNode.Descendants("div").First(x => x.Id == "mangaimg").Descendants().First().Attributes["src"].Value;

            foreach (HtmlNode link in htmlDocument2.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href") && x.ParentNode.OriginalName == "td" && x.ParentNode.Descendants("div").Any(y => y.Attributes["class"].Value == "chico_manga")))
            {
                manga.Chapters.Insert(0, new Chapter { Name = link.ParentNode.InnerText.Replace("\n", "").Replace(manga.Name, ""), Url = link.Attributes["href"].Value });
            }

            manga.Description = htmlDocument2.DocumentNode.Descendants("div").First(x => x.Id == "readmangasum").Descendants("p").First().InnerText;
        }

        public async void GetImages(Chapter c)
        {
            c.Images.Clear();
                //images.ItemsSource = c.Images;
                HtmlDocument htmlDocument2 = new HtmlDocument();
                htmlDocument2.OptionFixNestedTags = true;
                htmlDocument2.LoadHtml(await Utils.DownloadPageStringAsync(("http://www.mangareader.net" + c.Url)));

                List<string> holder = new List<string>();

                var items = htmlDocument2.DocumentNode.Descendants("option").Where((x =>
                        x.ParentNode.Id == "pageMenu"));
                /*for (int x = 0; x < items.Count(); x++)
                {
                    FlipViewItem item = new FlipViewItem();

                    images.Items.Add(item);
                }*/

                int count = 0;
                foreach (HtmlNode link in items)
                {

                    HtmlDocument htmlDocument3 = new HtmlDocument();
                    htmlDocument3.OptionFixNestedTags = true;
                    htmlDocument3.LoadHtml(await Utils.DownloadPageStringAsync(("http://www.mangareader.net" + link.Attributes["value"].Value)));

                    foreach (HtmlNode link2 in htmlDocument3.DocumentNode.Descendants("img").Where(x => x.Id == "img"))
                    {
                       
                        c.Images.Add(link2.Attributes["src"].Value);

                        /*(images.Items.ElementAt(count) as FlipViewItem).Content = new ScrollViewer
                        {
                            Content = new Image
                            {
                                Source = new BitmapImage(new Uri(link2.Attributes["src"].Value))
                            },
                            ZoomMode = ZoomMode.Enabled,
                            MaxZoomFactor = 3,
                            MinZoomFactor = 1
                        };*/
                        count++;
                        break;
                    }
        }
    }
    }
}
