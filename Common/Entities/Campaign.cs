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

        [Display(Name = "Start")]
        public double Spend { get; set; }

        [Display(Name = "Id")]
        public string Id { get; set; }
        public CampaignStatus Status { get; set; }

        public List<Creative> Creatives { get; set; } = new List<Creative>();

        public Schedule Schedule { get; set; } = new Schedule { };
        public Budget Budget { get; set; }

        // storage properties
        string IStorageEntity.ETag { get; set; }
        DateTime IStorageEntity.Updated { get; set; }
        int IStorageEntity.Version { get; set; }
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

    public class Schedule
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

    public class Targetting
    {
        // need some rules
    }

    public class Budget
    {
        public double TotalHourly { get; set; }
        public double TotalDaily { get; set; }
        public double Total { get; set; }
    }

    public class FrequencyCaps
    {

    }
}