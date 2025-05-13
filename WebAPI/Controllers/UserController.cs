using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Repository;
using Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Shared.ModelAdd;
using Shared.ModelEdit;
using Shared.SearchModel;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;
        private readonly IGenericRepository<AdviceRules> _adviceRulesRepository;
        private readonly IConfiguration _configuration;
        public UserController(IGenericRepository<User> userRepository, IGenericRepository<RefreshToken> refreshTokenRepository, IGenericRepository<AdviceRules> adviceRulesRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _adviceRulesRepository = adviceRulesRepository;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("get-all-with-filter")]
        public async Task<IActionResult> GetAllUsersWithFilter(UserSearchModel userSearchModel)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                if (userSearchModel.Id != Guid.Empty)
                {
                    users = users.Where(x => x.Id == userSearchModel.Id).ToList();
                }
                if (!string.IsNullOrEmpty(userSearchModel.FullName))
                {
                    users = users.Where(x => x.FullName.ToLower().Contains(userSearchModel.FullName.ToLower())).ToList();
                }
                if (!string.IsNullOrEmpty(userSearchModel.Email))
                {
                    users = users.Where(x => x.Email.ToLower().Contains(userSearchModel.Email.ToLower())).ToList();
                }
                if (!string.IsNullOrEmpty(userSearchModel.UserName))
                {
                    users = users.Where(x => x.UserName.ToLower().Contains(userSearchModel.UserName.ToLower())).ToList();
                }
                if (!string.IsNullOrEmpty(userSearchModel.Role))
                {
                    users = users.Where(x => x.Role == userSearchModel.Role).ToList();
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("get-page-data-with-filter")]
        public async Task<IActionResult> GetPageDataUsersWithFilter(UserSearchModel userSearchModel)
        {
            try
            {
                int pageSize = 8; 
                int pageIndex = userSearchModel.pageIndex;
                int totalCount = 0;

                var users = await _userRepository.GetAllAsync();
                if (userSearchModel.Id != Guid.Empty)
                {
                    users = users.Where(x => x.Id == userSearchModel.Id).ToList();
                }
                if (!string.IsNullOrEmpty(userSearchModel.FullName))
                {
                    users = users.Where(x => x.FullName.ToLower().Contains(userSearchModel.FullName.ToLower())).ToList();
                }
                if (!string.IsNullOrEmpty(userSearchModel.Email))
                {
                    users = users.Where(x => x.Email.ToLower().Contains(userSearchModel.Email.ToLower())).ToList();
                }
                if (!string.IsNullOrEmpty(userSearchModel.UserName))
                {
                    users = users.Where(x => x.UserName.ToLower().Contains(userSearchModel.UserName.ToLower())).ToList();
                }
                if (!string.IsNullOrEmpty(userSearchModel.Role))
                {
                    users = users.Where(x => x.Role == userSearchModel.Role).ToList();
                }
                users = users.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                totalCount = users.Count();
                return Ok(new
                {
                    users = users,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = totalCount,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] UserAddModel addUserModel)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var userExist = users.FirstOrDefault(u => u.UserName == addUserModel.UserName);
                if (userExist == null)
                {
                    User user = new User
                    {
                        FullName = addUserModel.FullName,
                        Email = addUserModel.Email,
                        UserName = addUserModel.UserName,
                        Password = addUserModel.Password,
                        Role = "Student"
                    };
                    await _userRepository.AddAsync(user);
                    users = await _userRepository.GetAllAsync();
                    var userInDb = users.FirstOrDefault(x => x.UserName == user.UserName && x.Password == user.Password);
                    if (userInDb == null)
                    {
                        return BadRequest("User not found in the database.");
                    }
                    AdviceRules adviceRules = new AdviceRules
                    {
                        UserId = userInDb.Id,
                        Role = userInDb.Role
                    };
                    await _adviceRulesRepository.AddAsync(adviceRules);
                    return Ok(new { message = "User added successfully" });
                }
                return BadRequest("Username already exist");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Student")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UserEditModel user)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var userInDb = users.FirstOrDefault(x => x.Id == user.Id);
                if (userInDb == null)
                {
                    return BadRequest("User not found in the database.");
                }
                userInDb.FullName = user.FullName;
                userInDb.Email = user.Email;
                userInDb.UserName = user.UserName;
                userInDb.Password = user.Password;
                await _userRepository.UpdateAsync(userInDb);
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("admin-update")]
        public async Task<IActionResult> AdminUpdateUser([FromBody] User user)
        {
            try
            {
                await _userRepository.UpdateAsync(user);
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromBody] User user)
        {
            try
            {
                await _userRepository.DeleteAsync(user);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            List<User> users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(x => x.UserName == request.UserName && x.Password == request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid name or password");
            }
            else
            {
                var accesssToken = GenerateJwtToken(user, 60*24);
                var refreshToken = GenerateJwtToken(user, 60 * 24 * 7);
                var newRefreshToken = new RefreshToken
                {
                    Token = refreshToken,
                    ExpirationDate = DateTime.UtcNow.AddDays(7),
                    UserId = user.Id,
                    IsActive = true,
                };
                try
                {
                    await _refreshTokenRepository.AddAsync(newRefreshToken);
                }
                catch (Exception ex)
                {
                    return BadRequest("Error while saving refresh token: " + ex.Message);
                }
                return Ok(accesssToken);
            }
        }

        [Authorize(Policy = "Student")]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var allRefreshTokens = await _refreshTokenRepository.GetAllAsync();
            var tokenInDb = allRefreshTokens.FirstOrDefault(t => t.Token == refreshToken);

            if (tokenInDb == null || !tokenInDb.IsActive || tokenInDb.ExpirationDate < DateTime.UtcNow)
            {
                return Unauthorized("Refresh token is invalid or expired.");
            }

            var allUsers = await _userRepository.GetAllAsync();
            var user = allUsers.FirstOrDefault(u => u.Id == tokenInDb.UserId);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var newAccessToken = GenerateJwtToken(user, 5);

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = tokenInDb.Token
            });
        }

        private string GenerateJwtToken(User user, int time)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim("fullname", user.FullName),
            new Claim(ClaimTypes.Role, user.Role),

        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(time),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}