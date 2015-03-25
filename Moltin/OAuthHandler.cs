using Moltin.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Moltin
{
    public enum HttpMethod { GET, POST, PUT, DELETE }

    public class OAuthHandler
    {
        // Private variables
        private readonly string publicKey;
        private readonly string secretKey;

        private readonly string apiUrl;
        private readonly string authUrl;

        private readonly IOAuthProvider provider;

        /// <summary>
        /// Handles authencated calls made to the Moltin API.
        /// </summary>
        /// <param name="provider">A provider with generic functions that relate to OAuth.</param>
        /// <param name="authUrl">URL used to get the access token.</param>
        /// <param name="baseUrl">URL used for all other API requests.</param>
        /// <param name="version">API version which is appended to the base URL to make a full URL.</param>
        /// <param name="publicKey">Your public key.</param>
        /// <param name="secretKey">Your secret key.</param>
        public OAuthHandler(IOAuthProvider provider, string authUrl, string baseUrl, string version, string publicKey, string secretKey)
        {
            this.provider = provider;

            this.publicKey = publicKey;
            this.secretKey = secretKey;
            
            this.apiUrl = baseUrl + version + "/";
            this.authUrl = authUrl;
        }

        /// <summary>
        /// Query the API using GET HttpMethod.
        /// </summary>
        /// <param name="path">The path to the requested resource.</param>
        /// <returns></returns>
        public JToken QueryApi(string path)
        {
            return QueryApi(path, HttpMethod.GET, null);
        }

        /// <summary>
        /// Query the API using the specified HttpMethod.
        /// </summary>
        /// <param name="path">The path to the requested resource.</param>
        /// <param name="method">The HttpMethod to use for the call.</param>
        /// <param name="data">The data to be set to the API.</param>
        /// <returns></returns>
        public JToken QueryApi(string path, HttpMethod method, string data)
        {
            var model = GetAccessToken();
            var url = this.apiUrl + path;
            var result = "";

            using (var client = new WebClient())
            {
                client.Headers.Add("Authorization", "Bearer " + model.AccessToken);

                switch (method)
                {
                    case HttpMethod.PUT:
                    case HttpMethod.POST:
                        client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                        result = client.UploadString(url, "POST", data);
                        break;
                    default:
                        result = client.DownloadString(url + path);
                        break;
                }
            }

            return JObject.Parse(result);
        }

        /// <summary>
        /// TODO: Store access token in cache.
        /// </summary>
        /// <returns></returns>
        private Token GetAccessToken()
        {
            var result = Authenticate();
            var json = JObject.Parse(result);

            var token = json["access_token"].ToString();
            var expiresTimestamp = int.Parse(json["expires"].ToString());
            var expires = this.provider.UnixTimeStampToDateTime(expiresTimestamp);

            return new Token()
            {
                AccessToken = token,
                Expires = expires
            };
        }

        /// <summary>
        /// Function to authenticate for the first time.
        /// </summary>
        /// <returns>A JSON string with the access token.</returns>
        public string Authenticate()
        {
            var defaults = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", this.publicKey},
                {"client_secret", this.secretKey}
            };

            var normalisedParameters = provider.NormalizeParameters(defaults);

            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                return client.UploadString(authUrl, "POST", normalisedParameters);
            }
        }
    }
}
