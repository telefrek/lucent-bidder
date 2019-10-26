using System.Collections.Generic;

namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// Ledger extensions
    /// </summary>
    public static partial class LucentExtensions
    {
        /// <summary>
        /// Get the request metadata
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Dictionary<string, int> GetMetadata(this BidRequest request)
        {
            var metadata = new Dictionary<string, int>();
            var geo = (Geo)null;

            if (request.User != null)
            {
                metadata.AddIfNotNull(request.User.Gender, 1);
                geo = request.User.Geo;
            }

            if (request.Device != null)
            {
                metadata.AddIfNotNull(request.Device.OS + "_" + request.Device.OSVersion, 1);
                metadata.AddIfNotNull(request.Device.NetworkConnection.ToString(), 1);
                metadata.AddIfNotNull(request.Device.MobileCarrierCode, 1);
                geo = geo ?? request.Device.Geo;
            }

            if (request.Site != null)
            {
                foreach (var cat in request.Site.SiteCategories ?? new string[0])
                    metadata.AddIfNotNull(cat, 1);
                metadata.Add("site", 1);
            }

            if (request.App != null)
            {
                foreach (var cat in request.App.AppCategories ?? new string[0])
                    metadata.AddIfNotNull(cat, 1);
                metadata.Add("app", 1);
            }

            foreach (var imp in request.Impressions)
            {
                if (imp.Banner != null)
                {
                    metadata.Add("banner", 1);
                    metadata.Add("{0}x{1}".FormatWith(imp.Banner.H, imp.Banner.W), 1);
                }
            }

            if (geo != null)
            {
                metadata.AddIfNotNull(geo.Country, 1);
                metadata.AddIfNotNull(geo.ISP.ToString(), 1);
            }

            metadata.Remove("Unknown");

            return metadata;
        }

        /// <summary>
        /// Add to the dictionary if the key and value are not null
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddIfNotNull<T>(this Dictionary<string, T> dict, string key, T value)
        {
            if (key.IsNullOrDefault() || key == "_" || value.IsNullOrDefault())
                return;

            dict.TryAdd(key, value);
        }
    }
}