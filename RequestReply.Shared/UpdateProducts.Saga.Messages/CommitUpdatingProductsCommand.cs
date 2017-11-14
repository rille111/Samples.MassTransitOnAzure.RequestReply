using System;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public interface ICommitUpdatingProductsCommand
    {
        Guid CorrelationId { get; set; }
    }

    public class CommitUpdatingProductsCommand : ICommitUpdatingProductsCommand
    {
        public Guid CorrelationId { get; set; }

    }
}