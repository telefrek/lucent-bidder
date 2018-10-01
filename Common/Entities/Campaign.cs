using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lucent.Common.Storage;

namespace Lucent.Common.Entities
{
    public class Campaign : IStorageEntity
    {
        [Display(Name = "Name")]
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Display(Name = "Spend")]
        public double Spend { get; set; }

        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "Status")]
        public CampaignStatus Status { get; set; }

        public List<Creative> Creatives { get; set; } = new List<Creative>();

        public string LandingPage { get; set; }
        public string[] AdDomains { get; set; }

        [Display(Name = "Schedule")]
        public CampaignSchedule Schedule { get; set; } = new CampaignSchedule { };

        [Display(Name = "Caps")]
        public CampaignSpendCaps SpendCaps { get; set; } = new CampaignSpendCaps { };

        // storage properties
        string IStorageEntity.ETag { get; set; }
        DateTime IStorageEntity.Updated { get; set; }
    }

    public enum CampaignStatus
    {
        [Display(Name = "Experiencing Problems")]
        Unknown = 0,
        [Display(Name = "Active")]
        Active = 1,
        [Display(Name = "Not Active")]
        InActive = 2,
        [Display(Name = "Budget Spend")]
        BudgedExhausted = 3,
    }

    public class CampaignSpendCaps
    {
        [Display(Name = "Hourly Cap")]
        public double HourlySpendCap { get; set; } = 100d;
        [Display(Name = "Daily Cap")]
        public double DailySpendCap { get; set; } = 100d;
        [Display(Name = "Weekly Cap")]
        public double WeeklySpendCap { get; set; } = 100d;
    }

    public class CampaignSchedule
    {
        [Display(Name = "Start")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "End")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; } = DateTime.Now;
    }
}