using System.ComponentModel.DataAnnotations;

namespace Lucent.Portal.Entities
{
    public class LoginEntity
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Domain { get; set; } = "lucent-dev.com";
        [Required]
        [DataType(DataType.Password)]
        public string Credentials { get; set; }
        public bool RememberMe { get; set; } = true;
    }
}