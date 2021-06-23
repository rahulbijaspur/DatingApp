using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;

namespace API.Services
{

    public class ThresholdValues
    {
        public double SNR { get; set; }
        public double SpeechLength { get; set; }
    }

    public class ObtainedValues
    {
        public double SNR { get; set; }
        public double SpeechLength { get; set; }
    }

    public class checkQuality
    {
        public ThresholdValues threshold_values { get; set; }
        public ObtainedValues obtained_values { get; set; }
        public string quality_short_description { get; set; }
    }


    public class scores_thres_arr
    {
        public float[] scores { get; set; }
        public float threshold { get; set; }

    }

    public class Indx
    {
        public List<int> MyArray { get; set; }
    }

    public class enrichtemp
    {
        public string identification_list { get; set; }
        public string[] voice_templates { get; set; }
    }
    public class iden_voice_temp
    {
        public string identification_list { get; set; }
        public string voice_template { get; set; }
    }
    public class GetVoiceRequest
    {
        public string template1 { get; set; }
        public string template2 { get; set; }
    }
    public class VoicetempList
    {
        public string[] voice_templates { get; set; }

    }
    public class apiservices
    {
        public static float threshold;
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;
        public apiservices(UserManager<AppUser> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public void wavfileCreate(IFormFile file)
        {
            MemoryStream stream = new MemoryStream();
            file.CopyTo(stream);
            byte[] arr = stream.ToArray();
            System.IO.File.WriteAllBytes(@"..\api\wavfile\register.wav", arr);


        }

        public async Task<string> Get_voice_template(IFormFile file)
        {

            var client = new RestClient("https://voice-rest-api.idrnd.net/voice_template_factory/create_voice_template_from_file?channel_type=TEL");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddFile("wav_file", @"..\api\wavfile\register.wav");
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());
        }

        public async Task<string> Get_match(string s1, string s2)
        {

            var client = new RestClient("https://voice-rest-api.idrnd.net/voice_template_matcher/match_voice_templates");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "application/json");
            GetVoiceRequest obj = new GetVoiceRequest { template1 = s1, template2 = s2 };
            string jsonString = JsonConvert.SerializeObject(obj);
            request.AddParameter("application/json", jsonString, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());
        }


        public async Task<string> createIdentificationList()
        {
            List<string> voicetempArr = new List<string>();
            foreach (AppUser item in await _userManager.Users.ToListAsync())
            {
                if (item.Voiceprint != null && item.Voiceprint != "")
                {
                    voicetempArr.Add(item.Voiceprint);
                }

            }

            string[] arr = voicetempArr.ToArray() as string[];
            var obj = new VoicetempList
            {
                voice_templates = arr
            };
            string jsonString = JsonConvert.SerializeObject(obj);
            var client = new RestClient("https://voice-rest-api.idrnd.net/identification_engine/create_identification_list");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", jsonString, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string res = await Task.FromResult(response.Content.ToString());
                System.IO.File.WriteAllText(@"..\API\wavfile\identificationList.txt", res);
            }
            return Get_identification_list();


        }

        public async Task enrichIdentificationList(string[] voicetemplatelist)
        {
            var idenlist = Get_identification_list();
            if (idenlist == "")
            {
                idenlist = await createIdentificationList();
            }

            enrichtemp temp = new enrichtemp();
            temp.identification_list = idenlist;
            temp.voice_templates = voicetemplatelist;
            string jsonString = JsonConvert.SerializeObject(temp);


            var client = new RestClient("https://voice-rest-api.idrnd.net/identification_engine/enrich_identification_list");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", jsonString, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string res = await Task.FromResult(response.Content.ToString());
                System.IO.File.WriteAllText(@"..\API\wavfile\identificationList.txt", res);
            }
            return;

        }

        public string Get_identification_list()
        {
            return System.IO.File.ReadAllText(@"..\API\wavfile\identificationList.txt");
        }

        public scores_thres_arr Get_identification_result_array(iden_voice_temp socreArr)
        {

            string jsonString = JsonConvert.SerializeObject(socreArr);

            var client = new RestClient("https://voice-rest-api.idrnd.net/identification_engine/identify");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "application/json");

            request.AddParameter("application/json", jsonString, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<scores_thres_arr>(response.Content);

        }


        public checkQuality getcheck_quality_from_file()
        {

            var client = new RestClient("https://voice-rest-api.idrnd.net/voice_template_factory/check_quality_from_file");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddFile("wav_file", @"..\api\wavfile\register.wav");
            IRestResponse response = client.Execute(request);
            checkQuality myDeserializedClass = JsonConvert.DeserializeObject<checkQuality>(response.Content);
            return myDeserializedClass;
        }

        public async Task<int[]> Get_indexes_of_matched_result(scores_thres_arr x)
        {
            string jsonString = JsonConvert.SerializeObject(x);
            var client = new RestClient("https://voice-rest-api.idrnd.net/identification_result/get_indexes_of_matched_templates");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", jsonString, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            string sc = await Task.FromResult(response.Content);
            sc = sc.Substring(1, sc.Length - 2);
            string[] splitted = sc.Split(',').ToArray();
            int[] nums = new int[splitted.Length];
            for (int i = 0; i < splitted.Length; i++)
            {
                nums[i] = int.Parse(splitted[i]);
            }
            return nums;

        }
        public async Task<string> GetSnr(string filepath)
        {
            var client = new RestClient("https://voice-rest-api.idrnd.net/snr_computer/compute_with_file");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddFile("wav_file", filepath);
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());

        }


        public async Task<string> GetRt60(string filepath)
        {
            var client = new RestClient("https://voice-rest-api.idrnd.net/rt60_computer/compute_with_file");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddFile("wav_file", filepath);
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());

        }

        public async Task<string> GetisSpoof(string filepath)
        {
            var client = new RestClient("https://voice-rest-api.idrnd.net/antispoof_engine/is_spoof_file");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddFile("wav_file", filepath);
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());
        }

        public async Task<string> GetSummmary(string filepath)
        {
            var client = new RestClient("https://voice-rest-api.idrnd.net/speech_summary_engine/get_speech_summary_from_file");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddFile("wav_file", filepath);
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());
        }

        public async Task<string> GetEventdetector(string filepath)
        {
            var client = new RestClient("https://voice-rest-api.idrnd.net/event_detector/detect_with_file");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddFile("wav_file", filepath);
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());
        }

        public async Task<string> GetAttributesEstimate(string filepath)
        {
            var client = new RestClient("https://voice-rest-api.idrnd.net/attributes_estimator/estimate_with_file");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddFile("wav_file", filepath);
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());
        }
    }


}