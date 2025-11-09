using AuthECAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthECAPI.Controllers
{
    public class UserRegistrationModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class LoginModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public static class IdentityUserEndpoints
    {
        public static IEndpointRouteBuilder MapIdentityUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/signUp", CreateUser);
            app.MapPost("/signin", SignIn);
            return app;
        }


        [AllowAnonymous]
        private static async Task<IResult> CreateUser(
            UserManager<AppUser> userManager,
            [FromBody] UserRegistrationModel userRegistrationModel)
            {
                AppUser user = new AppUser()
                {
                    UserName = userRegistrationModel.Email,
                    Email = userRegistrationModel.Email,
                    FullName = userRegistrationModel.FullName,
                };
                var result = await userManager.CreateAsync(
                    user,
                    userRegistrationModel.Password);

                if (result.Succeeded)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result.Errors);
        }

        [AllowAnonymous]
        private static async Task<IResult> SignIn(
           UserManager<AppUser> userManager,
                [FromBody] LoginModel loginModel,
                IOptions<AppSettings> appSettings)
        {
            if (string.IsNullOrEmpty(loginModel.Email))
            {
                return Results.BadRequest(new { message = "Email is required." });
            }
            if (string.IsNullOrEmpty(loginModel.Password))
            {
                return Results.BadRequest(new { message = "Password is required." });
            }
            var user = await userManager.FindByEmailAsync(loginModel.Email);
            if (user != null && await userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                if (string.IsNullOrEmpty(appSettings.Value.JWTSecret))
                {
                    return Results.BadRequest(new { message = "JWTSecret is not configured." });
                }

                var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Value.JWTSecret));

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserId", user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(10),
                    SigningCredentials = new SigningCredentials(
                        signInKey,
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Results.Ok(token);

            }
            else
            {
                return Results.BadRequest(new { message = "User name or password is incorrect." });
            }
        }


    }
}
