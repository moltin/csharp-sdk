using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace Moltin
{
    public class SDK
    {
        // Paths
        public static string version  = "beta";
        public static string url      = "https://api.molt.in/";
        public static string auth_url = "http://auth.molt.in/";

        // Variables
        public static string[] methods = new string[4] { "GET", "POST", "PUT", "DELETE" };
        public static Dictionary<string, string> store;
        public static Request request;
        public static Authenticate auth;

        // OAuth
        protected static string _token;
        protected static string _refresh;
        protected static string _expires;

        public SDK(string reqClass, string authClass, Dictionary<string, string> args = null)
        {
            // Create objects
            request = (Request)System.Activator.CreateInstance(Type.GetType("Moltin."+reqClass));
            auth    = (Authenticate)System.Activator.CreateInstance(Type.GetType("Moltin." + authClass));

            // Check for args
            if (args != null)
            {
                // Check for version
                if (args.ContainsKey("version")) { version = args["version"]; }

                // Get information from store
                if (args.ContainsKey("token")) { _token = store["token"]; }
                if (args.ContainsKey("refresh")) { _refresh = store["refresh"]; }
                if (args.ContainsKey("expires")) { _expires = store["expires"]; }
            }
        }

        public bool authenticate(Dictionary<string, string> args = null)
        {
            // Variables
            DateTime expires = ( _expires != null ? DateTime.Parse(_expires) : DateTime.Now );

            // Skip for active auth or refresh current
            if (expires.Year > 0 && expires > DateTime.Now) { return true; }
            else if (expires.Year > 0 && expires < DateTime.Now) { return refresh(args); }

            // Perform authentication
            auth.authenticate(args);

            // Store
            _storeToken();

            return (_token == null ? false : true);
        }

        public bool refresh(Dictionary<string, string> args = null)
        {
            // Perform refresh
            auth.refresh(args);

            // Store
            _storeToken();

            return (_token == null ? false : true);
        }

        public static string toQueryString(Dictionary<string, string> data)
        {
            // Variables
            string uri = "";

            // Loop dictionary
            foreach (KeyValuePair<string, string> entry in data)
            {
                uri += HttpUtility.UrlEncode(entry.Key) + "=" + HttpUtility.UrlEncode(entry.Value) + "&";
            }

            // Return new URI
            return uri.Remove(uri.Length - 1);
        }

        public Dictionary<string, string> get(string uri, Dictionary<string, string> post = null)
        {
            uri = url+version+"/"+uri;
            return _request(uri, "GET", post);
        }

        public Dictionary<string, string> post(string uri, Dictionary<string, string> post = null)
        {
            uri = url + version + "/" + uri;
            return _request(uri, "POST", post);
        }

        public Dictionary<string, string> put(string uri, Dictionary<string, string> post = null)
        {
            uri = url + version + "/" + uri;
            return _request(uri, "PUT", post);
        }

        public Dictionary<string, string> delete(string uri, Dictionary<string, string> post = null)
        {
            uri = url + version + "/" + uri;
            return _request(uri, "DELETE", post);
        }

        protected void _storeToken()
        {
            // Get data
            _token    = auth.get("token");
            _refresh = auth.get("refresh");
            _expires = auth.get("expires");

            // TODO: Add to longer-term storage
            store["token"]   = _token;
            store["refresh"] = _refresh;
            store["expires"] = _expires;
        }

        protected Dictionary<string, string> _request(string url, string method, Dictionary<string, string> data)
        {
            // Check type
            if (!methods.Contains(method))
            {
                throw new System.ArgumentException("Invalid request type (" + method + ")");
            }

            // Check token
            if (_token == null)
            {
                throw new System.ArgumentException("You haven't authenticated yet");
            }

            // Check token expiration
            if (DateTime.Parse(_expires).Year > 0 && DateTime.Now > DateTime.Parse(_expires))
            {
                throw new System.ArgumentException("Your current OAuth session has expired");
            }

            // Append URL
            if (method == "GET" && data.Count > 0)
            {
                url += "?" + toQueryString(data);
                data = null;
            }

            // Start request
            request.setup(url, method, data, _token);

            // Make request
            string result = request.make();

            // Check response
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Dictionary<string, string> json = jss.Deserialize<Dictionary<string, string>>(result);

            // Check JSON for error
            if (json.ContainsKey("status") && json["status"] == "true" )
            {
                string error = ( json.ContainsKey("errors") ? String.Join("\r\n", json["errors"]) : json["error"].ToString() );
                throw new System.ArgumentException(error);
            }

            // Return response
            return json;
        }

    }
}
