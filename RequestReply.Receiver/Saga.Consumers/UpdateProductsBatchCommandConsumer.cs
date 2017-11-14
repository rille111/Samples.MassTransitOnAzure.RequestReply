using System;
using System.Threading.Tasks;
using MassTransit;
using RequestReply.Shared.UpdateProducts.Saga.Messages;

namespace RequestReply.Receiver.Saga.Consumers
{
    public class UpdateProductsBatchCommandConsumer : IConsumer<UpdateProductsBatchCommand>
    {
        public async Task Consume(ConsumeContext<UpdateProductsBatchCommand> context)
        {
            await Console.Out.WriteLineAsync($"{nameof(UpdateProductsBatchCommand)} Received, Update for [{context.Message.Products.Count}] Products. Corr.Id: {context.Message.CorrelationId}");
        }
    }
}
