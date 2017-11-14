using System;
using System.Collections.Generic;
using MassTransit;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public class UpdateProductsSequence
    {
        public Guid CorrelationId { get; set; }
        public string CorrelateUniqueName { get; set; }

        public List<ProductData> Products { get; set; }

        public UpdateProductsSequence()
        {
            this.Products = new List<ProductData>();
        }
    }

    public class ProductData
    {
        public Guid ProductId => NewId.NextGuid();
    }
}
