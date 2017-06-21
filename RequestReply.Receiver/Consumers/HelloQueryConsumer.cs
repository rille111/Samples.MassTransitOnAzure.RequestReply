using System;
using System.Threading.Tasks;
using MassTransit;
using RequestReply.Shared.Messages;

namespace RequestReply.Receiver.Consumers
{
    public class HelloQueryConsumer : IConsumer<HelloQuery>
    {
        public async Task Consume(ConsumeContext<HelloQuery> context)
        {
            await Task.Delay(0);
            Console.WriteLine($"Got Query! {nameof(HelloQuery)}!: Sending response now.");
            GlobalVariables.RequestCounter++;

            // ReSharper disable once UseObjectOrCollectionInitializer
            var reply = new HelloResponse();
            reply.Counter = GlobalVariables.RequestCounter;
            reply.HelloText = $"Well hello there, {context.Message.MyName}. You were served, as number in line: {reply.Counter}";

            await context.RespondAsync<HelloResponse>(reply);
        }
    }
}
