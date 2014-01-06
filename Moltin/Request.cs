using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moltin
{
    public interface Request
    {    
        void setup(string url, string method, Dictionary<string, string> post = null, string token = null);

        string make();

        string _randomString(int size);
    }
}
