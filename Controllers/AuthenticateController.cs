using LearmApi.Dto;
using LearmApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LearmApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {

        private readonly EntityContext _context;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;


        public AuthenticateController(
       UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, EntityContext context,
       IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }



        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            var userExists = await _userManager.FindByNameAsync(userDto.Username);
            if (userExists != null)
            {

                return BadRequest(new { message = "User already exists", statuCode = 401 });
            }


            userExists = await _userManager.FindByEmailAsync(userDto.Email);
            if (userExists != null)
            {

                return NotFound("Email  already exists");
            }

            IdentityUser user = new()
            {
                Email = userDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = userDto.Username
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest("not Datta");
            }

            if (!await _roleManager.RoleExistsAsync(Role.Admin.ToString()))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role.Admin.ToString()));
            }

            if (!await _roleManager.RoleExistsAsync(Role.Manager.ToString()))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role.Manager.ToString()));
            }

            if (await _roleManager.RoleExistsAsync(Role.User.ToString()))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role.User.ToString()));
                await _userManager.AddToRoleAsync(user, Role.User.ToString());
            }
            else
            {
                await _userManager.AddToRoleAsync(user, Role.User.ToString());
            }


            return Ok("User created successfull");
        }



        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _userManager.FindByNameAsync(login.Username!);


            if (user != null && await _userManager.CheckPasswordAsync(user, login.Password!))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                return Ok(new
                {

                    data = new
                    {
                        accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        username = user.Email,
                        role = userRoles
                    },
                    statusCode = 200,

                    exp = token.ValidTo


                });
            }

            return Unauthorized();

        }


        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows time zone ID

            // Get the current time in Bangkok time zone
            var currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneInfo);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: currentTime.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }






        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;
            if (userName != null)
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user != null)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                    return Ok(new { message = "User logged out!", statusCode = 200 });
                }
            }
            return Ok();
        }



        [HttpPost]
        [Route("refreshtoken")]
        public async Task<IActionResult> RefreshToken()
        {
            // Validate Authorization header
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader) ||
                string.IsNullOrWhiteSpace(authHeader))
            {
                return Unauthorized(new { message = "Authorization header is missing." });
            }

            var bearerToken = authHeader.ToString();
            if (!bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Invalid authorization scheme." });
            }

            var token = bearerToken[7..]; // Using range operator instead of Substring
            var tokenHandler = new JwtSecurityTokenHandler();

            // Ensure configuration is available
            var jwtSecret = _configuration["JWT:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                return StatusCode(500, new { message = "JWT configuration is missing." });
            }

            var key = Encoding.ASCII.GetBytes(jwtSecret);

            try
            {
                // Validate the token
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false, // Don't validate lifetime for refresh token
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                // Extract user claims
                var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
                var userRole = principal.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(new { message = "Invalid token claims." });
                }

                // Create new claims for the refreshed token
                var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, userRole),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString())
        };

                // Generate new token
                var newToken = GetToken(authClaims);

                return Ok(new
                {
                    token = tokenHandler.WriteToken(newToken),
                    expiration = newToken.ValidTo,
                    type = "Bearer"
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = "Invalid token.", error = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, new { message = "An error occurred while processing the request." });
            }
        }

        // ฟังก์ชันสร้าง JWT ใหม่ (คุณอาจจะมีฟังก์ชันนี้อยู่แล้ว)


        // Model สำหรับ Refresh Token
        public class RefreshTokenModel
        {
            public required string Token { get; set; }
        }


    }
}
