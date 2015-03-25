using System;
using System.Collections.Generic;

namespace Moltin
{
    public interface IOAuthProvider
    {
        string NormalizeParameters(Dictionary<string, string> parameters);
        DateTime UnixTimeStampToDateTime(double unixTimeStamp);
    }
}
