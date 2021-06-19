using System;
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
        public apiservices(DataContext context)
        {
            _context = context;
        }

        public string wavfileCreate(IFormFile file){
            MemoryStream stream = new MemoryStream();
            file.CopyTo(stream);
            byte[] arr = stream.ToArray();
            System.IO.File.WriteAllBytes(@"E:\register.wav", arr);
            string path ="E:\register.wav";
            return Path.GetFullPath(path);

        }

        public async Task<string> Get_voice_template(string path)
        {

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
            foreach (AppUser item in await _context.Users.ToListAsync())
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
                System.IO.File.WriteAllText(@"E:\voice sample collection\identificationList.txt", res);
            }


        }

        // public async Task<bool> ifUseralreadyExists(string path)
        // {
        //     string voiceprint_login = await Get_voice_template(path);

        //     iden_voice_temp some = new iden_voice_temp();
        //     some.identification_list= Get_identification_list();
        //     some.voice_template=Get_voice_template(path);
        //     string resultArr = await Get_identification_result_array();
        //     scores_thres_arr json = JsonConvert.DeserializeObject<scores_thres_arr>(resultArr);
        //     float m = json.scores.Max();
        //     threshold = json.threshold;
        //     if (m > 1 && json.threshold > 0)
        //     {
        //         return true;

        //     }


        //     return false;
        // }



        public async void enrichIdentificationList(string[] voicetemplatelist)
        {
            var idenlist = Get_identification_list();
            if (idenlist == "")
            {
                createIdentificationList();
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
                System.IO.File.WriteAllText(@"E:\voice sample collection\identificationList.txt", res);
            }

        }

        public string Get_identification_list()
        {
            return System.IO.File.ReadAllText(@"E:\voice sample collection\identificationList.txt");
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


        public checkQuality getcheck_quality_from_file(string path)
        {
            var client = new RestClient("https://voice-rest-api.idrnd.net/voice_template_factory/check_quality_from_file");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddFile("wav_file", @"E:\register.wav");
            IRestResponse response = client.Execute(request);
            checkQuality myDeserializedClass = JsonConvert.DeserializeObject<checkQuality>(response.Content);
            return myDeserializedClass;
        }

        public int[] Get_indexes_of_matched_result(scores_thres_arr x)
        {
            string jsonString = JsonConvert.SerializeObject(x);
            var client = new RestClient("https://voice-rest-api.idrnd.net/identification_result/get_indexes_of_matched_templates");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", x, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            
            int[] Aint = Array.ConvertAll(response.Content.ToCharArray(), c => (int)Char.GetNumericValue(c));
            return Aint;

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