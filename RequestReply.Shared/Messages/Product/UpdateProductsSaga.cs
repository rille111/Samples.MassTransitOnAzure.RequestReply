using System;
using Automatonymous;

namespace RequestReply.Shared.Messages.Product
{
    public class UpdateProductsSaga : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}