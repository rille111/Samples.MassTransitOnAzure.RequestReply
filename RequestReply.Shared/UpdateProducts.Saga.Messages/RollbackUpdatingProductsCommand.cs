using System;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public interface IRollbackUpdatingProductsCommand
    {
        Guid CorrelationId { get; set; }
    }
    public class RollbackUpdatingProductsCommand : IRollbackUpdatingProductsCommand
    {
        public Guid CorrelationId { get; set; }
    }
}