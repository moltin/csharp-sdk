using System;
using System.Collections.Generic;
using System.Net;

namespace Moltin
{
    public class OAuthHandler
    {
        private readonly string apiKey;
        private readonly string apiSecret;

        private readonly IOAuthProvider provider;

        public OAuthHandler(IOAuthProvider provider, string apiKey, string apiSecret)
        {
            this.provider = provider;

            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
        }

        public string GetAccessToken(string requestTokenUrl)
        {
            var defaults = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                {"client_id", this.apiKey},
                {"client_secret", this.apiSecret}
            };

            var normalisedParameters = provider.NormalizeParameters(defaults);

            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                return client.UploadString(requestTokenUrl, "POST", normalisedParameters);
            }
        }

        public string Get(string url)
        {
            using (var client = new WebClient())
                return client.DownloadString(url);
        }

        public string Post(string url, string parameters)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                return client.UploadString(url, "POST", parameters);
            }
        }
    }
}
