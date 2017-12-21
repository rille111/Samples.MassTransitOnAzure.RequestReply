using System;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public interface ISagaRollbackUpdatesCommand
    {
        Guid CorrelationId { get; set; }
    }
    public class SagaRollbackUpdatesCommand : ISagaRollbackUpdatesCommand
    {
        public Guid CorrelationId { get; set; }
    }
}