using System;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public interface IUpdateProductsStartedEvent
    {
        Guid CorrelationId { get; set; }
        string UniqueName { get; set; }
    }


    public class UpdateProductsStartedEvent : IUpdateProductsStartedEvent
    {
        public UpdateProductsStartedEvent()
        {
            
        }
        public UpdateProductsStartedEvent(UpdateProductsSaga instance)
        {
            this.UniqueName = instance.UniqueName;
            this.CorrelationId = instance.CorrelationId;
        }

        public Guid CorrelationId { get; set; }

        public string UniqueName { get; set; }
    }
}
