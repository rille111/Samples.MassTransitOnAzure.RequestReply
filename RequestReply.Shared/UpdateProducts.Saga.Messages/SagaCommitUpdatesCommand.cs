using System;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public interface ISagaCommitUpdatesCommand
    {
        Guid CorrelationId { get; set; }
    }

    public class SagaCommitUpdatesCommand : ISagaCommitUpdatesCommand
    {
        public Guid CorrelationId { get; set; }

    }
}