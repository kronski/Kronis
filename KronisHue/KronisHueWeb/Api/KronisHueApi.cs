using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using KronisHue;
using KronisHueWeb.Classes;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KronisHueWeb
{
    [Route("api/kronishue")]
    [ApiController]
    public class KronisHueController : Controller
    {
        private static async Task<IActionResult> Forward(HttpResponseMessage response)
        {
            var res = new ContentResult();
            res.StatusCode = (int)response.StatusCode;
            res.ContentType = response.Content.Headers.GetValues("Content-Type").FirstOrDefault();
            res.Content = await response.Content.ReadAsStringAsync();
            return res;
        }

        // GET: api/<controller>
        [HttpPost("nonce")]
        public async Task<IActionResult> Nonce([FromBody] NonceInput data)
        {
            using (HttpClient c = new HttpClient())
            {
                string url = $"https://api.meethue.com/oauth2/token?code={data.Code}&grant_type=authorization_code";
                var result = await c.PostAsync(url, new StringContent(string.Empty));

                var headers = result.Headers.GetValues("WWW-Authenticate");
                var regex = new Regex("Digest realm=\"([^\"]*)\",\\s*nonce=\"([^\"]*)");

                foreach (var s in headers)
                {
                    var match = regex.Match(s);
                    if (match.Success)
                    {
                        return Ok(match.Groups[2].Value);
                    }
                }
                return NoContent();
            }
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token([FromBody] TokenInput data)
        {

            using (HttpClient c = new HttpClient())
            {
                string response;
                using (MD5 md5 = MD5.Create())
                {
                    string h1 = MD5Calc.GetMd5Hash(md5, $"{AppSecret.ClientID}:oauth2_client@api.meethue.com:{AppSecret.ClientSecret}");
                    string h2 = MD5Calc.GetMd5Hash(md5, "POST:/oauth2/token");
                    response = MD5Calc.GetMd5Hash(md5, h1 + ":" + data.Nonce + ":" + h2);
                }

                string url = $"https://api.meethue.com/oauth2/token?code={data.Code}&grant_type=authorization_code";

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Post,

                };

                string param = $"username=\"{AppSecret.ClientID}\", realm=\"oauth2_client@api.meethue.com\", nonce=\"{data.Nonce}\", uri=\"/oauth2/token\", response=\"{response}\"";

                request.Headers.Authorization = new AuthenticationHeaderValue("Digest", param);

                var result = await c.SendAsync(request);

                if (result.IsSuccessStatusCode)
                {
                    var obj = await result.Content.ReadAsAsync<TokenResult>();
                    return Json(obj);
                }
                else
                    return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenInput data)
        {

            using (HttpClient c = new HttpClient())
            {
                string response;
                using (MD5 md5 = MD5.Create())
                {
                    string h1 = MD5Calc.GetMd5Hash(md5, $"{AppSecret.ClientID}:oauth2_client@api.meethue.com:{AppSecret.ClientSecret}");
                    string h2 = MD5Calc.GetMd5Hash(md5, "POST:/oauth2/token");
                    response = MD5Calc.GetMd5Hash(md5, h1 + ":" + data.Nonce + ":" + h2);
                }

                string url = $"https://api.meethue.com/oauth2/refresh?grant_type=refresh_token";

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Post
                };

                string param = $"username=\"{AppSecret.ClientID}\", realm=\"oauth2_client@api.meethue.com\", nonce=\"{data.Nonce}\", uri=\"/oauth2/token\", response=\"{response}\"";

                request.Headers.Authorization = new AuthenticationHeaderValue("Digest", param);
                request.Content = new StringContent($"refresh_token={data.Token}", Encoding.UTF8, "application/x-www-form-urlencoded");

                var result = await c.SendAsync(request);

                if (result.IsSuccessStatusCode)
                {
                    var obj = await result.Content.ReadAsAsync<TokenResult>();
                    return Json(obj);
                }
                else
                    return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("appinfo")]
        public IActionResult AppInfo()
        {
            return Json(new { clientid= AppSecret.ClientID, appid= AppSecret.AppID });
        }

        [HttpPost("setdevicetype")]
        public async Task<IActionResult> SetDeviceType([FromBody] SetDeviceTypeInput input)
        {
            using (HttpClient c = new HttpClient())
            {
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", input.Token);
                var response = await c.PutAsync("https://api.meethue.com/bridge/0/config",new JsonContent(new { linkbutton=true }));
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode);

                response = await c.PostAsync("https://api.meethue.com/bridge/", new JsonContent(new { devicetype = input.DeviceType }));
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode);
                var stringResponse = await response.Content.ReadAsStringAsync();

                JObject result;
                try
                {
                    JArray jresponse = JArray.Parse(stringResponse);
                    result = (JObject)jresponse.First;
                }
                catch
                {
                    return StatusCode(500, stringResponse);
                }

                if (result.TryGetValue("error", out JToken error))
                {
                    if (error["type"].Value<int>() == 101) // link button not pressed
                        return StatusCode(500, "Link button not pressed");
                    else
                        return StatusCode(500, error["description"].Value<string>());
                }

                var username = result["success"]["username"].Value<string>();
                return Json(new { username });

            }
        }

        [HttpPost("lights")]
        public async Task<IActionResult> GetLights([FromBody] ApiInput input)
        {

            using (HttpClient c = new HttpClient())
            {
                ApplyAuthentication(c,input);
                var response = await c.GetAsync($"{input.ApiUrl}/lights");
                return await Forward(response);
            }
        }

        private static void ApplyAuthentication(HttpClient c, ApiInput input)
        {
            if(input.Token!=null)
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", input.Token);
        }

        [HttpPost("groups")]
        public async Task<IActionResult> GetGroups([FromBody] ApiInput input)
        {
            using (HttpClient c = new HttpClient())
            {
                ApplyAuthentication(c, input);
                var response = await c.GetAsync($"{input.ApiUrl}/groups");
                return await Forward(response);
            }
        }

        [HttpPost("locateHue")]
        public async Task<IActionResult> LocateHue()
        {
            var t = await Task.WhenAll(
                new Task<string[]>[]
                {
                    BridgeLocator.FindHueBridgeViaUpnp(device => 
                    {
                    }
                    ,null),
                    BridgeLocator.FindHueBridgeViaMeetHue()
                }
            );

            HashSet<string> result = new HashSet<string>();
            foreach (var x in t)
                foreach (var s in x)
                    if(!result.Contains(s))
                        result.Add(s);
            return Ok(result.ToArray());
        }

        [HttpPost("registerLocalHue")]
        public async Task<IActionResult> RegisterLocalHue([FromBody] ApiInput input)
        {
            using (HttpClient c = new HttpClient())
            {
                // {"devicetype":"my_hue_app#iphone peter"}
                var response = await c.PostAsync(input.BaseUrl, new JsonContent(new { devicetype = "KronisHueWeb" }));
                return await Forward(response);
            }
        }

        [HttpPut("lights/{id}/state")]
        public async Task<IActionResult> PutLightState([FromBody] LightStateInput input, [FromRoute] int id)
        {
            using (HttpClient c = new HttpClient())
            {
                var uri = $"{input.ApiUrl}/lights/{id}/state";
                ApplyAuthentication(c, input);
                var response = await c.PutAsync(uri, new JsonContent(input.State));
                return await Forward(response);
            }
        }

        [HttpPut("groups/{id}/action")]
        public async Task<IActionResult> PutGroupState([FromBody] GroupActionInput input, [FromRoute] int id)
        {
            using (HttpClient c = new HttpClient())
            {
                var uri = $"{input.ApiUrl}/groups/{id}/action";
                ApplyAuthentication(c, input);

                var response = await c.PutAsync(uri, new JsonContent(input.Action));
                return await Forward(response);
            }
        }
    }

    public class NonceInput
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class TokenInput
    {
        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class RefreshTokenInput : TokenInput
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class SetDeviceTypeInput
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("devicetype")]
        public string DeviceType { get; set; }
    }

    public class ApiInput
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }

        internal string ApiUrl => BaseUrl ?? $"https://api.meethue.com/bridge/{UserName}";
    }

    public class LightStateInput : ApiInput
    {
        [JsonProperty("state")]
        public LightState State { get; set; }
    }

    public class GroupActionInput : ApiInput
    {
        [JsonProperty("action")]
        public GroupAction Action { get; set; }
    }

    public class TokenResult
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("access_token_expires_in")]
        public string AccessTokenExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("refresh_token_expires_in")]
        public string RefreshTokenExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }

    class MD5Calc
    {
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }

    class AppSecret
    {
        public static string ClientID => Environment.GetEnvironmentVariable("kronishue_clientid");
        public static string ClientSecret => Environment.GetEnvironmentVariable("kronishue_clientsecret");
        public static string AppID => Environment.GetEnvironmentVariable("kronishue_clientid");
    }
}
