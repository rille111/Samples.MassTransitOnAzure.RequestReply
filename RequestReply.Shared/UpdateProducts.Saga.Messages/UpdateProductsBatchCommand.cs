using System;
using System.Collections.Generic;
using MassTransit;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public interface IUpdateProductsBatchCommand
    {
        Guid CorrelationId { get; set; }
        List<ProductData> Products { get; set; }
    }

    public class UpdateProductsBatchCommand : IUpdateProductsBatchCommand
    {
        public Guid CorrelationId { get; set; }
        public List<ProductData> Products { get; set; }
        public UpdateProductsBatchCommand()
        {
            this.Products = new List<ProductData>();
        }
    }

    public class ProductData
    {
        public Guid ProductId => NewId.NextGuid();
    }
}
