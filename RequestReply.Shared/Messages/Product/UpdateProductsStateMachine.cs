using System;
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

            Event(() => DoUpdateProducts,
                     x => x // x is the correlator of the Saga and the Command/Event, use this x correlator to configure stuff below
                    .CorrelateBy(c => c.UniqueName, context => context.Message.CommandUniqueName) // Correlate the saga instance, by defining the what property to correlate by
                    .SelectId(ctx => Guid.NewGuid()) // Upon a new Saga instance, use this CorrelationId
                );

            //Initially defined events that can create a state machine instance.
            Initially(
                When(DoUpdateProducts) // First, listen on this command
                    .Then(context =>
                    {
                    // Set the saga instance properties
                        Console.Out.WriteLineAsync($"Initiating Saga");
                        context.Instance.CreatedUtc = DateTime.UtcNow;
                        context.Instance.UniqueName = context.Data.CommandUniqueName; // from the Command's UniqueName
                    })
                    .ThenAsync(context =>
                        Console.Out.WriteLineAsync($"DoUpdateProducts: {context.Data.CommandUniqueName} to {context.Instance.CorrelationId}"))
                    .TransitionTo(OnGoing)
                    .Finalize()
                    );

            SetCompletedWhenFinalized();
        }


        // The Events that may occur during the lifecycle of the Saga
        public Event<UpdateProductsCommand> DoUpdateProducts { get; set; }

        // The possible Saga states
        public State OnGoing { get; set; }
        public State Finished { get; set; }
    }
}