using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DistillNET;
using Guitarify.Wpf.Properties;

namespace Guitarify.Wpf.Util
{
    public static class AdBlock
    {

        private static FilterDbCollection _adBlockFilters;

        static AdBlock()
        {
            var parser = new AbpFormatRuleParser();

            var blacklist = Resources.adblock_blacklist.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var compiledFilters = new List<Filter>(blacklist.Length);

            foreach (var entry in blacklist)
            {
                compiledFilters.Add(parser.ParseAbpFormattedRule(entry, 1));
            }

            _adBlockFilters = new FilterDbCollection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Guitarify\\AdBlock.db"), true, true);

            _adBlockFilters.ParseStoreRules(blacklist, 1);
            _adBlockFilters.FinalizeForRead();
        }
        public static bool IsAdUrl(Uri url, NameValueCollection headers, string referer = "")
        {
            var filters = _adBlockFilters.GetFiltersForRequest(url, referer);

            if (filters == null) return false;

            foreach (Filter filter in filters)
            {
                if (filter is UrlFilter urlFilter)
                {
                    if (urlFilter.IsMatch(url, headers)) return true;
                }
            }

            return false;
        }
    }
}
