using ImbUserManagment2.Models;
using ImbUserManagment2.ViewModels.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImbUserManagment2.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/roles")]
    public class RolesApiController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<Users> userManager;

        public RolesApiController(
            RoleManager<IdentityRole> roleManager,
            UserManager<Users> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await roleManager.Roles
                .Select(role => new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty
                })
                .ToListAsync();

            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roleName = model.Name.Trim();

            var roleExists = await roleManager.RoleExistsAsync(roleName);

            if (roleExists)
            {
                return BadRequest(new
                {
                    message = "Role already exists."
                });
            }

            var role = new IdentityRole(roleName);

            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                message = "Role created successfully.",
                roleId = role.Id,
                roleName = role.Name
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, UpdateRoleDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound(new
                {
                    message = "Role not found."
                });
            }

            var newRoleName = model.Name.Trim();

            var existingRole = await roleManager.FindByNameAsync(newRoleName);

            if (existingRole != null && existingRole.Id != id)
            {
                return BadRequest(new
                {
                    message = "Another role with this name already exists."
                });
            }

            role.Name = newRoleName;
            role.NormalizedName = newRoleName.ToUpper();

            var result = await roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                message = "Role updated successfully.",
                roleId = role.Id,
                roleName = role.Name
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound(new
                {
                    message = "Role not found."
                });
            }

            if (role.Name == "Admin")
            {
                return BadRequest(new
                {
                    message = "You cannot delete the Admin role."
                });
            }

            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);

            if (usersInRole.Any())
            {
                return BadRequest(new
                {
                    message = "You cannot delete this role because it is assigned to users."
                });
            }

            var result = await roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                message = "Role deleted successfully.",
                roleId = id,
                roleName = role.Name
            });
        }
    }
}