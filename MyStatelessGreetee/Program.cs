using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.ServiceFabric.Services.Runtime;

namespace MyStatelessGreetee
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                var config = ConfigurationFactory.ParseString(@"
                    akka {
                        actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""

                        remote {
                            helios.tcp {
                                port = 9090
                                hostname = localhost
                            }
                        }
                    }
                ");

                var system = ActorSystem.Create("MyStatelessGreetee", config);

                ServiceRuntime.RegisterServiceAsync("MyStatelessGreeteeType",
                    context => new MyStatelessGreetee(system, context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(MyStatelessGreetee).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
