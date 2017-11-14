using System;
using System.Threading.Tasks;
using MassTransit;

namespace RequestReply.Receiver.MassTransit.Observers
{
    public class ReceiveObserver :
        IReceiveObserver
    {
        public Task PreReceive(ReceiveContext context)
        {
            
            // called immediately after the message was delivery by the transport
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
            return Task.CompletedTask;
        }

        public Task ReceiveFault(ReceiveContext context, Exception exception)
        {
            // called when an exception occurs early in the message processing, such as deserialization, etc.
            return Task.CompletedTask;
        }
    }
}
