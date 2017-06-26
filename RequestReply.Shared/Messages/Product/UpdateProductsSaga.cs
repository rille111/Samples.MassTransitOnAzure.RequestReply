using System;
using Automatonymous;

namespace RequestReply.Shared.Messages.Product
{
    public class UpdateProductsSaga : SagaStateMachineInstance
    {
        /// <summary>
        /// Don't change or touch.
        /// </summary>
        public Guid CorrelationId { get; set; }

        public string UniqueName { get; set; }

        /// <summary>
        /// Auto* uses this to keep track of the current state, dont change it by yourself!
        /// </summary>
        public string CurrentState { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime UpdatedUtc { get; set; }

    }
}