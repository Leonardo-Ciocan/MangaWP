using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;

namespace MangaApp
{
    public class Utils
    {
        public static async Task<string> DownloadPageStringAsync(string url)
        {
            HttpClientHandler handler = new HttpClientHandler() { UseDefaultCredentials = true, AllowAutoRedirect = true };

            HttpClient client = new HttpClient(handler);
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public static ObservableCollection<DownloadOperation> downloads = new ObservableCollection<DownloadOperation>();
    }
}
