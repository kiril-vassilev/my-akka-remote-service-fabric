using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using MyStatelessGreeter.Actors;

namespace MyStatelessGreeter
{
    public class MyCommunicationListener : ICommunicationListener
    {
        private const string AkkaSection = @"
                akka {
                    actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""

                    remote {
                        helios.tcp {
                            port = 0
                            hostname = localhost
                        }
                    }
                }";


        private readonly StatelessServiceContext _context;

        private ActorSystem _actorSystem;
        

        protected internal MyCommunicationListener(StatelessServiceContext context)
        {
            _context = context;
        }


        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var endpoint = _context.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");


            var config = ConfigurationFactory
                .ParseString("akka.remote.helios.tcp.port=" + endpoint.Port)
                .WithFallback("akka.remote.helios.tcp.hostname=" + _context.NodeContext.IPAddressOrFQDN)
                .WithFallback(AkkaSection);


            _actorSystem = ActorSystem.Create("MyStatelessGreeter", config);
            var greeter = _actorSystem.ActorOf(Props.Create(() => new GreeterActor(_context)), "greeter");


            var url = $"akka.tcp://{_actorSystem.Name}@{_context.NodeContext.IPAddressOrFQDN}:{endpoint.Port}";

            return Task.FromResult(url);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _actorSystem?.Terminate().Wait(cancellationToken);
            _actorSystem?.Dispose();

            return Task.FromResult(0);
        }

        public void Abort()
        {
            _actorSystem?.Dispose();
        }
    }
}
