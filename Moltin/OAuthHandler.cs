using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Moltin
{
    public enum HttpMethod { GET, POST, PUT, DELETE }

    public class OAuthHandler
    {
        // Private variables
        private readonly string publicKey;
        private readonly string secretKey;

        /// <summary>
        /// Handles authencated calls made to the Moltin API.
        /// </summary>
        /// <param name="provider">A provider with generic functions that relate to OAuth.</param>
        /// <param name="authUrl">URL used to get the access token.</param>
        /// <param name="baseUrl">URL used for all other API requests.</param>
        /// <param name="version">API version which is appended to the base URL to make a full URL.</param>
        /// <param name="publicKey">Your public key.</param>
        /// <param name="secretKey">Your secret key.</param>
        public OAuthHandler(string publicKey, string secretKey)
        {
            this.publicKey = publicKey;
            this.secretKey = secretKey;
        }

        /// <summary>
        /// Get the access token.
        /// </summary>
        /// <param name="url">The authorization url.</param>
        /// <returns></returns>
        public async Task<string> GetAccessTokenAsync(string url)
        {

            // Create our pairs
            var pairs = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", this.publicKey},
                {"client_secret", this.secretKey}
            };

            // Encode our content
            var data = new FormUrlEncodedContent(pairs);

            // Using the HttpClient
            using (var client = new HttpClient())
            {

                // Get our response
                var response = await client.PostAsync(url, data);
                
                // If we suceed
                if (response.IsSuccessStatusCode)
                {

                    // Read our results
                    var resultString = await response.Content.ReadAsStringAsync();

                    // Get our resolve
                    var jsonObject = JObject.Parse(resultString);

                    // Return our result
                    return jsonObject["access_token"].ToString();
                }

                // Return null if we get this far
                return null;
            }
        }

        /// <summary>
        /// Query the API using GET HttpMethod.
        /// </summary>
        /// <param name="accessToken">The access token used to autenticate the request.</param>
        /// <param name="url">The path to the requested resource.</param>
        /// <returns></returns>
        public async Task<JToken> QueryApiAsync(string accessToken, string url)
        {
            return await QueryApiAsync(accessToken, url, HttpMethod.GET);
        }

        /// <summary>
        /// Query the API without passing data.
        /// </summary>
        /// <param name="accessToken">The access token used to autenticate the request.</param>
        /// <param name="url">The path to the requested resource.</param>
        /// <param name="method">The HttpMethod to use for the call.</param>
        /// <returns></returns>
        public async Task<JToken> QueryApiAsync(string accessToken, string url, HttpMethod method)
        {
            return await QueryApiAsync(accessToken, url, method, null);
        }

        /// <summary>
        /// Query the API using the specified HttpMethod.
        /// </summary>
        /// <param name="accessToken">The access token used to autenticate the request.</param>
        /// <param name="url">The path to the requested resource.</param>
        /// <param name="method">The HttpMethod to use for the call.</param>
        /// <param name="data">The data to be set to the API.</param>
        /// <returns></returns>
        public async Task<JToken> QueryApiAsync(string accessToken, string url, HttpMethod method, Object data)
        {

            // Using a new WebClient
            using (var client = new HttpClient())
            {

                // If we have an access token, apply it to our header
                if (!string.IsNullOrEmpty(accessToken))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                // Create our response
                HttpResponseMessage response;

                // Switch method
                switch (method)
                {

                    // For creating
                    case HttpMethod.POST:

                        // Post our data
                        response = await client.PostAsJsonAsync(url, data);

                        // Break out of the switch
                        break;

                    // For Updating
                    case HttpMethod.PUT:

                        response = await client.PutAsJsonAsync(url, data);

                        // Break out of the switch
                        break;

                    // For Deleting
                    case HttpMethod.DELETE:

                        response = await client.DeleteAsync(url);

                        // Break out of the switch
                        break;

                    // For everything else
                    default:

                        // For get requests
                        response = await client.GetAsync(url);

                        // Break out of the switch
                        break;
                }

                // Return our response
                return await HandleResponse(response);
            }
        }

        /// <summary>
        /// Used to handle any responses
        /// </summary>
        /// <param name="response">The HttpResponseMessage</param>
        /// <returns>A JToken Object</returns>
        static async Task<JToken> HandleResponse(HttpResponseMessage response)
        {

            // If we suceed
            if (response.IsSuccessStatusCode)
            {

                // Read our results
                var resultString = await response.Content.ReadAsStringAsync();

                // Get our resolve
                var jsonObject = JObject.Parse(resultString);

                // Return our result
                return jsonObject["result"];
            }

            // Return nothing if there is an error
            return null;
        }
    }
}
