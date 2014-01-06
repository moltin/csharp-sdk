using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace Moltin
{
    class ClientCredentials : Authenticate
    {
        protected Dictionary<string, string> _data = new Dictionary<string, string>(){
            {"token",   null},
            {"refresh", null},
            {"expires", null}
        };

        public void authenticate(Dictionary<string, string> args)
        {
            // Variables
            string url = SDK.url + "oauth/access_token";
            Dictionary<string, string> data = new Dictionary<string, string>() {
                {"grant_type",    "client_credentials"},
                {"client_id",     args["client_id"]},
                {"client_secret", args["client_secret"]}
            };

            // Make request
            SDK.request.setup(url, "POST", data);
            string result = SDK.request.make().Replace("\\\"", "'");

            // Check response
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Dictionary<string,string> json = jss.Deserialize<Dictionary<string, string>>(result);

            // Check JSON for errors
            if (json.ContainsKey("error"))
            {
                throw new System.ArgumentException(json["error"].ToString());
            }

            // Set data
            data["token"]   = json["token"].ToString();
            data["refresh"] = json["refresh"].ToString();
            data["expires"] = json["expires"].ToString();
        }

        public void refresh(Dictionary<string, string> args)
        {
            authenticate(args);
        }

        public string get(string key)
        {
            if (!_data.ContainsKey(key)) { return null; }
            return _data[key];
        }

    }
}
