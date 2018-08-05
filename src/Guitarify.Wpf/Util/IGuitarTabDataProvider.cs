using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guitarify.Wpf.Util
{
    public interface IGuitarTabDataProvider
    {

        IGuitarTabSearchResult[] Search(string artistName, string trackName);

    }

    public enum GuitarTabType
    {
        //Official,
        //GuitarPro,
        //Power,
        //Drums,
        Chords,
        Tab,
        Bass
    }

    public interface IGuitarTabDataEntry
    {

        string ArtistName { get; }
        string TrackName { get; }

        float Rating { get; }
        int RatingCount { get; }


    }

    public interface IGuitarTabSearchResult
    {
        string ArtistName { get; }
        string TrackName { get; }

        string Version { get; }

        double Rating { get; }
        int RatingCount { get; }

        string Type { get; }

        string Url { get; }
    }
}
