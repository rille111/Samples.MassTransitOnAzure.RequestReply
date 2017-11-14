using System;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public class UpdateProductsFinish
    {
        public Guid CorrelationId { get; set; }
        public string CorrelateUniqueName { get; set; }

    }
}