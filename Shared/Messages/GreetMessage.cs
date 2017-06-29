using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messages
{
    public class GreetMessage
    {
        public string Who { get; private set; }

        public GreetMessage(string who)
        {
            Who = who;
        }
    }
}
