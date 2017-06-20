using System;
using System.Threading.Tasks;
using MassTransit;
using RequestReply.Shared;

namespace RequestReply.Receiver
{
    public class BarEventConsumer : IConsumer<BarEvent>
    {
        public async Task Consume(ConsumeContext<BarEvent> context)
        {
            await Task.Delay(0);
            Console.WriteLine($"Got {nameof(BarEvent)}!: TimeStampSent: {context.Message.TimeStampSent}, Id:{context.Message.Id}, Text:{context.Message.Text}");
        }
    }
}