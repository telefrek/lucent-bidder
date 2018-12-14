using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB
{
    /// <summary>
    /// 
    /// </summary>
    public class User
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(1, "id")]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(2, "buyeruid")]
        public string BuyerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(3, "yob")]
        public int YOB { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Gender Gender { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(4, "gender")]
        public string GenderStr
        {
            get
            {
                switch (Gender)
                {
                    case Gender.Male:
                        return "M";
                    case Gender.Female:
                        return "F";
                    default:
                        return "O";
                }
            }
            set
            {
                switch (value)
                {
                    case "M":
                        Gender = Gender.Male;
                        break;
                    case "F":
                        Gender = Gender.Female;
                        break;
                    case "O":
                        Gender = Gender.Other;
                        break;
                    default:
                        Gender = Gender.Unknown;
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(5, "keywords")]
        public string Keywords { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(6, "customdata")]
        public string CustomB85 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(7, "geo")]
        public Geo Geo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [SerializationProperty(8, "data")]
        public Data[] Data { get; set; }
    }
}