using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Sample.Pages
{
    public class callbackModel : PageModel
    {
        readonly IConfiguration configuration;

        public callbackModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task OnGet()
        {
            var clientId = configuration["clientId"]; // we are using client credentials flow here (and different client id)
            var clientSecret = configuration["clientSecret"];

            var clientCreds = Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}");
            var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(clientCreds));

            var code = Request.Query["code"];
            var state = Request.Query["state"];

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = configuration["callbackURL"]
            };

            var formContent = new FormUrlEncodedContent(form);

            var tokenEndpoint = configuration["tokenEndpoint"];

            string refreshToken = null;

            using var client = new HttpClient();

            using var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = formContent,
                RequestUri = new Uri(tokenEndpoint)
            };

            requestMessage.Headers.Authorization = authHeader;

            using var resp = await client.SendAsync(requestMessage);

            if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorMsg = await resp.Content.ReadAsStringAsync();
            }

            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();

            using var jsonDoc = JsonDocument.Parse(json);

            var idToken = jsonDoc.RootElement.GetProperty("id_token").ToString();
            refreshToken = jsonDoc.RootElement.GetProperty("refresh_token").ToString(); // STORE THIS, this is a LONG LIVED TOKEN
            var accessToken = jsonDoc.RootElement.GetProperty("access_token").ToString();

            var handler = new JwtSecurityTokenHandler();

            var idTokenObj = handler.ReadJwtToken(idToken); // parse if need be. just for demonstration purposes.

            // FOR DEMONSTRATION PURPOSES, we'll get an access token immediately. in real life access token should be requested at the point of use
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await DemoGetAccessToken(tokenEndpoint, refreshToken, client, authHeader);
            }
        }

        private async Task DemoGetAccessToken(string tokenEndpoint, string refreshToken, HttpClient client, AuthenticationHeaderValue authHeader)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["scope"] = "openid offline_access",
                ["refresh_token"] = refreshToken,
                ["redirect_uri"] = configuration["callbackURL"]
            };

            var formContent = new FormUrlEncodedContent(form);
          
            using var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = formContent,
                RequestUri = new Uri(tokenEndpoint)
            };

            requestMessage.Headers.Authorization = authHeader;

            using var resp = await client.SendAsync(requestMessage);

            if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorMsg = await resp.Content.ReadAsStringAsync();
            }

            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();

            using var jsonDoc = JsonDocument.Parse(json);

            var idToken = jsonDoc.RootElement.GetProperty("id_token").ToString();
            var refreshTokenNew = jsonDoc.RootElement.GetProperty("refresh_token").ToString(); // THIS MAY CHANGE incase the old refresh token expired, etc
            var accessToken = jsonDoc.RootElement.GetProperty("access_token").ToString();

            var changed = !refreshToken.Equals(refreshTokenNew, StringComparison.OrdinalIgnoreCase);
        }
    }
}
