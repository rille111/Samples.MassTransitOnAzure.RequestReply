using System;
using System.Threading.Tasks;
using MassTransit;
using RequestReply.Shared;
using RequestReply.Shared.Messages;

namespace RequestReply.Receiver.Consumers
{
    public class AnotherBarEventConsumer : IConsumer<BarEvent>
    {
        public async Task Consume(ConsumeContext<BarEvent> context)
        {
            await Task.Delay(0);
            Console.WriteLine($"SNOOPER GOT {nameof(BarEvent)}!: TimeStampSent: {context.Message.TimeStampSent}, Id:{context.Message.Id}, Text:{context.Message.Text}");
        }
    }
}