using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Campaign : IStorageEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Id")]
        [SerializationProperty(1, "id")]
        public string Id
        {
            get => Key.ToString();
            set
            {
                Key = new StringStorageKey(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public StorageKey Key { get; set; } = new StringStorageKey();

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Name")]
        [Required, StringLength(100)]
        [SerializationProperty(2, "name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Spend")]
        [SerializationProperty(3, "spend")]
        public double Spend { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Schedule")]
        [SerializationProperty(4, "schedule")]
        public CampaignSchedule Schedule { get; set; } = new CampaignSchedule { };

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "filters")]
        public BidFilter BidFilter { get; set; } = new BidFilter { GeoFilters = new Filters.Filter[] { new Filters.Filter { Property = "Country", PropertyType = typeof(string), Value = "USA", FilterType = Filters.FilterType.EQ } } };

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Landing Page")]
        [Required, StringLength(1024)]
        [SerializationProperty(6, "landing")]
        public string LandingPage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "BuyerId")]
        [SerializationProperty(7, "buyerid")]
        public string BuyerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "BundleId")]
        [SerializationProperty(8, "bundleid")]
        public string BundleId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value> 
        [Display(Name = "Domains")]
        [UIHint("Domains")]
        [SerializationProperty(9, "domains")]
        public string[] AdDomains { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Status")]
        [SerializationProperty(10, "status")]
        public CampaignStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// 
        public List<Creative> Creatives { get; set; } = new List<Creative>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [SerializationProperty(11, "createiveids")]
        public string[] CreativeIds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Caps")]
        [SerializationProperty(12, "spendcap")]
        public SpendCap SpendCaps { get; set; } = new SpendCap { };

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Conversion Price")]
        [SerializationProperty(13, "conversion")]
        public double ConversionPrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Func<BidRequest, bool> IsFiltered { get; set; } = (br) => false;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string ETag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public DateTime Updated { get; set; }

        /// <inheritdoc/>
        public EntityType EntityType { get; set; } = EntityType.Campaign;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CampaignStatus
    {
        /// <value></value>
        [Display(Name = "Experiencing Problems")]
        Unknown = 0,
        /// <value></value>
        [Display(Name = "Active")]
        Active = 1,
        /// <value></value>
        [Display(Name = "InActive")]
        InActive = 2,
        /// <value></value>
        [Display(Name = "Budget Exhausted")]
        BudgedExhausted = 3,
    }

    /// <summary>
    /// 
    /// </summary>
    public class SpendCap
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Hourly Cap")]
        [SerializationProperty(1, "hourly")]
        public double HourlySpendCap { get; set; } = 100d;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Daily Cap")]
        [SerializationProperty(2, "daily")]
        public double DailySpendCap { get; set; } = 100d;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Weekly Cap")]
        [SerializationProperty(3, "weekly")]
        public double WeeklySpendCap { get; set; } = 100d;
    }

    /// <summary>
    /// 
    /// </summary>
    public class CampaignSchedule
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Start")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [SerializationProperty(1, "start")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "End")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [SerializationProperty(2, "end")]
        public DateTime EndDate { get; set; } = DateTime.Now;
    }
}