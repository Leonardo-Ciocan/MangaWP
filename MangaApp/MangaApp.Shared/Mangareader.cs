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
    public class Mangareader: INotifyPropertyChanged
    {
        public ObservableCollection<Manga> _saved = new ObservableCollection<Manga>();
        public ObservableCollection<Manga> Saved
        {
            get
            {
                LoadSaved();
                return _saved;
            }
        }

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
        public ObservableCollection<Manga> _latest;
        public ObservableCollection<Manga> Latest { get { return _latest; } set { _latest = value; RaisePropertyChanged(); } }

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

        public Mangareader()
        {
            Latest = new ObservableCollection<Manga>();
            SearchResults = new ObservableCollection<Manga>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        public async Task getLatest()
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.OptionFixNestedTags = true;
            htmlDocument.LoadHtml(await Utils.DownloadPageStringAsync("http://www.mangareader.net/"));

            HtmlDocument htmlDocument2 = new HtmlDocument();

            List<Manga> mangas = new List<Manga>();

            foreach (HtmlNode link in htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "chapter"))
            {
                //Debug.WriteLine(link.Descendants("strong").ElementAt(0).InnerText);
                var m = new Manga
                {
                    Name = link.FirstChild.InnerText,
                    Url = "http://www.mangareader.net" + link.Attributes["href"].Value
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
                if (mangas.Count == 4)
                {
                    foreach (Manga mx in mangas) Latest.Add(mx);
                    mangas.Clear();
                }

                if (mangas.Count > 15) return;
            }
            foreach (Manga mx in mangas) Latest.Add(mx);
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

        async Task<List<string>> getImageUrls(Chapter c)
        {
            List<string> urls = new List<string>();

            return urls;
        }

        StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        public async void SaveChapter(Chapter c , Manga m)
        {
            string manga_url = m.Url.Replace("/", "").Replace(".", "").Replace(":", "").Replace("-", "").Replace("?","");
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


        }

        void t_sk(int n__key)
        {
            int joint_hash_ptr = -3;
            if (joint_hash_ptr % 3 == -1) joint_hash_ptr--;
        }
    }
}
