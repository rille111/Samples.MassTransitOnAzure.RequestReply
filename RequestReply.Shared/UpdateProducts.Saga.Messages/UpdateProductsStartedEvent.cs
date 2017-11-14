using System;

namespace RequestReply.Shared.UpdateProducts.Saga.Messages
{
    public interface IUpdateProductsStartedEvent
    {
        Guid CorrelationId { get; set; }
    }


    public class UpdateProductsStartedEvent : IUpdateProductsStartedEvent
    {
        public UpdateProductsStartedEvent()
        {
            
        }
        public UpdateProductsStartedEvent(UpdateProductsSaga instance)
        {
            this.CorrelationId = instance.CorrelationId;
        }

        public Guid CorrelationId { get; set; }
    }
}
