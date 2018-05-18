using Moltin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Moltin
{
    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) :
            base(() => Task.Factory.StartNew(valueFactory))
        {
        }

        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Factory.StartNew(taskFactory).Unwrap())
        {
        }
    }

    public class OAuthHandler
    {
        // Private variables
        private readonly string _publicKey;
        private readonly string _secretKey;
        private readonly string _authUrl;

        /// <summary>
        ///     Handles authencated calls made to the Moltin API.
        /// </summary>
        /// <param name="publicKey">Your public key.</param>
        /// <param name="secretKey">Your secret key.</param>
        /// <param name="authUrl"></param>
        public OAuthHandler(string publicKey, string secretKey, string authUrl = "https://api.molt.in/oauth/access_token")
        {
            _publicKey = publicKey;
            _secretKey = secretKey;
            _authUrl = authUrl;

            // Configure our JSON output globally
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None
            };
        }

        /// <summary>
        ///     Query the API using GET HttpMethod.
        /// </summary>
        /// <param name="url">The path to the requested resource.</param>
        /// <param name="requiresAuthentication">Does the request require authentication, default: true</param>
        /// <returns></returns>
        public async Task<JToken> QueryApiAsync(string url, bool requiresAuthentication = true) => 
            await QueryApiAsync(url, HttpMethod.GET, requiresAuthentication);

        /// <summary>
        ///     Query the API without passing data.
        /// </summary>
        /// <param name="url">The path to the requested resource.</param>
        /// <param name="method">The HttpMethod to use for the call.</param>
        /// <param name="requiresAuthentication">Does the request require authentication, default: true</param>
        /// <returns></returns>
        public async Task<JToken> QueryApiAsync(string url, HttpMethod method, bool requiresAuthentication = true) => 
            await QueryApiAsync(url, method, null, requiresAuthentication);

        /// <summary>
        ///     Query the API using the specified HttpMethod.
        /// </summary>
        /// <param name="url">The path to the requested resource.</param>
        /// <param name="method">The HttpMethod to use for the call.</param>
        /// <param name="data">The data to be set to the API.</param>
        /// <param name="requiresAuthentication">Does the request require authentication, default: true</param>
        /// <returns></returns>
        public async Task<JToken> QueryApiAsync(string url, HttpMethod method, object data, bool requiresAuthentication = true)
        {
            // If we don't have a URL, throw an error
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            // Factorize our data
            var model = Factorize(data);

            // Using a new WebClient
            using (var client = new HttpClient())
            {
                if (requiresAuthentication) { 
                    var accessToken = await GetAccessTokenAsync();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                }

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
        
        private async Task<string> GetAccessTokenAsync() =>
            await GetFromCache("AccessToken", async () => await GetAccessTokenAsync(_authUrl));

        private static async Task<string> GetFromCache(string key, Func<Task<string>> factory)
        {
            var cache = MemoryCache.Default;
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(55) };
            var newValue = new AsyncLazy<string>(factory);

            if (cache.AddOrGetExisting(key, newValue, policy) is Lazy<string> value) return value.Value;
            return await newValue.Value;
        }

        private async Task<string> GetAccessTokenAsync(string url)
        {
            // Create our pairs
            var pairs = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", _publicKey},
                {"client_secret", _secretKey}
            };

            // Encode our content
            var data = new FormUrlEncodedContent(pairs);

            // Using the HttpClient
            using (var client = new HttpClient())
            {
                // Set the security protocol
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                try
                {
                    // Get our response
                    var response = await client.PostAsync(url, data);

                    // Return null if we have failed
                    if (!response.IsSuccessStatusCode) return null;

                    // Read our results
                    var resultString = await response.Content.ReadAsStringAsync();

                    // Get our resolve
                    var jsonObject = JObject.Parse(resultString);

                    // Return our result
                    return jsonObject["access_token"].ToString();

                    // Return null if we get this far
                }
                catch
                {
                    // Throw an error
                    throw new Exception("Failed to get your access token");
                }
            }
        }

        private static async Task<JToken> HandleResponse(HttpResponseMessage response)
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

        private static void ProcessErrors(JObject errors)
        {
            // Exit if we have no errors
            if (errors == null) return;

            // Create our string builder
            var sb = new StringBuilder();

            // Loop through our errors
            foreach (var error in errors)
            {
                // Remove double quotes and square brackets from our value
                var value = Regex.Replace(error.Value.ToString(), @"[\[\]""]+", "");

                // Create our first item
                sb.Append(error.Key + ": " + value);
            }

            // Throw an error
            throw new Exception(sb.ToString());
        }
        
        private static object Factorize(object data)
        {
            // If the data that has been sent is the checkout model
            if (data is ICheckoutBindingModel model) return ModelFactory.Create(model);

            // Fallback, return the original object (can be null)
            return data;
        }
    }
}