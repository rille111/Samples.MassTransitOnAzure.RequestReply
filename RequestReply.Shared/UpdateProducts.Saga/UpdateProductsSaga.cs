using System;
using Automatonymous;

namespace RequestReply.Shared.UpdateProducts.Saga
{
    /// <summary>
    /// The UpdateProductsSaga State. This is what gets stored in the repo. It contains all the different States
    /// </summary>
    public class UpdateProductsSaga : SagaStateMachineInstance
    {
        /// <summary>
        /// Don't change or touch.
        /// </summary>
        public Guid CorrelationId { get; set; }

        public string UniqueName { get; set; }

        /// <summary>
        /// Auto* uses this to keep track of the current state, don't touch!!
        /// </summary>
        public string CurrentState { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime UpdatedUtc { get; set; }

    }
}