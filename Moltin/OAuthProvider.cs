using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moltin
{
    public class OAuthProvider : IOAuthProvider
    {
        /// <summary>
        /// Normalises the parameters passed.
        /// </summary>
        /// <param name="parameters">A dictionary object of parameters to be normalised.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts a TimeStamp to the correct DateTime.
        /// </summary>
        /// <param name="unixTimeStamp">The TimeStamp that needs converting.</param>
        /// <returns></returns>
        public DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();

            return dtDateTime;
        }
    }
}
