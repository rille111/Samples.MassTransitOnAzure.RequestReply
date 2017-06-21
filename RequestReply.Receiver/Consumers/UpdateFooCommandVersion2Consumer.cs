using System;
using System.Threading.Tasks;
using MassTransit;
using RequestReply.Shared;

namespace RequestReply.Receiver.Consumers
{
    public class UpdateFooVersion2CommandConsumer : IConsumer<UpdateFooVersion2Command>
    {
        public async Task Consume(ConsumeContext<UpdateFooVersion2Command> context)
        {
            await Task.Delay(0);
            Console.WriteLine($"Got {nameof(UpdateFooVersion2Command)}!: TimeStampSent: {context.Message.TimeStampSent}, Id:{context.Message.Id}, Text:{context.Message.Text}");
        }
    }
}