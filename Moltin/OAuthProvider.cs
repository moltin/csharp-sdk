using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moltin
{
    public class OAuthProvider : IOAuthProvider
    {
        public string NormalizeParameters(Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();

            var i = 0;
            foreach (var parameter in parameters.OrderBy(x => x.Key).ThenBy(x => x.Value).ToList())
            {
                if (i > 0)
                    sb.Append("&");

                sb.AppendFormat("{0}={1}", parameter.Key, parameter.Value);

                i++;
            }

            return sb.ToString();
        }
    }
}
