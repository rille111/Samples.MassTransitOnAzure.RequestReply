using System;
using MassTransit;

namespace RequestReply.Shared.Messages.Product
{
    public class ProductData
    {
        public Guid ProductId => NewId.NextGuid();
    }
}
