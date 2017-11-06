using System;
using System.Collections.Generic;

namespace RequestReply.Shared.Messages.Product
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
}
