using System.ComponentModel.DataAnnotations;

namespace Lucent.Common.Entities
{
    public class LoginEntity
    {
        [Required]
        public string Username { get; set; }
        public string Domain { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Credentials { get; set; }
        public bool RememberMe { get; set; } = true;
    }
}