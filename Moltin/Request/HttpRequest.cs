using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Moltin
{
    class HttpRequest : Request
    {
        protected static HttpWebRequest request;
        protected static string session;

        public void Main()
        {
            session = _randomString(20);
        }

        public void setup(string url, string method, Dictionary<string, string> post = null, string token = null)
        {
            // Create client
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Accept = "application/json";

            // Add post data
            if ( post != null )
            {
                // Variables
                string query = SDK.toQueryString(post);
                byte[] bytes = Encoding.UTF8.GetBytes(query);

                // Add headers
                request.ContentType   = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;

                // Add to query
                Stream stream = request.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

            // Add OAuth header
            if ( token != null )
            {
                request.Headers["Authorization"] = "Bearer " + token;
            }

            // Add session header
            request.Headers["X-Moltin-Session"] = session;
        }

        public string make()
        {
            // Variables
            WebResponse response;

            try
            {
                // Make request
                response = request.GetResponse();
            }
            catch (WebException ex)
            {
                // Get response anyway
                response = ex.Response as WebResponse;
            }

            // Read response
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string result = reader.ReadToEnd().Trim().Replace("\\\"", "'");

            // Clean up
            reader.Close();
            response.Close();

            // DEBUG
            Console.WriteLine(result);

            // Send back cleaned string
            return result;
        }

        public string _randomString(int size)
        {
            // Variables
            Random random = new Random((int)DateTime.Now.Ticks); 
            StringBuilder builder = new StringBuilder();
            char ch;

            // Loop and build string
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            // Send back formatted string
            return builder.ToString();
        }

    }
}
