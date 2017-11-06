using System;

namespace RequestReply.Shared.Messages.Product
{
    public class UpdateProductsFinish
    {
        public Guid CorrelationId { get; set; }
        public string CorrelateUniqueName { get; set; }

    }
}