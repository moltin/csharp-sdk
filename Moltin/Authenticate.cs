using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moltin
{
    public interface Authenticate
    {
        void authenticate(Dictionary<string, string> args);

        void refresh(Dictionary<string, string> args);

        string get(string key);
    }
}
