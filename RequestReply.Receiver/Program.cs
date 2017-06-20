using System;
using MassTransit;
using Messaging.Infrastructure.ServiceBus.BusConfigurator;
using RequestReply.Shared;
using RequestReply.Shared.Tools;

namespace RequestReply.Receiver
{
    static class Program
    {
        

        static void Main()
        {
            Console.WriteLine("Starting bus, please wait ..");
            IBusControl azureBus = CreateIBusControl();
            azureBus.Start();
            Console.WriteLine("Bus started, waiting for messages ..");
            Console.WriteLine("-- Press any key to exit --");
            Console.ReadKey();
        }

        private static IBusControl CreateIBusControl()
        {
            var connstring = new JsonConfigFileReader().GetValue("AzureSbConnectionString");

            var bus = AzureSbBusConfigurator
                .CreateBus(connstring, (cfg, host) => 
                {
                    //TODO: How to handle pool of receivers? aka "Competing Consumers"
                    // https://masstransit.readthedocs.io/en/latest/configuration/gotchas.html#how-to-setup-a-competing-consumer
                    // http://docs.masstransit-project.com/en/latest/overview/underthehood.html

                    // Command Consumers 
                    cfg.ReceiveEndpoint<IUpdateFooCommand>(c =>  // The interface = the queue name
                    {
                        c.Consumer<UpdateFooCommandConsumer>(); // What class will consume the messages
                    });

                    // Event Consumers
                    cfg.ReceiveEndpoint("BarQueue", c =>  // The interface name = the queue name
                    {
                        c.Consumer<BarEventConsumer>(); // What class will consume the messages
                    });

                    cfg.ReceiveEndpoint("snooper", c =>  // The interface = the queue name
                    {
                        c.Consumer<AnotherBarEventConsumer>(); // What class will consume the messages
                    });


                });
            return bus;
        }
    }
}
