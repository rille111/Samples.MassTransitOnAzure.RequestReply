using System;
using System.Threading.Tasks;
using MassTransit;
using RequestReply.Shared.FooBar.Messages;

namespace RequestReply.Receiver.FooBar.Consumers
{
    public class BarEventConsumer : IConsumer<BarEvent>
    {
        public async Task Consume(ConsumeContext<BarEvent> context)
        {
            await Task.Delay(0);
            Console.WriteLine($"Got Event! {nameof(BarEvent)}!: TimeStampSent: {context.Message.TimeStampSent}, Id:{context.Message.Id}, Text:{context.Message.Text}");
        }
    }
}