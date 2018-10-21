using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.OpenRTB;
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
        [Display(Name = "Name")]
        [Required, StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Spend")]
        public double Spend { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Id")]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Status")]
        public CampaignStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Creative> Creatives { get; set; } = new List<Creative>();

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string LandingPage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string[] AdDomains { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public BidFilter BidFilter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Func<BidRequest, bool> IsFiltered { get; set; } = (br) => false;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Schedule")]
        public CampaignSchedule Schedule { get; set; } = new CampaignSchedule { };

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Caps")]
        public CampaignSpendCaps SpendCaps { get; set; } = new CampaignSpendCaps { };

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        string IStorageEntity.ETag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        DateTime IStorageEntity.Updated { get; set; }
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
        [Display(Name = "Not Active")]
        InActive = 2,
        /// <value></value>
        [Display(Name = "Budget Spend")]
        BudgedExhausted = 3,
    }

    /// <summary>
    /// 
    /// </summary>
    public class CampaignSpendCaps
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Hourly Cap")]
        public double HourlySpendCap { get; set; } = 100d;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Daily Cap")]
        public double DailySpendCap { get; set; } = 100d;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "Weekly Cap")]
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
        public DateTime StartDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Display(Name = "End")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; } = DateTime.Now;
    }
}