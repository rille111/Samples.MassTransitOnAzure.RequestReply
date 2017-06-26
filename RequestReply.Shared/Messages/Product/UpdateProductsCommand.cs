using System;

namespace RequestReply.Shared.Messages.Product
{
    public interface IUpdateProductsCommand
    {
        string CommandUniqueName { get; set; }
        Guid CorrelationId { get; set; }
    }

    public class UpdateProductsCommand : IUpdateProductsCommand
    {
        public Guid CorrelationId { get; set; }
        public string CommandUniqueName { get; set; }

    }
}
