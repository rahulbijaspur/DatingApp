using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;

namespace API.Services
{

    public class scores_thres_arr
    {
        public float[] scores { get; set; }
        public float threshold { get; set; }

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
        private readonly DataContext _context;
        public apiservices(DataContext context)
        {
            _context = context;
        }

        public async Task<string> Get_voice_template(IFormFile file)
        {
            MemoryStream stream = new MemoryStream();
            file.CopyTo(stream);
            byte[] arr = stream.ToArray();
            System.IO.File.WriteAllBytes(@"E:\register.wav", arr);


            var client = new RestClient("https://voice-rest-api.idrnd.net/voice_template_factory/create_voice_template_from_file?channel_type=TEL");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddFile("wav_file", @"E:\register.wav");
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


        public async void createIdentificationList()
        {
            List<string> voicetempArr = new List<string>();
            string dummy = System.IO.File.ReadAllText(@"E:\voice sample collection\adarsh.txt");
            foreach (AppUser item in await _context.Users.ToListAsync())
            {
                if (item.Voiceprint != null && item.Voiceprint != "")
                {
                    voicetempArr.Add(item.Voiceprint);
                }
                else
                {
                    voicetempArr.Add(dummy);
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
            string res = await Task.FromResult(response.Content.ToString());

            System.IO.File.WriteAllText(@"E:\voice sample collection\identificationList.txt", res);

        }

        public string Get_identification_list()
        {
            return System.IO.File.ReadAllText(@"E:\voice sample collection\identificationList.txt");
        }

        public async Task<string> Get_identification_result_array(string idenList, string voiceTemp)
        {
            var obj = new iden_voice_temp
            {
                identification_list = idenList,
                voice_template = voiceTemp
            };
            string jsonString = JsonConvert.SerializeObject(obj);

            var client = new RestClient("https://voice-rest-api.idrnd.net/identification_engine/identify");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "application/json");

            request.AddParameter("application/json", jsonString, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());

        }

        public async Task<string> Get_indexes_of_matched_result(string body)
        {
            var client = new RestClient("https://voice-rest-api.idrnd.net/identification_result/get_indexes_of_matched_templates");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());
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
            request.AddFile("wav_file",filepath);
            IRestResponse response = client.Execute(request);
            return await Task.FromResult(response.Content.ToString());
        }
    }


}