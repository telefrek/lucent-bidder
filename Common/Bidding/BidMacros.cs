using Lucent.Common.Exchanges;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Class to manipulate bid macros
    /// </summary>
    public class BidMacros
    {
        /// <summary>
        /// ID of advertiser for the offer
        /// </summary>
        /// <value></value>
        public static readonly string ADVERTISER_ID = "{adv_id}";

        /// <summary>
        /// Publisher click ID specified in the tracking link
        /// </summary>
        /// <value></value>
        public static readonly string CLICK_ID = "{click_id}";

        /// <summary>
        /// ID/name of app or website
        /// </summary>
        /// <value></value>
        public static readonly string PUBLISHER_ID = "{pub_id}";

        /// <summary>
        /// ID/name of exchange
        /// </summary>
        /// <value></value>
        public static readonly string EXCHANGE_ID = "{exc_id}";

        /// <summary>
        /// City name where the click came from
        /// </summary>
        /// <value></value>
        public static readonly string CITY = "{city}";

        /// <summary>
        /// User’s ISO two-letter country code based on IP address (ex: US, UK)
        /// </summary>
        /// <value></value>
        public static readonly string COUNTRY_CODE = "{country_code}";

        /// <summary>
        /// Three-letter ISO currency abbreviation (ex: USD, EUR)
        /// </summary>
        /// <value></value>
        public static readonly string CURRENCY = "{currency}";

        /// <summary>
        /// Current date formatted as YYYY-MM-DD
        /// </summary>
        /// <value></value>
        public static readonly string DATE = "{date}";

        /// <summary>
        /// Current date and time formatted as YYYY-MM-DDTHH:MM:SSZ (ISO8601)
        /// </summary>
        /// <value></value>
        public static readonly string DATETIME = "{datetime}}";

        /// <summary>
        /// Current time formatted as HH:MM:SS
        /// </summary>
        /// <value></value>
        public static readonly string TIME = "{time}";

        /// <summary>
        /// IP address that started the click session
        /// </summary>
        /// <value></value>
        public static readonly string IPV4 = "{ip}";

        /// <summary>
        /// Name of user’s mobile carrier
        /// </summary>
        /// <value></value>
        public static readonly string CARRIER = "{carrier}";

        /// <summary>
        /// Offer payout from advertiser
        /// </summary>
        /// <value></value>
        public static readonly string PAYOUT = "{payout}";

        /// <summary>
        /// Value of a customer’s purchase
        /// </summary>
        /// <value></value>
        public static readonly string PURCHASE = "{purchase}";

        /// <summary>
        /// ID of the event (a unique # for install, purchase etc)
        /// </summary>
        /// <value></value>
        public static readonly string CONVERSION_ID = "{conversion_id}";

        /// <summary>
        /// Name of the event (example: Install)
        /// </summary>
        /// <value></value>
        public static readonly string EVENT = "{event}";

        /// <summary>
        /// Brand name of user’s device from user agent string (example: Apple)
        /// </summary>
        /// <value></value>
        public static readonly string DEVICE_BRAND = "{device_brand}";

        /// <summary>
        /// Model name of user’s device from user agent string (example: iPhone)
        /// </summary>
        /// <value></value>
        public static readonly string DEVICE_MODEL = "{device_model}";

        /// <summary>
        /// OS name of user’s device from user agent string (example: iOS)
        /// </summary>
        /// <value></value>
        public static readonly string DEVICE_OS = "{device_os}";

        /// <summary>
        /// OS version of user’s device from user agent string (example: 4.3.2)
        /// </summary>
        /// <value></value>
        public static readonly string DEVICE_OS_VERSION = "{device_os_version}";

        /// <summary>
        /// Google Android advertising identifier used to attribute clicks to installs for apps in the Google Play store
        /// </summary>
        /// <value></value>
        public static readonly string GOOGLE_AID = "{google_aid}";

        /// <summary>
        /// Unique ID for Android device
        /// </summary>
        /// <value></value>
        public static readonly string ANDROID_ID = "{android_id}";

        /// <summary>
        /// Apple iOS advertising identifier (iOS 6+)
        /// </summary>
        /// <value></value>
        public static readonly string IDFA = "{idfa}";

        /// <summary>
        /// Catch-all for unknown mobile device identifiers
        /// </summary>
        /// <value></value>
        public static readonly string UNID = "{unid}";

        /// <summary>
        /// Application-specific user ID (generated by app developer)
        /// </summary>
        /// <value></value>
        public static readonly string USER_ID = "{user_id}";

        /// <summary>
        /// ID of creative
        /// </summary>
        /// <value></value>
        public static readonly string CREATIVE_ID = "{creative_id}";

        /// <summary>
        /// id or name of a set of creatives
        /// </summary>
        /// <value></value>
        public static readonly string CREATIVE_GROUP = "{creative_group}";

        /// <summary>
        /// Static collection of supported macros
        /// </summary>
        /// <value></value>
        public static readonly string[] ALL = new string[]{
            ADVERTISER_ID,
            CLICK_ID,
            PUBLISHER_ID,
            EXCHANGE_ID,
            CITY,
            COUNTRY_CODE,
            CURRENCY,
            DATE, 
            DATETIME,
            TIME,
            IPV4,
            CARRIER,
            PAYOUT, 
            PURCHASE,
            CONVERSION_ID, 
            EVENT,
            DEVICE_BRAND, 
            DEVICE_MODEL,
            DEVICE_OS,
            DEVICE_OS_VERSION, 
            GOOGLE_AID,
            ANDROID_ID, 
            IDFA,
            UNID,
            USER_ID,
            CREATIVE_ID,
            CREATIVE_GROUP,
        };
    }
}