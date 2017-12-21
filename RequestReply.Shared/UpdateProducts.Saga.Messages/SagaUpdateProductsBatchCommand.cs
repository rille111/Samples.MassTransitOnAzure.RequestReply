using System;
using System.Collections.Generic;
using MassTransit;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public interface ISagaUpdateProductsBatchCommand
    {
        Guid CorrelationId { get; set; }
        List<ProductData> Products { get; set; }
    }

    public class SagaUpdateProductsBatchCommand : ISagaUpdateProductsBatchCommand
    {
        public Guid CorrelationId { get; set; }
        public List<ProductData> Products { get; set; }
        public SagaUpdateProductsBatchCommand()
        {
            this.Products = new List<ProductData>();
        }
    }

    public class ProductData
    {
        public Guid ProductId => NewId.NextGuid();
    }
}
