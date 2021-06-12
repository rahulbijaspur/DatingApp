using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly apiservices _apiservices;

        public AccountController(DataContext context, ITokenService tokenService, apiservices apiservices)
        {
            _apiservices = apiservices;
            _tokenService = tokenService;
            _context = context;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromForm] IFormCollection form)
        {
            // if (registerDto.Username.Length<=0)
            //     return BadRequest("request not properly sent ,refresh the page and try");

            if (form.Files.Count == 0)
                return BadRequest("please enter all the fields");

            MemoryStream stream = new MemoryStream();
            IFormFile file = form.Files[0];
            file.CopyTo(stream);
            byte[] arr = stream.ToArray();
            System.IO.File.WriteAllBytes(@"E:\register.wav", arr);
            string voiceprint = await _apiservices.Get_voice_template(@"E:\register.wav");

            var data = form["details"];
            RegisterDto registerDto = JsonConvert.DeserializeObject<RegisterDto>(data);
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            using var hamc = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hamc.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
                PasswordSalt = hamc.Key,
                Voiceprint = voiceprint
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)

            };
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromForm] IFormCollection form)
        {

            var data = form["details"];
            LoginDto logindto = JsonConvert.DeserializeObject<LoginDto>(data);
            var user = await _context.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == logindto.Username);
            if (user == null)
            {
                // return Unauthorized("Invalid Username");
                MemoryStream stream = new MemoryStream();
                IFormFile file = form.Files[0];
                file.CopyTo(stream);
                byte[] arr = stream.ToArray();
                System.IO.File.WriteAllBytes(@"E:\login.wav", arr);
                string voiceprint_login = await _apiservices.Get_voice_template(@"E:\login.wav");

                foreach (AppUser item in await _context.Users.ToListAsync())
                {
                    if (item.Voiceprint != null)
                    {
                        dynamic result = JsonConvert.DeserializeObject(await _apiservices.Get_match(item.Voiceprint, voiceprint_login));
                        if (result.probability > 0.75)
                        {
                            
                            return new UserDto
                            {
                                Username = item.UserName,
                                Token = _tokenService.CreateToken(item),
                                PhotoUrl=user.Photos.FirstOrDefault(x =>x.IsMain)?.Url
                            };
                        }

                    }
                }


            }
            if (logindto.password.Length > 0)
            {
                using var hmac = new HMACSHA512(user.PasswordSalt);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(logindto.password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
                }
                return new UserDto
                {
                    Username = user.UserName,
                    Token = _tokenService.CreateToken(user),
                    PhotoUrl=user.Photos.FirstOrDefault(x =>x.IsMain)?.Url
                };

            }
            else
            {
                MemoryStream stream = new MemoryStream();
                IFormFile file = form.Files[0];
                file.CopyTo(stream);
                byte[] arr = stream.ToArray();
                System.IO.File.WriteAllBytes(@"E:\login.wav", arr);
                string voiceprint_login = await _apiservices.Get_voice_template(@"E:\login.wav");
                string voice_user = user.Voiceprint;
                dynamic result = JsonConvert.DeserializeObject(await _apiservices.Get_match(voice_user, voiceprint_login));
                if (result.probability > 0.75)
                {
                    return new UserDto
                    {
                        Username = user.UserName,
                        Token = _tokenService.CreateToken(user),
                        PhotoUrl=user.Photos.FirstOrDefault(x =>x.IsMain)?.Url
                    };
                }
                return BadRequest("please enter passoword correctly or check with voice login");
            }
        }


        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}