using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json.Linq;
using Shared.Messages;

namespace MyStatelessGreetee
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class MyStatelessGreetee : StatelessService
    {

        private readonly ActorSystem _actorSystem;

        public MyStatelessGreetee(ActorSystem system, StatelessServiceContext context)
            : base(context)
        {
            _actorSystem = system;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

            ResolvedServicePartition partition =
                await resolver.ResolveAsync(new Uri(@"fabric:/my_akka_remote_service_fabric/MyStatelessGreeter"), new ServicePartitionKey(), cancellationToken);

            var endpoint = partition.GetEndpoint();

            JObject addresses = JObject.Parse(endpoint.Address);
            string primaryReplicaAddress = (string)addresses["Endpoints"].First();

            var greeter = _actorSystem.ActorSelection(primaryReplicaAddress + "/user/greeter");

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                iterations++;
                var name = $"MyStatelessGreetee: {iterations}, {this.Context.NodeContext.NodeName}";

                greeter.Tell(new GreetMessage(name));

                ServiceEventSource.Current.ServiceMessage(this.Context, "MyStatelessGreeter is working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
