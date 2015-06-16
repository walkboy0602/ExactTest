using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Configuration;
using Web.Models;

namespace Web.Repository
{
    public interface ITokenRepository
    {
        string GetAccessToken();
        //string RefreshAndGetNewToken(Credential currentCredential);
    }

    public class TokenRepository : ITokenRepository
    {
        const string MediaType = "application/json";
        private readonly string _tokenEndpointUrl = string.Format("{0}/api/oauth2/token", WebConfigurationManager.AppSettings["ExactOnlineEndpoint"]);

        public string GetAccessToken()
        {
            Credential credential = HttpContext.Current.Session["Credential"] as Credential;

            var result = "";
            if (credential != null)
            {
                result = credential.AccessToken;

                if (credential.ExpiresAt <= DateTime.Now)
                {
                    result = RefreshAndGetNewToken(credential);
                }
            } 
            return result;
        }

        private string RefreshAndGetNewToken(Credential currentCredential)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", currentCredential.AccessToken);

            var accessToken = "";
            var postData = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("refresh_token", currentCredential.RefreshToken),
				new KeyValuePair<string, string>("grant_type", "refresh_token"),
				new KeyValuePair<string, string>("client_id", WebConfigurationManager.AppSettings["ExactOnlineClientId"]),
				new KeyValuePair<string, string>("client_secret", WebConfigurationManager.AppSettings["ExactOnlineClientSecret"])
			};

            try
            {

                HttpContent content = new FormUrlEncodedContent(postData);
                //Make a new  to get new access token
                var tokenRequestResult = client.PostAsync(_tokenEndpointUrl, content).Result;
                var tokenResult = JsonConvert.DeserializeObject<TokenResult>(tokenRequestResult.Content.ReadAsStringAsync().Result);

                if (tokenResult == null) return accessToken;

                accessToken = tokenResult.AccessToken;

            }
            catch (Exception)
            {
                throw new Exception("There was an error while obtaining the access token.");
            }

            return accessToken;
        }
    }
}