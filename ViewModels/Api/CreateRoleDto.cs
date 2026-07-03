using System.ComponentModel.DataAnnotations;

namespace ImbUserManagment2.ViewModels.Api
{
    public class CreateRoleDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}