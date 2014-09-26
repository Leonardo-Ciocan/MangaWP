using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MangaApp
{
    public interface IMangaSource
    {
        List<Manga> Latest { get; }

        void GetLatest(int page);
        void GetChapters(Manga manga);
        void GetImages(Chapter c);
    }
}
