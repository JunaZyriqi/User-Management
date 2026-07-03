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
    [Route("api/users")]
    public class UsersApiController : ControllerBase
    {
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UsersApiController(
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        private bool IsImbEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) &&
                   email.Trim().ToLower().EndsWith("@imb.al");
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await userManager.Users.ToListAsync();

            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                result.Add(new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    Department = user.Department,
                    Position = user.Position,
                    Roles = roles
                });
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsImbEmail(model.Email))
            {
                return BadRequest(new
                {
                    message = "Only @imb.al emails are allowed."
                });
            }

            var existingUser = await userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                return BadRequest(new
                {
                    message = "User with this email already exists."
                });
            }

            var roleExists = await roleManager.RoleExistsAsync(model.Role);

            if (!roleExists)
            {
                return BadRequest(new
                {
                    message = "Role does not exist."
                });
            }

            var user = new Users
            {
                FullName = model.FullName,
                UserName = model.Email,
                NormalizedUserName = model.Email.ToUpper(),
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                Department = model.Department,
                Position = model.Position
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await userManager.AddToRoleAsync(user, model.Role);

            return Ok(new
            {
                message = "User created successfully.",
                userId = user.Id,
                email = user.Email,
                role = model.Role
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsImbEmail(model.Email))
            {
                return BadRequest(new
                {
                    message = "Only @imb.al emails are allowed."
                });
            }

            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            var roleExists = await roleManager.RoleExistsAsync(model.Role);

            if (!roleExists)
            {
                return BadRequest(new
                {
                    message = "Role does not exist."
                });
            }

            var existingUserWithEmail = await userManager.FindByEmailAsync(model.Email);

            if (existingUserWithEmail != null && existingUserWithEmail.Id != id)
            {
                return BadRequest(new
                {
                    message = "Another user with this email already exists."
                });
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.NormalizedEmail = model.Email.ToUpper();
            user.NormalizedUserName = model.Email.ToUpper();
            user.Department = model.Department;
            user.Position = model.Position;

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

            var currentRoles = await userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await userManager.AddToRoleAsync(user, model.Role);

            return Ok(new
            {
                message = "User updated successfully.",
                userId = user.Id,
                email = user.Email,
                role = model.Role
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            var roles = await userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
            {
                return BadRequest(new
                {
                    message = "You cannot delete an admin user."
                });
            }

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                message = "User deleted successfully.",
                userId = id,
                email = user.Email
            });
        }
    }
}