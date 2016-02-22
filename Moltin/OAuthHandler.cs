using Moltin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Moltin
{
    public enum HttpMethod { GET, POST, PUT, DELETE }

    public class OAuthHandler
    {
        // Private variables
        private readonly string publicKey;
        private readonly string secretKey;
        private ModelFactory factory;

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
            this.factory = new ModelFactory();
            
            // Configure our JSON output globally
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None
            };
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

            // If we don't have a access token, throw an error
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException("accessToken");

            // If we don't have a URL, throw an error
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            // Factorize our data
            var model = Factorize(data);

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
                        response = await client.PostAsJsonAsync(url, JObject.FromObject(model));

                        // Break out of the switch
                        break;

                    // For Updating
                    case HttpMethod.PUT:

                        response = await client.PutAsJsonAsync(url, JObject.FromObject(model));

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
        private async Task<JToken> HandleResponse(HttpResponseMessage response)
        {

            // Read our results
            var resultString = await response.Content.ReadAsStringAsync();

            // Get our resolve
            var jsonObject = JObject.Parse(resultString);

            // Get our errors
            var errors = jsonObject["errors"] as JObject;

            // Process any errors
            ProcessErrors(errors);

            // If no errors were caught, ensure that the status code is 200
            response.EnsureSuccessStatusCode();

            // Return our result
            return jsonObject;
        }

        private void ProcessErrors(JObject errors)
        {

            // If we have any errors
            if (errors != null)
            {

                // Create our string builder
                var sb = new StringBuilder();

                // Loop through our errors
                foreach(var error in errors)
                {

                    // Remove double quotes and square brackets from our value
                    var value = Regex.Replace(error.Value.ToString(), @"[\[\]""]+", "");

                    // Create our first item
                    sb.Append(error.Key + ": " + value);
                }

                // Throw an error
                throw new Exception(sb.ToString());
            }
        }

        /// <summary>
        /// Factorizes the data if neccessary, to match the expected definitions for moltin
        /// </summary>
        /// <param name="data">The object that has been sent to the API</param>
        /// <returns></returns>
        private Object Factorize(Object data)
        {

            // If the data that has been sent is the checkout model
            if (data is ICheckoutBindingModel)
            {

                // Factorize our data
                return this.factory.Create(data as ICheckoutBindingModel);
            }

            // Fallback, return the original object (can be null)
            return data;
        }
    }
}
