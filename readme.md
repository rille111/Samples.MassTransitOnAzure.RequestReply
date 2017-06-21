TODO:
* Send an unknown command to a queue that has no consumers
	* Attach fault consumers to catch those
	* Think about logging .. Observer? Middleware?

h1. Instructions

* Make sure you have an AzureBus (Standard), copy the Shared access policy connection string to clipboard
* Edit config.json and paste that connection string (see config.example.json)
* Look at the code in frmSender.cs and Program.cs - edit and experiment.
* Compile & run!

h2. Tools

* Download Azure Service Bus Explorer
* File - Connect - choose "Enter Connection String" - paste the same connection string from above

h2. Conclusions

**Queues** will receive the messages sent by using SendEndpoint.Send()
**Temporary queues** are mostly only used for request/reply, faultreplies or routingslips (sagas) coordination - and can be ignored
**Topics** will receive the messages send via Bus.Publish()
**Consumers** tell what type they consume by code, and that will become the name of the Topic, and it becomes the Queue that they will listen on.
**Consumers** will consume both Commands and Events because of how MassTransit works (see below).
**Consumers** will only get message from queues. Topics forward messages to queues.
**Subcription** Names are equal to the queue name that they forward to, configured up by MassTransit.
**Queue Names** are the SendEndpoint/ReceiveEndpoint queue names (configured either as a string, or or the interface name if you're using our code - an extension method)
**Parallell Execution** is on by default. That is - several consumers will run and handle messages in parallell in the same application. You can control this by configuring PrefetchCount.
**Messages** that are sent to an endpoint that have consumers, but no consumer can understand the type, then MassTransit will move the messages to the _skipped queue.

The message is reserved and when it gets routed to some Consumer, and while the Consumer works - no other Consumer will get this message.
The message is acknowledged and removed from the Queue when the Consume() method runs to Completion (Task.RanToCompletion)

h2. Messages - Lifecycle

* Messages are removed from the Queue when:
	* The message is sent to a Consumer, and runs to Completion (Task.RanToCompletion).
* Messages are moved to the _skipped queue when
	* When a receive endpoint gets a message but doesnt have any connected consumers
	* When a receive endpoint gets a message type but doesnt have any connected consumers that can handle that type. (FooBarTwo gets sent, but we only have Consume<FooBar>)
	* When there are several receive endpoints sharing the same queue, DONT DO THIS!
	* When consumer doesn't know how to handle the message for whatever reason
* Messages are moved to the _errors queue when:
	* Only gets moved when the retry policy has been exhausted! Therefore: 
	* The Consume() Method throws an exception. 
	* The Consume() does not run to Task.RanToCompletion.
* Messages get moved to the 'deadletter' stash when
	* Messages have been in the Queue and it expires the configured queue TTL. Says so in the docs.
	* MassTransit could potentially move messages here for other reasons
	* Other Azure-related reasons: 

h2. Sending (Commands)

Starting the bus is not required or even recommended.

**When** only using `_azureBus.GetSendEndpointAsync<IUpdateFooCommand>();`
**And** sending with `commandSendpoint.Send(new UpdateFooCommand(..));`
**Then** only a queue `iupdatefoocommand` gets created (because of the send endpoint, this is our own code doing it otherwise you must define an Uri and that's a lotta work)

No topics will be created when sending commands.
Sent Commands ONLY gets sent to the Queues directly, never the queues.

h2. Publishing (Events)

Starting the bus is not required or even recommended.

**When** publishing an event (ie to a topic) with `await _azureBus.Publish<IBarEvent>(new BarEvent());`
**Then** a topic-container with name `requestreply.shared` gets created 
**And** under that topic-container a Topic with the name `ibarevent` gets created (with no subscribers)
**If** You instead  would send with `await _azureBus.Publish(new BarEvent());`  (using the concrete type, not the interface)
**Then** A topic with name `barevent` would be created

So - the resulting topic will be named as the type being sent, be that a class or interface - you decide.

h2. Consuming (Receiving BOTH Commands and Events)

A lot more happens when setting up consumers, to ensure messages get routed correctly using the MassTransit topology.
A consumer only ever listens to a Queue, never a Topic. But! The consumer will STILL get messages from both a Topic and a Queue.
Why? Because a Topic will be created, along with a Subscription that route messages to the actual Queue, and then the Consumer gets it from the Queue.
Therefore, a consumer will get both Events and Commands - this is a feature.

**When** creating bus and connecting a receive endpoint to an interface of `IUpdateFooCommand` to a `IConsumer<UpdateFooCommand>`, like so: `cfg.ReceiveEndpoint<IUpdateFooCommand>`
**And** then starting the bus
**Then** a topic-container `requestreply.shared` gets created 
**Then** a topic `updatefoocommand` gets created
**Then** a queue `iupdatefoocommand` gets created 
**Then** the topic `updatefoocommand` gets a subscription with forwarding to the queue: `IUpdateFooCommand`
**Then** a queue `somethingunique_receivervshost_bus_blablabla` gets created (unique to this application session) auto-delete after 5 mins of idling, because of MassTransit load balancing

Knowing this, therefore a Sender/Bus can either
* SendEndpoint.Send() a command directly to the queue `iupdatefoocommand` and not involving the topic
* Or Bus.Publish() an event to the topic `updatefoocommand` and it would get forwarded to the `iupdatefoocommand` queue
* The Consumer will still only listen on the queue and pick up messages from there, regardless of origin
* This might feel unnecessary, that a topic gets created and when you only ever .Send() this topic wouldn't get used. Yes this is true but thats how it is to ensure both options.

Therefore, MassTransit ensures that you can from a Senders perspective either Send Commands, or Publish Events, and those messages would hit eventually home once anyone has ever set up a consumer.

h2. Fails, exceptions and dead letters

Queues have a 'deadletter' stash. Messages that cannot be delivered to any receiver, or messages that could not be processed, gets moved here.
There may be various reasons why, but the messages can be peeked and you can call DeadLetterErrorDescription().
There is other metadata here such as the reason etc. See https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues
Best is probably to use FaultConsumers and log exceptions and reasons to some external log stash.

h2. Gotchas and FYIS

* Some times when starting up Consumers, messages that already are in the queue, don't get delivered?? Reason? Dunno! Can't reproduce right now.

* It should be fine to create you own queue manually, and add subscription to topics. Just know that when adding consumers, those consumers will either use a queue that you specify,
	but ALSO create topics that route to it. So be heedful of the naming!

* When sending commands, it is SUPER important that you know what type you send it as. Do you send it as an interface?
	* Example, using .Send( (IMyInterface) MyCommand )
	* Then you must have a Consumer that Consume<IMyInterface>, Consume<MyCommand> will not suffice and lead to the message being moved to Skipped queue.

h3. Do's and Dont's

* Do use Bus.Publish() if you ever want several Consumers catch the Messages, since those get posted to Topics
* Do use middleware if you ALSO want other subscribers to catch the Messages, and post those to some Topic of choice, like "SomeCommandWasSentEvent"
* Do make sure that each receive endpoint works against its own queue (inside an application) otherwise there will be trouble
* Do use the same queue for a given receive endpoint for different instances of the application - to achieve 'Competing Consumer' pattern
* Avoid creating your own topics/queues for the routing - let MassTransit do it by starting up the Consumers with bus.start
* Do feel free to create your own subscriptions and forward messages sent to topics - but know that when creating MT consumers - queues and topics will be created which may cause confusion
* Do attach fault consumers with external logging to make debugging easier
* Do send Concrete Types as Commands and Events
* Do hookup Receive and Send endpoints against interfaces (or strings) - not on concrete types

h3. Scenarios

	h4. Say that you have two different applications that want to receive the same message. Then you have some options.

		h5. You have control over the producer
		* Ensure that the message being sent is PUBLISHED as an Event, therefore arrive to a topic. Message should NOT being sent as a Command via a Send Endpoint.
		* Ensure that there is a queue, and a subscription that route these Events to that Queue. 
			(MassTransit will create these for you when you register a Consumer, so it's better to let MT do its work to avoid problems.)

		h5. You do NOT have control over sender/publisher and the message is sent to a queue
		This is trickier. Are you allowed to re-route messages? If thats the case, you can forward from the original queue into a topic, and then create subscriptions to that topic.
		Maybe Azure can create copies to other queues? Dunno.

	h4. Say that you want to have several consumers, one for the top interface, and one for a concrete implementation. You can!
	* Example, using .Send( (MyCommand) MyCommand )
	* And having two consumers like:
		* Consume<IMyInterface>
		* Consume<MyCommand>
	Will result in both Consumers being fired. Not sure how stable it is tho.

h3. References

http://docs.masstransit-project.com/en/latest/configuration/gotchas.html
http://docs.masstransit-project.com/en/latest/overview/underthehood.html?highlight=_skipped
http://masstransit-project.com/MassTransit/usage/request-response.html
https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues
https://github.com/MassTransit/Sample-RequestResponse