using System;
using Automatonymous;
using RequestReply.Shared.UpdateProducts.Saga.Messages;

// https://app.pluralsight.com/player?course=masstransit-rabbitmq-scaling-microservices&author=roland-guijt&name=masstransit-rabbitmq-scaling-microservices-m5&clip=6&mode=live
// http://masstransit-project.com/MassTransit/advanced/sagas/automatonymous.html
// http://automatonymous.readthedocs.io/en/latest/configuration/quickstart.html
namespace RequestReply.Shared.UpdateProducts.Saga
{
    public class UpdateProductsStateMachine : MassTransitStateMachine<UpdateProductsSaga>
    {
        // The Events that may occur during the lifecycle of the Saga (Commands and Events from the queues/topics. (Can Events not coming from the queues be used as well?)
        public Event<SagaStartUpdatesCommand> Started { get; set; }
        public Event<SagaUpdateProductsBatchCommand> UpdateBatch { get; set; }
        public Event<SagaCommitUpdatesCommand> Finished { get; set; }
        public Event<SagaRollbackUpdatesCommand> Rollback { get; set; }

        // The possible Saga states here:
        public State Updating { get; set; }
        public State Completed { get; set; }

        // And the behaviour
        public UpdateProductsStateMachine()
        {
            // Declare what property holds the state (populated when saga is retrieved)
            InstanceState(x => x.CurrentState);

            // First, we need to set up Correlation for the Event that starts the Saga!
            Event(() => Started, // When the message that starts the Saga comes in, we dont have any correlation yet. We need to configure this using either something unique, or using an already existing CorrelationId that is perhaps set from the GUI. But we cannot always control this if we don't own the message.
                     x => x  // x is the correlator used to configure stuff below.
                     //.CorrelateBy(saga => saga.SomethingUnique, ctx => ctx.Message.UniqueText) // No correlationId yet, so we need either to have a custom association/correlation, by using 'CorrelateBy'. 
                     //.SelectId(ctx => NewId.NextGuid()) // And from now on, we should use a true CorrelationId. (Unsure about this)
                     .CorrelateById(ctx => ctx.Message.CorrelationId) // OR! You can prefer to use this if you already have a CorrelationId! MT will fill out CorrelationId on the message automatically if you just define it on the message classes (I think).
                     .SelectId(ctx => ctx.Message.CorrelationId)
                    
                );
            // And the Correlation for the other Events
            Event(() => Finished, x => x.CorrelateById(context => context.Message.CorrelationId)); // We can now use .CorrelationId because this is a message class that we 'own' and therefore implemented.
            Event(() => Rollback, x => x.CorrelateById(context => context.Message.CorrelationId)); // Same here. I think it gets autopopulated from the above .NewId() code, and brought along in the bus..
            Event(() => UpdateBatch, x => x.CorrelateById(context => context.Message.CorrelationId));

            // We tell the Saga what actually can initiate a new workflow with 'Initially'
            Initially(
                When(Started) // First, listen on this command
                    .Then(context =>
                    {
                        Console.Out.WriteLineAsync($"Saga: Initiating Saga, with {nameof(SagaStartUpdatesCommand.UniqueText)}: {context.Data.UniqueText}, ");
                        // Put the details in the state object (Set the saga instance properties)
                        context.Instance.CreatedUtc = DateTime.UtcNow;
                        context.Instance.SomethingUnique = context.Data.UniqueText;
                    })
                    .ThenAsync(context =>
                        Console.Out.WriteLineAsync($"Saga: Started! {nameof(SagaStartUpdatesCommand)}: Instance.CorrId: {context.Data.CorrelationId}, To Saga.CorrelationId: {context.Instance.CorrelationId}"))

                        // Publish some Event, a consumer somewhere will get ready for batched updates..
                        .Publish(ctx => new UpdateProductsStartedEvent(ctx.Instance))

                        // And then change State to 'Active'
                        .TransitionTo(Updating)
                    );

            // Set up what Events can happen during when state is 'Active'
            During(Updating,
                When(UpdateBatch)
                    .Then(context =>
                    {
                        Console.Out.WriteLineAsync($"Saga: UPDATE BATCH! Saga.CorrId: {context.Instance.CorrelationId}, Products.Count: {context.Data.Products?.Count}");
                    }),
                When(Rollback)
                    .Then(context =>
                    {
                        Console.Out.WriteLineAsync($"Saga: ROLLBACK! Saga.CorrId: {context.Instance.CorrelationId}, Message.CorrId: {context.Data.CorrelationId}");
                    })
                    .Finalize(),

                When(Finished)
                    .Then(context =>
                    {
                        Console.Out.WriteLineAsync($"Saga: FINISH!, Saga.CorrId: {context.Instance.CorrelationId}, Message.CorrId: {context.Data.CorrelationId}");
                    })
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }
    }


}