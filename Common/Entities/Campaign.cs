using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.Budget;
using Lucent.Common.Filters;
using Lucent.Common.Middleware;
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

        /// <inheritdoc/>
        public StorageKey Key { get; set; } = new StringStorageKey();


        /// <inheritdoc/>
        public int Version { get; set; }

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
        public CampaignSchedule Schedule { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "filters")]
        public BidFilter BidFilter { get; set; }

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
        [SerializationProperty(11, "creatives")]
        public string[] CreativeIds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Budget Schedule")]
        [SerializationProperty(12, "budgetSchedule")]
        public BudgetSchedule BudgetSchedule { get; set; } = new BudgetSchedule { };

        /// <summary>
        /// The set of actions associated with this campaigns
        /// </summary>
        /// <value></value>
        [SerializationProperty(14, "actions")]
        public PostbackAction[] Actions { get; set; }

        /// <summary>
        /// The maximum CPM to spend on this campaign
        /// </summary>
        /// <value></value>
        [SerializationProperty(15, "maxcpm")]
        public double MaxCPM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(16, "targetting")]
        public BidTargets BidTargets { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(17, "offset")]
        public int Offset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(18, "jsonFilters")]
        public JsonFilter[] JsonFilters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(19, "jsonTargets")]
        public JsonFilter[] JsonTargets { get; set; }


        /// <summary>
        /// The maximum CPM to spend on this campaign
        /// </summary>
        /// <value></value>
        [SerializationProperty(20, "targetCPM")]
        public double TargetCPM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Func<BidRequest, bool> IsFiltered { get; set; } = (br) => false;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Func<BidRequest, double> GetModifier { get; set; } = (br) => 0d;

        /// <inheritdoc/>
        public string ETag { get; set; }

        /// <inheritdoc/>
        public DateTime Updated { get; set; }

        /// <inheritdoc/>
        public EntityType EntityType { get; set; } = EntityType.Campaign;
    }

    /// <summary>
    /// JSON based filter
    /// </summary>
    public class JsonFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "operation")]
        public string Operation { get; set; } = "eq";

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "entity")]
        public string Entity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "property")]
        public string Property { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "values")]
        public FilterValue[] Values { get; set; }

        /// <summary>
        /// The modifier for the targets
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "modifier")]
        public double Modifier { get; set; }
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
        [Display(Name = "Paused")]
        InActive = 2,
        /// <value></value>
        [Display(Name = "Budget Exhausted")]
        BudgedExhausted = 3,
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
        public DateTime EndDate { get; set; }
    }
}