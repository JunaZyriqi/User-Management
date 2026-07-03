using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImbUserManagment2.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Department { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;
    }
}