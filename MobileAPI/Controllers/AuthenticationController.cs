using BackendTemplate.BLL.Authentication;
using BLL.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _confugration;

        public AuthenticationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration confugration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._confugration = confugration;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseDTO<string>("User Already Exists", null));
            }
            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Username,
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            var result = await userManager.CreateAsync(user,model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseDTO<string>("User Register Failed", null));
            }
            return Ok(new APIResponseDTO<string>("success", null));
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var userExists = await userManager.FindByNameAsync(model.Username);
                if (userExists != null && await userManager.CheckPasswordAsync(userExists, model.Password))
                {
                    var userRoles = await userManager.GetRolesAsync(userExists);
                    var authClaims = new List<Claim> {
                    new Claim(ClaimTypes.Name,model.Username),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                    foreach (var userrole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userrole));
                    }
                    var authSignKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_confugration["JWT:Secret"]));
                    var token = new JwtSecurityToken(
                        issuer: _confugration["JWT:ValidIssuer"],
                        audience: _confugration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddDays(1),
                        claims: authClaims, signingCredentials: new SigningCredentials(authSignKey, SecurityAlgorithms.HmacSha256));
                    return Ok(new APIResponseDTO<string>("success", new JwtSecurityTokenHandler().WriteToken(token)));
                }
                return Unauthorized();

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseDTO<string>(ex.ToString(), null));
                //throw;
            }
            
        }

        //public async Task<IActionResult> GetData()
        //{
        //    OracleConnection connPRD = new OracleConnection();
        //    connPRD.ConnectionString = _confugration.GetConnectionString("ConnStr");
        //    OracleCommand selectcmd = new OracleCommand(@"", connPRD);

        //    OracleDataAdapter UserAdapter = new OracleDataAdapter(selectcmd);
        //    DataSet ds = new DataSet();
        //    UserAdapter.Fill(ds);
        //    conn.Close();
        //}
    }
}
