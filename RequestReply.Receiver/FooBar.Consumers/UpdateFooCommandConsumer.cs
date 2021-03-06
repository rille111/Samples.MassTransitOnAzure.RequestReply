﻿using System;
using System.Threading.Tasks;
using MassTransit;
using RequestReply.Shared.FooBar.Messages;

namespace RequestReply.Receiver.FooBar.Consumers
{
    public class UpdateFooCommandConsumer : IConsumer<UpdateFooCommand>
    {
        public async Task Consume(ConsumeContext<UpdateFooCommand> context)
        {
            await Task.Delay(0);
            Console.WriteLine($"Got {nameof(UpdateFooCommand)}!: TimeStampSent: {context.Message.TimeStampSent}, Id:{context.Message.Id}, Text:{context.Message.Text}");
        }
    }
}