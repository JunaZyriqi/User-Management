using ImbUserManagment2.Data;
using ImbUserManagment2.Models;
using ImbUserManagment2.ViewModels.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ImbUserManagment2.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/reports")]
    public class ReportsApiController : ControllerBase
    {
        private readonly UserManager<Users> userManager;
        private readonly AppDbContext context;

        public ReportsApiController(
            UserManager<Users> userManager,
            AppDbContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        [HttpGet("registrations")]
        public async Task<IActionResult> GetRegistrationsReport()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-6);
            var endDate = today.AddDays(1);

            var users = await userManager.Users
                .Where(user => user.CreatedAt >= startDate && user.CreatedAt < endDate)
                .ToListAsync();

            var groupedUsers = users
                .GroupBy(user => user.CreatedAt.Date)
                .ToDictionary(group => group.Key, group => group.Count());

            var result = new List<RegistrationReportDto>();

            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);

                result.Add(new RegistrationReportDto
                {
                    Day = date.ToString("ddd"),
                    Date = date.ToString("dd/MM"),
                    Count = groupedUsers.ContainsKey(date) ? groupedUsers[date] : 0
                });
            }

            return Ok(result);
        }

        [HttpGet("users-by-role")]
        public async Task<IActionResult> GetUserCountByRole()
        {
            var result = new List<RoleCountReportDto>();

            var connection = context.Database.GetDbConnection();

            await using var command = connection.CreateCommand();

            command.CommandText = "dbo.GetUserCountByRole";
            command.CommandType = CommandType.StoredProcedure;

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new RoleCountReportDto
                {
                    RoleName = reader["RoleName"].ToString() ?? string.Empty,
                    UserCount = Convert.ToInt32(reader["UserCount"])
                });
            }

            return Ok(result);
        }
    }
}