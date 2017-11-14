using System;
using System.Threading.Tasks;
using MassTransit;
using RequestReply.Shared.FooBar.Messages;

namespace RequestReply.Receiver.FooBar.Consumers
{
    // ReSharper disable once InconsistentNaming
    public class IUpdateFooCommandConsumer : IConsumer<IUpdateFooCommand>
    {
        public async Task Consume(ConsumeContext<IUpdateFooCommand> context)
        {
            await Task.Delay(0);
            Console.WriteLine($"Got interface {nameof(IUpdateFooCommand)}!: TimeStampSent: {context.Message.TimeStampSent}, Id:{context.Message.Id}, Text:{context.Message.Text}");
        }
    }
}
