using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using Newtonsoft.Json;  
namespace TodoApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {   
        private const string API_END_POINT = "http://192.168.0.3:8000/v0";
        private const string ACCESS_KEY = "U_td7CHRolIzIUF65m6s1e2EERUgICvcCapB04kIjyahWc_nLjEGo4Qpk3hedGRe";
        private const string PRIVATE_KEY = "v4xBRNyhHOu8obaTjy-EG0nwlakPeKQdRDppuhjnUKhcPnap8U22QpoukYkWwNUd";
        private static readonly HttpClient client = new HttpClient();

        private static string CreateJwt() {
            TimeSpan diff = DateTime.Now - new DateTime(1970, 1, 1);
            var nonce = Convert.ToInt64(diff.TotalMilliseconds);
            var payload = new JwtPayload
            {
                { "access_key", ACCESS_KEY },
                { "nonce", nonce }
            };
            byte[] keyBytes = Encoding.Default.GetBytes(PRIVATE_KEY);
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyBytes);
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, "HS256");
            var header = new JwtHeader(credentials);
            var secToken = new JwtSecurityToken(header, payload);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(secToken);
            return jwtToken;
        }
        private static string MakeRequest(string httpMethod, string route, Dictionary<string, string> postParams = null)
        {
            using (var client = new HttpClient())
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod(httpMethod), $"{API_END_POINT}/{route}");
                requestMessage.Headers.Add("Authrization", "Bearer " + CreateJwt());
                if (postParams != null)
                    requestMessage.Content = new FormUrlEncodedContent(postParams);   // This is where your content gets added to the request body


                HttpResponseMessage response = client.SendAsync(requestMessage).Result;

                string apiResponse = response.Content.ReadAsStringAsync().Result;
                try
                {
                    // Attempt to deserialise the reponse to the desired type, otherwise throw an expetion with the response from the api.
                    if (apiResponse != "")
                        return apiResponse;
                    else
                        throw new Exception();
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error ocurred while calling the API. It responded with the following message: {response.StatusCode} {response.ReasonPhrase}");
                }
            }
        }
        


        [HttpGet("login")]
        public string Login([FromQuery]string gxc_account_name)
        {
            
            var values = new Dictionary<string, string> {
                {"gxc_account_name", gxc_account_name},
                {"callback_url", "hi"}
            };
            var res = MakeRequest("POST", "game/gxcrogue/login/", values);
            
            return res;
        }

        [HttpGet("login_verify")]
        public string LoginVerify([FromQuery]string gxc_account_name)
        {
            
            var values = new Dictionary<string, string> {
                {"gxc_account_name", gxc_account_name},
            };
            var res = MakeRequest("POST", "game/gxcrogue/login_verify/", values);
            return res;
        }
    }
}
