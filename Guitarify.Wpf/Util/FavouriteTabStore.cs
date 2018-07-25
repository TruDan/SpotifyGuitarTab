using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Guitarify.Wpf.Util
{
    public class FavouriteTabStore
    {

        public static string StoreSavePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Guitarify\\Favourites.json");
        public static ObservableCollection<FavouriteTabEntry> Entries { get; } = new ObservableCollection<FavouriteTabEntry>();


        static FavouriteTabStore()
        {
            Load();
        }

        public static FavouriteTabEntry AddEntry(string trackId, string url)
        {
            RemoveEntry(trackId);

            var entry = new FavouriteTabEntry() {TrackId = trackId, TabUrl = url};
            Entries.Add(entry);
            Save();
            return entry;
        }

        public static bool TryGetEntry(string trackId, out FavouriteTabEntry entry)
        {
            var match = Entries.FirstOrDefault(e => e.TrackId.Equals(trackId));
            entry = match;
            return match != null;
        }

        public static void RemoveEntry(string trackId)
        {
            if (TryGetEntry(trackId, out var match))
            {
                Entries.Remove(match);
            }
        }


        private static void Load()
        {
            if (!File.Exists(StoreSavePath))
            {
                Save();
            }

            var contents = File.ReadAllText(StoreSavePath);
            var list = JsonConvert.DeserializeObject<List<FavouriteTabEntry>>(contents);

            Entries.Clear();
            foreach (var entry in list)
            {
                Entries.Add(entry);
            }
        }

        private static void Save()
        {
            var json = JsonConvert.SerializeObject(Entries.ToArray());

            File.WriteAllText(StoreSavePath, json);
        }

    }

    public class FavouriteTabEntry
    {
        public string TrackId { get; set; }
        public string TabUrl { get; set; }
    }
}
