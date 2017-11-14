using System;
using Automatonymous;
using RequestReply.Shared.UpdateProducts.Saga.Messages;

// https://app.pluralsight.com/player?course=masstransit-rabbitmq-scaling-microservices&author=roland-guijt&name=masstransit-rabbitmq-scaling-microservices-m5&clip=6&mode=live
namespace RequestReply.Shared.UpdateProducts.Saga
{
    /// <summary>
    ///     This is the State Machine Saga.
    /// </summary>
    /// <remarks>
    ///     http://masstransit-project.com/MassTransit/advanced/sagas/automatonymous.html
    /// </remarks>
    public class UpdateProductsStateMachine : MassTransitStateMachine<UpdateProductsSaga>
    {
        // The possible Saga states here:
        public State Active { get; set; }
        public State Finished { get; set; }

        // The Events that may occur during the lifecycle of the Saga (Commands and Events from the queues/topics. (Can Events not coming from the queues be used as well?)
        public Event<StartUpdateProducts> Started { get; set; }
        public Event<UpdateProductsSequence> NewSequenceArrived { get; set; }
        public Event<UpdateProductsFinish> Finish { get; set; }
        public Event<UpdateProductsRollback> Rollback { get; set; }

        // And the behaviour
        public UpdateProductsStateMachine()
        {
            // Declare what property holds the state (populated when saga is retrieved)
            InstanceState(x => x.CurrentState);

            // First, we need to set up the Event that starts the Saga!
            Event(() => Started, // When the message that starts the Saga comes in, we dont have any correlation yet. We need to configure this using either something unique, or using an already existing CorrelationId that is perhaps set from the GUI. But we cannot always control this if we don't own the message.
                     x => x  // x is the correlator used to configure stuff below.
                     .CorrelateBy(saga => saga.UniqueName, ctx => ctx.Message.CorrelateUniqueName) // No correlationId yet, so we need to have a temporary association, by using this. (Unsure about this)
                     //.CorrelateById(context => context.Message.CorrelationId) // OR! You can prefer to use this if you already have a CorrelationId! MT will fill out CorrelationId on the message automatically if you just define it on the message classes (I think).
                    .SelectId(ctx => Guid.NewGuid()) // And from now on, we should use a true CorrelationId. (Unsure about this)
                );

            Event(() => NewSequenceArrived, x => x.CorrelateById(context => context.Message.CorrelationId)); // We can now use .CorrelationId because this is a message class that we 'own' and therefore implemented.
            Event(() => Finish, x => x.CorrelateById(context => context.Message.CorrelationId)); // Same here. I think it gets autopopulated from the above .NewId() code, and brought along in the bus..
            Event(() => Rollback, x => x.CorrelateById(context => context.Message.CorrelationId)); // Same here

            // We tell the Saga what actually can initiate a new workflow with 'Initially'
            Initially(
                When(Started) // First, listen on this command
                    .Then(context =>
                    {
                        Console.Out.WriteLineAsync($"Initiating Saga, with CorrName: {context.Data.CorrelateUniqueName}");
                        // Put the details in the state object (Set the saga instance properties)
                        context.Instance.CreatedUtc = DateTime.UtcNow;
                        context.Instance.UniqueName = context.Data.CorrelateUniqueName; // from the Command's UniqueName
                    })
                    .ThenAsync(context =>
                        Console.Out.WriteLineAsync($"START! {nameof(StartUpdateProducts)}.CorrName: {context.Data.CorrelateUniqueName}, To Saga.CorrelationId: {context.Instance.CorrelationId}"))
                    .TransitionTo(Active) // And then change State to 'Active'
                    .Publish(ctx => new UpdateProductsStartedEvent(ctx.Instance)) // And publish some Event
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
    }


}