using System;
using System.Threading.Tasks;
using MassTransit;

namespace RequestReply.Shared.MassTransit.Observers
{
    public class ConsoleOutReceiveObserver :
        IReceiveObserver
    {
        public Task PreReceive(ReceiveContext context)
        {
            // called immediately after the message was delivery by the transport
            Console.Out.WriteLineAsync($"MASSTRANSIT PreReceive: On Address: {context.InputAddress}");
            return Task.CompletedTask;
        }

        public Task PostReceive(ReceiveContext context)
        {
            // called after the message has been received and processed
            return Task.CompletedTask;
        }

        public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType)
            where T : class
        {
            // called when the message was consumed, once for each consumer
            return Task.CompletedTask;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan elapsed, string consumerType, Exception exception) where T : class
        {
            // called when the message is consumed but the consumer throws an exception
            Console.Error.WriteLineAsync($"MASSTRANSIT ConsumeFault! Exception: " + exception.Message);
            return Task.CompletedTask;
        }

        public Task ReceiveFault(ReceiveContext context, Exception exception)
        {
            // called when an exception occurs early in the message processing, such as deserialization, etc.
            Console.Error.WriteLineAsync($"MASSTRANSIT ReceiveFault! Exception: " + exception.Message);
            return Task.CompletedTask;
        }
    }
}
