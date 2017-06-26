using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Automatonymous;

namespace RequestReply.Shared.Messages.Product
{
    /// <summary>
    ///     This is the State Machine Saga.
    /// </summary>
    /// <remarks>
    ///     http://masstransit-project.com/MassTransit/advanced/sagas/automatonymous.html
    /// </remarks>
    public class UpdateProductsStateMachine : MassTransitStateMachine<UpdateProductsSaga>
    {
        public UpdateProductsStateMachine()
        {
            // Declare what property holds the state
            InstanceState(x => x.CurrentState);

            Event(() => Started,
                     x => x // x is the correlator of the Saga and the Command/Event, use this x correlator to configure stuff below
                     .CorrelateById(context => context.Message.CorrelationId)
                    //.CorrelateBy(c => c.UniqueName, context => context.Message.CorrelateUniqueName) // Correlate the saga instance, by defining the what property to correlate by
                    //.SelectId(ctx => Guid.NewGuid()) // Upon a new Saga instance, use this CorrelationId
                );

            Event(() => NewSequenceArrived, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => Finish, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => Rollback, x => x.CorrelateById(context => context.Message.CorrelationId));

            //Initially defined events that can create a state machine instance.
            Initially(
                When(Started) // First, listen on this command
                    .Then(context =>
                    {
                        // Set the saga instance properties
                        Console.Out.WriteLineAsync($"Initiating Saga, with CorrName: {context.Data.CorrelateUniqueName}");
                        context.Instance.CreatedUtc = DateTime.UtcNow;
                        context.Instance.UniqueName = context.Data.CorrelateUniqueName; // from the Command's UniqueName
                    })
                    .ThenAsync(context =>
                        Console.Out.WriteLineAsync($"START! {nameof(StartUpdateProducts)}.CorrName: {context.Data.CorrelateUniqueName}, To Saga.CorrelationId: {context.Instance.CorrelationId}"))
                    .TransitionTo(Active)
                    );

            // Set up what happens during events, when state is 'Active'
            During(Active,
                When(NewSequenceArrived)
                    .Then(context =>
                    {
                        Console.Out.WriteLineAsync($"{nameof(NewSequenceArrived)} arrived. CorrName: {context.Data.CorrelateUniqueName}");
                        foreach (var product in context.Data.Products)
                        {
                            Console.Out.WriteLineAsync($"Update Product: {product.ProductId}");
                        }
                    }),
                When(Rollback)
                    .Then(context =>
                    {
                        Console.Out.WriteLineAsync($"Rollback .. CorrName: {context.Data.CorrelateUniqueName}");
                    })
                    .Finalize(),

                When(Finish)
                    .Then(context =>
                    {
                        Console.Out.WriteLineAsync($"FINISH!, CorrName: {context.Data.CorrelateUniqueName}");
                    })
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }


        // The Events that may occur during the lifecycle of the Saga
        public Event<StartUpdateProducts> Started { get; set; }
        public Event<UpdateProductsSequence> NewSequenceArrived { get; set; }
        public Event<UpdateProductsFinish> Finish { get; set; }
        public Event<UpdateProductsRollback> Rollback { get; set; }

        // The possible Saga states
        public State Active { get; set; }
        public State Finished { get; set; }
    }
}