using System;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public class StartUpdateProducts
    {
        public Guid CorrelationId { get; set; }
        public string CorrelateUniqueName { get; set; }
    }
}
