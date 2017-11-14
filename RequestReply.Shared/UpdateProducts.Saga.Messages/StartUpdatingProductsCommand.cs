using System;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public interface IStartUpdatingProductsCommand
    {
        Guid CorrelationId { get; set; }
        string UniqueText { get; set; }
    }

    public class StartUpdatingProductsCommand : IStartUpdatingProductsCommand
    {
        public Guid CorrelationId { get; set; }
        public string UniqueText { get; set; }
    }
}
