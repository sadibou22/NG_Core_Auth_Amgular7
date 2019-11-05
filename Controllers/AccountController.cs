using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NG_Core_Auth.Helpers;
using NG_Core_Auth.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NG_Core_Auth.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;

        private readonly SignInManager<IdentityUser> _signManager;

        private readonly AppSettings _appSettings;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signManager,IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _signManager = signManager;
            _appSettings = appSettings.Value;
        }

        [HttpPost("[action]")]
        public async  Task<IActionResult> Register([FromBody] RegisterVM formdata)
        {
            //Will hold all th errors related to registration
            List<string> errorsList = new List<string>();

            var user = new IdentityUser
            {
                Email = formdata.Email,
                UserName = formdata.Username,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(user, formdata.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");

                //Sending Confirmation email TODO

                return Ok(new { username = user.UserName, email = user.Email, status = 1, message = "Registration Succesful"});
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                    errorsList.Add(error.Description);
                }
            }

            return BadRequest(new JsonResult(errorsList));

        }

        //Login
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginVM formdata)
        {
            //get the user from the database
            var user = await _userManager.FindByNameAsync(formdata.Username);

            var roles = await _userManager.GetRolesAsync(user);

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            Double tokenExpireTime = Convert.ToDouble(_appSettings.ExpireTime);

            if (user != null && await _userManager.CheckPasswordAsync(user, formdata.Password))
            {
                //Confirmatio email
                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor {
                    Subject = new ClaimsIdentity(new Claim[] {
                        new Claim(JwtRegisteredClaimNames.Sub, formdata.Username),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim("LoggedOn", DateTime.Now.ToString()),
                    }),

                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSettings.Site,
                    Audience = _appSettings.Audience,
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpireTime),
                };

                //Generate token
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return Ok(new { token = tokenHandler.WriteToken(token), expiration = token.ValidTo, username = user.UserName, userRole = roles.FirstOrDefault()});

            }

            //return error
            ModelState.AddModelError("","Username/Password was not found");
            return Unauthorized(new { LoginError = "Please check the Login Credentials - Invalid Username/Password was entered" });
        }

        private IActionResult Unauthorized(object p)
        {
            throw new NotImplementedException();
        }


        //public int MyProperty { get; set; }
        //// GET: api/<controller>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<controller>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<controller>
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/<controller>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/<controller>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
