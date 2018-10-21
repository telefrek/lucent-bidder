using System.ComponentModel.DataAnnotations;

namespace Lucent.Common.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class LoginEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Domain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Required]
        [DataType(DataType.Password)]
        public string Credentials { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool RememberMe { get; set; } = true;
    }
}