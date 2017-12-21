using System;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public interface ISagaStartUpdatesCommand
    {
        Guid CorrelationId { get; set; }
        string UniqueText { get; set; }
    }

    public class SagaStartUpdatesCommand : ISagaStartUpdatesCommand
    {
        public Guid CorrelationId { get; set; }
        public string UniqueText { get; set; }
    }
}
