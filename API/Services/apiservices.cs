using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace API.Services
{
        public  class GetVoiceRequest
    {
        public string template1 { get; set; }
        public string template2 { get; set; }
    }
    public class apiservices
    {
        public async Task<string> Get_voice_template(string filepath)
        {
            var client = new RestClient("https://voice-rest-api.idrnd.net/voice_template_factory/create_voice_template_from_file?channel_type=TEL");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("x-api-key", "hkXVNav9gG67bETYEa3TS8imx12ljSYUqMEsWaEg");
            request.AddHeader("Content-Type", "multipart/form-data");
            request.AddFile("wav_file", filepath);
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
            request.AddParameter("application/json",jsonString, ParameterType.RequestBody);
            IRestResponse response =  client.Execute(request);
            return await Task.FromResult(response.Content.ToString());
        }
    }
}