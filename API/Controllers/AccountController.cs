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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace API.Controllers
{
    public class AccountController : BaseApiController
    {

        private readonly ITokenService _tokenService;
        private readonly apiservices _apiservices;
        private readonly IMapper _mapper;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, apiservices apiservices, IMapper mapper, ILogger<AccountController> logger)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _apiservices = apiservices;
            _tokenService = tokenService;


        }

        [HttpGet("getsome")]
        public AppUser Getsome()
        {
            try
            {
                return _userManager.Users.FirstOrDefault(x => x.UserName == "lisa");
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation(ex.Message);
                throw ex;
            }
            
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromForm] IFormCollection form)
        {
            // if (registerDto.Username.Length<=0)
            //     return BadRequest("request not properly sent ,refresh the page and try");
            string voiceprint = null;
            if (form.Files.Count != 0)
            {
                // getting the voiceprint from voice 
                IFormFile file = form.Files[0];/* getting the blob file out of form*/
                _apiservices.wavfileCreate(file);
                voiceprint = await _apiservices.Get_voice_template(file);
                checkQuality quality = _apiservices.getcheck_quality_from_file();

                if (!quality.quality_short_description.Equals("OK"))
                {
                    return BadRequest(quality.quality_short_description);
                }

                if (quality.obtained_values.SNR > quality.threshold_values.SNR)
                {
                    return BadRequest("your noise to voice ratio is high");
                }
                string[] arr = new string[1];
                arr[0] = voiceprint;
                string str = _apiservices.Get_identification_list();
                if (str == "")
                {
                    _apiservices.createIdentificationList();

                }
                _apiservices.enrichIdentificationList(arr);

            }

            var data = form["details"];
            RegisterDto registerDto = JsonConvert.DeserializeObject<RegisterDto>(data);
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");
            var user = _mapper.Map<AppUser>(registerDto);
            user.UserName = registerDto.Username.ToLower();
            user.Voiceprint = voiceprint;

            var result = await _userManager.CreateAsync(user, registerDto.password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if (!roleResult.Succeeded) return BadRequest(result.Errors);
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateTokenAsync(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromForm] IFormCollection form)
        {
            _logger.LogInformation("login api called");

            var data = form["details"];
            
            LoginDto logindto = JsonConvert.DeserializeObject<LoginDto>(data);
            if (form.Files.Count == 0 && logindto.Username == "" && logindto.password == "")
            {
                return BadRequest("Nothing entered");
            }
            if (logindto.Username == "" && logindto.password == "")
            {

                IFormFile file = form.Files[0];


                _apiservices.wavfileCreate(file);
                string voiceprint_login = await _apiservices.Get_voice_template(file);
                string idenficationList = _apiservices.Get_identification_list();

                iden_voice_temp scoreArr = new iden_voice_temp();
                scoreArr.identification_list = idenficationList;
                scoreArr.voice_template = voiceprint_login;
                scores_thres_arr resultArr = _apiservices.Get_identification_result_array(scoreArr);
                int[] indxs = _apiservices.Get_indexes_of_matched_result(resultArr);
                var user1 = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.Id == indxs[0] + 1);



                return new UserDto
                {
                    Username = user1.UserName,
                    Token = await _tokenService.CreateTokenAsync(user1),
                    PhotoUrl = user1.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                    KnownAs = user1.KnownAs,
                    Gender = user1.Gender
                };


            }
            var user = await _userManager.Users
           .Include(p => p.Photos)
           .SingleOrDefaultAsync(x => x.UserName == logindto.Username);

            if (user == null)
            {
                return Unauthorized("invalid credentials");
            }

            if (logindto.password.Length > 0)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, logindto.password, false);
                if (!result.Succeeded) return Unauthorized("Incorrect password");

                return new UserDto
                {
                    Username = user.UserName,
                    Token = await _tokenService.CreateTokenAsync(user),
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
                        Token = await _tokenService.CreateTokenAsync(user),
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
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}