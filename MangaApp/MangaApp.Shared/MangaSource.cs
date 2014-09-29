using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace MangaApp
{
    public interface IMangaSource
    {
        event EventHandler DataChanged;
        ObservableCollection<Manga> Latest { get; }
        ObservableCollection<Manga> CategoryMangas { get; }
        List<string> CategoryURL { get; }

        void GetMangasFromCategory(int category);
        void GetLatest(int page = 0);
        void GetChapters(Manga manga);
        void GetImages(Chapter c);
    }
}
