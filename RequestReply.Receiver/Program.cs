using System;
using System.Threading.Tasks;
using Automatonymous;
using MassTransit;
using MassTransit.Saga;
using Messaging.Infrastructure.ServiceBus.BusConfigurator;
using RequestReply.Receiver.Other;
using RequestReply.Shared.Messages.Product;
using RequestReply.Shared.Tools;

namespace RequestReply.Receiver
{
    static class Program
    {
        // These need not to be held "in reference" they will live on anyway, but since we investigate them they are held as global variables.
        static UpdateProductsStateMachine _machine;
        static InMemorySagaRepository<UpdateProductsSaga> _updProductsSagaRepo;

        static void Main()
        {
            Console.WriteLine("Starting bus, please wait ..");
            IBusControl azureBus = CreateIBusControl();
            azureBus.Start();
            Console.WriteLine("Bus started, waiting for messages ..");
            WriteKeyPressInteractions();
            LoopRepoContent();
            ReadUserKeyPress();
            azureBus.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static void ReadUserKeyPress()
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key.ToString().ToLower() == "r")
                {
                    Console.Out.WriteLineAsync($"Repo contains: {_updProductsSagaRepo.Count} Sagas!");
                    var correlationGuids = _updProductsSagaRepo.Where(p => true).ConfigureAwait(false).GetAwaiter().GetResult();
                    foreach (var corrGuid in correlationGuids)
                    {
                        var saga = _updProductsSagaRepo[corrGuid];
                        Console.Out.WriteLineAsync($"Saga[{corrGuid}], State: {saga.Instance.CurrentState}");
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private static void WriteKeyPressInteractions()
        {
            Console.WriteLine("-- Press --");
            Console.WriteLine("R             - Show Saga Repository contents");
            Console.WriteLine("Any other key - Exit");
        }

        private static Task LoopRepoContent()
        {
            var task = Task.Run(async () =>  // <- marked async
            {
                while (true)
                {
                    await Task.Delay(1000); // <- await with cancellation
                    
                }
            });
            return task;
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

                    //// Command Consumers 
                    //cfg.ReceiveEndpoint<IUpdateFooCommand>(c =>  // The interface = the queue name
                    //{
                    //    // LAB: Try turn all consumers off and see what happens when sending the commands ..

                    //    c.Consumer<UpdateFooCommandConsumer>(); // What class will consume the messages
                    //    c.Consumer<UpdateFooVersion2CommandConsumer>(); // What class will consume the messages
                    //    c.Consumer<IUpdateFooCommandConsumer>(); // What class will consume the messages
                    //});

                    //// Event Consumers
                    //cfg.ReceiveEndpoint<IBarEvent>(c =>  // The interface name = the queue name
                    //{
                    //    c.Consumer<BarEventConsumer>(); // What class will consume the messages
                    //});

                    //// Request Reply Consumers
                    //cfg.ReceiveEndpoint<ServeBarsCommand>(c =>  // The interface name = the queue name
                    //{
                    //    c.Consumer<ServeBarsCommandConsumer>(); // What class will consume the messages
                    //});

                    // Saga Consumers
                    _machine = new UpdateProductsStateMachine();
                    _updProductsSagaRepo = new InMemorySagaRepository<UpdateProductsSaga>();

                    cfg.ReceiveEndpoint("update_products_saga", c =>
                    {
                        c.StateMachineSaga(_machine, _updProductsSagaRepo);
                    });

                    // Manual Consumers
                    //cfg.ReceiveEndpoint("manual_queue", c =>  // The interface = the queue name
                    //{
                    //    c.Consumer<AnotherBarEventConsumer>(); // What class will consume the messages
                    //});
                });
            // Use this to debug. Example: You send stuff that should arrive to a saga but it doesn't, and you wanna know if the messages are even received, then place breakpoints there.
            bus.ConnectReceiveObserver(new ReceiveObserver());
            return bus;
        }
    }
}
