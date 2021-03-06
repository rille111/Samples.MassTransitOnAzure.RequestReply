﻿using System;
using System.Threading.Tasks;
using Automatonymous;
using MassTransit;
using MassTransit.Saga;
using Messaging.Infrastructure.ServiceBus.BusConfigurator;
using RequestReply.Receiver.FooBar.Consumers;
using RequestReply.Receiver.Saga.Consumers;
using RequestReply.Shared.FooBar.Messages;
using RequestReply.Shared.MassTransit.Observers;
using RequestReply.Shared.Shared.Tools;
using RequestReply.Shared.UpdateProducts.Saga;
using RequestReply.Shared.UpdateProducts.Saga.Messages;

namespace RequestReply.Receiver
{
    static class Program
    {
        // These need not to be held "in reference" they will live on anyway, but since we investigate them they are held as global variables.
        static UpdateProductsStateMachine _machine;
        static InMemorySagaRepository<UpdateProductsSaga> _updProductsSagaRepo;
        private static bool _enableCommandConsumers;
        private static bool _enableEventConsumers;
        private static bool _enableRequestReplyConsumers;
        private static bool _enableSagaConsumers;

        static void Main()
        {
            Console.WriteLine("Starting bus, please wait ..");

            // Configure here
            _enableCommandConsumers = false;
            _enableEventConsumers = false;
            _enableRequestReplyConsumers = false;
            _enableSagaConsumers = true;

            // Start the bus
            IBusControl azureBus = CreateIBusControl();
            azureBus.Start();

            // Wait and exit on key press
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
                        Console.Out.WriteLineAsync($"Saga[{corrGuid}], State: {saga.Instance.CurrentState}, SomethingUnique: {saga.Instance.SomethingUnique}");
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

            var bus = new AzureSbBusConfigurator(connstring)
                .CreateBus((cfg, host) =>
               {
                   //TODO: How to handle pool of receivers? aka "Competing Consumers"
                   // https://masstransit.readthedocs.io/en/latest/configuration/gotchas.html#how-to-setup-a-competing-consumer
                   // http://docs.masstransit-project.com/en/latest/overview/underthehood.html

                   // Command Consumers 
                   if (_enableCommandConsumers)
                   {
                       cfg.ReceiveEndpoint<IUpdateFooCommand>(c =>  // The interface = the queue name
                       {
                           // LAB: Try turn all consumers off and see what happens when sending the commands ..

                           c.Consumer<UpdateFooCommandConsumer>(); // What class will consume the messages
                           c.Consumer<UpdateFooVersion2CommandConsumer>(); // What class will consume the messages
                           c.Consumer<IUpdateFooCommandConsumer>(); // What class will consume the messages
                       });
                   }


                   // Event Consumers
                   if (_enableEventConsumers)
                   {
                       cfg.ReceiveEndpoint<IBarEvent>(c =>  // The interface name = the queue name
                        {
                            c.Consumer<BarEventConsumer>(); // What class will consume the messages
                        });
                       cfg.ReceiveEndpoint<IUpdateProductsStartedEvent>(c =>
                        {
                            c.Consumer<UpdateProductsStartedEventConsumer>();
                        });
                       cfg.ReceiveEndpoint<ISagaUpdateProductsBatchCommand>(c =>
                        {
                            c.Consumer<UpdateProductsBatchCommandConsumer>();
                        });
                   }

                   // Request Reply Consumers
                   if (_enableRequestReplyConsumers)
                   {
                       cfg.ReceiveEndpoint<ServeBarsCommand>(c =>  // The interface name = the queue name
                       {
                           c.Consumer<ServeBarsCommandConsumer>(); // What class will consume the messages
                       });
                   }

                   // Saga Consumers
                   _machine = new UpdateProductsStateMachine();
                   _updProductsSagaRepo = new InMemorySagaRepository<UpdateProductsSaga>();

                   if (_enableSagaConsumers)
                   {
                       // It looks like all messages related to the saga must be sent to the same queue? But what if we can't control this? (Look it up)
                       cfg.ReceiveEndpoint("update_products_saga", c =>
                       {
                           c.StateMachineSaga(_machine, _updProductsSagaRepo);
                       });
                   }
                   
                   // Manual Consumers
                   //cfg.ReceiveEndpoint("manual_queue", c =>  // The interface = the queue name
                   //{
                   //    c.Consumer<AnotherBarEventConsumer>(); // What class will consume the messages
                   //});
               });
            
            // Use this to debug. Example: You send stuff that should arrive to a saga but it doesn't, and you wanna know if the messages are even received, then place breakpoints there.
            bus.ConnectReceiveObserver(new ConsoleOutReceiveObserver());

            return bus;
        }
    }
}
