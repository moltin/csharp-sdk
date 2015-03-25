using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;

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
        public OAuthHandler(IOAuthProvider provider, string publicKey, string secretKey)
        {
            this.provider = provider;

            this.publicKey = publicKey;
            this.secretKey = secretKey;
        }

        /// <summary>
        /// Get the access token.
        /// </summary>
        /// <param name="url">The authorization url.</param>
        /// <returns></returns>
        public string GetAccessToken(string url)
        {
            var defaults = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", this.publicKey},
                {"client_secret", this.secretKey}
            };

            var normalisedParameters = provider.NormalizeParameters(defaults);
            var result = "";

            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                result = client.UploadString(url, "POST", normalisedParameters);
            }

            var json = JObject.Parse(result);

            if (json["error"] != null)
                throw new Exception(json["error"].ToString());

            return json["access_token"].ToString();
        }

        /// <summary>
        /// Query the API using GET HttpMethod.
        /// </summary>
        /// <param name="url">The path to the requested resource.</param>
        /// <returns></returns>
        public JToken QueryApi(string accessToken, string url)
        {
            return QueryApi(accessToken, url, HttpMethod.GET, null);
        }

        /// <summary>
        /// Query the API using the specified HttpMethod.
        /// </summary>
        /// <param name="url">The path to the requested resource.</param>
        /// <param name="method">The HttpMethod to use for the call.</param>
        /// <param name="data">The data to be set to the API.</param>
        /// <returns></returns>
        public JToken QueryApi(string accessToken, string path, HttpMethod method, string data)
        {
            var url = this.apiUrl + path;
            var result = "";

            using (var client = new WebClient())
            {
                if (!string.IsNullOrEmpty(accessToken))
                    client.Headers.Add("Authorization", "Bearer " + accessToken);

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
    }
}
