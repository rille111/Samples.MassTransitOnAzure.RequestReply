using System;
using System.Threading.Tasks;
using MassTransit;
using RequestReply.Shared.FooBar.Messages;
using RequestReply.Shared.Tools;

namespace RequestReply.Receiver.FooBar.Consumers
{
    public class ServeBarsCommandConsumer : IConsumer<ServeBarsCommand>
    {
        public async Task Consume(ConsumeContext<ServeBarsCommand> context)
        {
            await Task.Delay(0);
            Console.WriteLine($"Got: {nameof(ServeBarsCommand)}! from {context.Message.BarOwner}: Sending response now.");
            GlobalVariables.RequestCounter++;

            // ReSharper disable once UseObjectOrCollectionInitializer
            var reply = new ServeBarsResponse();
            reply.ServedCounter = GlobalVariables.RequestCounter;
            reply.AckText = $"Thanks, {context.Message.BarOwner}. I was served, as number in line: {reply.ServedCounter}, Bars: {context.Message.Bars.Count}";

            await context.RespondAsync<ServeBarsResponse>(reply);
        }
    }
}
