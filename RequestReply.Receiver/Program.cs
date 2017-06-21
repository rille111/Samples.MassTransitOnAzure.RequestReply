using System;
using MassTransit;
using Messaging.Infrastructure.ServiceBus.BusConfigurator;
using RequestReply.Receiver.Consumers;
using RequestReply.Shared;
using RequestReply.Shared.Messages;
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
                        c.Consumer<UpdateFooVersion2CommandConsumer>(); // What class will consume the messages
                        c.Consumer<IUpdateFooCommandConsumer>(); // What class will consume the messages
                    });

                    // Event Consumers
                    cfg.ReceiveEndpoint<IBarEvent>(c =>  // The interface name = the queue name
                    {
                        c.Consumer<BarEventConsumer>(); // What class will consume the messages
                    });

                    // Request Reply Consumers
                    cfg.ReceiveEndpoint<HelloQuery>(c =>  // The interface name = the queue name
                    {
                        c.Consumer<HelloQueryConsumer>(); // What class will consume the messages
                    });

                    // Manual Consumers
                    //cfg.ReceiveEndpoint("manual_queue", c =>  // The interface = the queue name
                    //{
                    //    c.Consumer<AnotherBarEventConsumer>(); // What class will consume the messages
                    //});


                });
            return bus;
        }
    }
}
