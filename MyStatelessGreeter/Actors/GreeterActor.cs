using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Shared.Messages;

namespace MyStatelessGreeter.Actors
{
    public class GreeterActor : ReceiveActor
    {
        private readonly StatelessServiceContext _context;

        public GreeterActor(StatelessServiceContext context)
        {

            _context = context;

            Receive<GreetMessage>(x => Greet(x.Who));
        }


        private void Greet(string name)
        {

            ServiceEventSource.Current.ServiceMessage(_context, "Hello from {0} to {1}!", _context.NodeContext.NodeName, name);

        }

    }
}
