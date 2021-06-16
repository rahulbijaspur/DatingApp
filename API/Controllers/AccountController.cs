using System;
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
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly apiservices _apiservices;
        private readonly IMapper _mapper;

        public AccountController(DataContext context, ITokenService tokenService, apiservices apiservices, IMapper mapper)
        {
            _mapper = mapper;
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
            var data = form["details"];

            // getting the voiceprint from voice 
            IFormFile file = form.Files[0];/* getting the blob file out of form*/
            string voiceprint = await _apiservices.Get_voice_template(file);


            RegisterDto registerDto = JsonConvert.DeserializeObject<RegisterDto>(data);
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            var user = _mapper.Map<AppUser>(registerDto);

            using var hamc = new HMACSHA512();
            user.UserName = registerDto.Username.ToLower();
            user.PasswordHash = hamc.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password));
            user.PasswordSalt = hamc.Key;
            user.Voiceprint = voiceprint;


            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender

            };
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromForm] IFormCollection form)
        {

            var data = form["details"];
            LoginDto logindto = JsonConvert.DeserializeObject<LoginDto>(data);
            if (logindto.Username == "" && logindto.password == "")
            {
                // return Unauthorized("Invalid Username");

                IFormFile file = form.Files[0];
                string voiceprint_login = await _apiservices.Get_voice_template(file);
                _apiservices.createIdentificationList();

                string idenficationList = _apiservices.Get_identification_list();

                string resultArr = await _apiservices.Get_identification_result_array(idenficationList, voiceprint_login);
                scores_thres_arr json = JsonConvert.DeserializeObject<scores_thres_arr>(resultArr);

                float m = json.scores.Max();
                int indx = Array.IndexOf(json.scores, m);

                var user1 = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.Id == indx + 1);


                return new UserDto
                {
                    Username = user1.UserName,
                    Token = _tokenService.CreateToken(user1),
                    PhotoUrl = user1.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                    KnownAs = user1.KnownAs,
                    Gender = user1.Gender
                };


                // foreach (AppUser item in await _context.Users.ToListAsync())
                // {
                //     if (item.Voiceprint != null)
                //     {
                //         dynamic result = JsonConvert.DeserializeObject(await _apiservices.Get_match(item.Voiceprint, voiceprint_login));
                //         if (result.probability > 0.75)
                //         {
                //             var user1 = await _context.Users
                //                     .Include(p => p.Photos)
                //                        .SingleOrDefaultAsync(x => x.UserName == item.UserName);

                //             return new UserDto
                //             {
                //                 Username = item.UserName,
                //                 Token = _tokenService.CreateToken(item),
                //                 PhotoUrl = item.Photos.FirstOrDefault(x => x.IsMain)?.Url
                //             };
                //         }

                //     }
                // }


            }
            var user = await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == logindto.Username);
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
                    PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                    KnownAs = user.KnownAs,
                    Gender = user.Gender
                };

            }
            else
            {

                IFormFile file = form.Files[0];
                string voiceprint_login = await _apiservices.Get_voice_template(file);
                string voice_user = user.Voiceprint;
                dynamic result = JsonConvert.DeserializeObject(await _apiservices.Get_match(voice_user, voiceprint_login));
                if (result.probability > 0.75)
                {
                    return new UserDto
                    {
                        Username = user.UserName,
                        Token = _tokenService.CreateToken(user),
                        PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                        KnownAs = user.KnownAs,
                        Gender = user.Gender
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