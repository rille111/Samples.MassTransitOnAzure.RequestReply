﻿using System;
using System.Threading.Tasks;
using MassTransit;
using RequestReply.Shared.UpdateProducts.Saga.Messages;

namespace RequestReply.Receiver.Saga.Consumers
{
    public class UpdateProductsStartedEventConsumer : IConsumer<UpdateProductsStartedEvent>
    {
        public async Task Consume(ConsumeContext<UpdateProductsStartedEvent> context)
        {
            await Console.Out.WriteLineAsync($"{nameof(UpdateProductsStartedEvent)} Received, CorrelationId: {context.CorrelationId}");
        }
    }
}
