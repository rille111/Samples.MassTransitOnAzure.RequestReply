TODO:
* Send an unknown command to a queue that has no consumers
	* Attach fault consumers to catch those
	* Think about logging .. Observer? Middleware?
* Dropdown with Started sagas that you can choose from and batch updates to

# Instructions

* Find out the connection string 
	* Make sure you have an AzureBus (Standard), copy the Shared access policy connection string to clipboard
	* Edit config.json and paste that connection string (see config.example.json)
	* Look at the code in frmSender.cs and Program.cs - edit and experiment.
* Delete all queues and topics to work with a clean slate
* Download Azure Service Bus Explorer
* File - Connect - choose "Enter Connection String" - paste the same connection string from above
* Compile & run!

# Conclusions

**Queues** will receive the messages sent by using SendEndpoint.Send()
**Temporary queues** are mostly only used for request/reply, routingslips or masstransit internals like faultreplies - and can otherwise be ignored.
**Topics** will receive the messages send via Bus.Publish()
**Consumers** tell what messagen type they consume by code, and that will result in a Topic, and it becomes the Queue that they will listen on.
**Consumers** will consume both Commands and Events because of how MassTransit works (see below), as a catch-both feature 
**Consumers** will only get message from queues. Topics forward messages to queues (im almost positive about this).
**Subcription** Names are equal to the queue name that they forward to, configured up by MassTransit.
**Queue Names** are the SendEndpoint/ReceiveEndpoint queue names (configured either as a string, or or the interface name if you're using our code - an extension method)
**Parallell Execution** is on by default. That is - several consumers will run and handle messages in parallell in the same application. You can control this by configuring PrefetchCount. I think.
**Messages** that are sent to an endpoint that have consumers, but if no consumer can understand the type, then MassTransit will move the messages to the _skipped queue.
**The message is reserved** and when it gets routed to some Consumer, and while the Consumer works - no other Consumer will get this message. (Lock)
**The message is acknowledged** and removed from the Queue when the Consume() method runs to Completion (Task.RanToCompletion), force this with return Task.CompletedTask

# Messages - Lifecycle

## Messages are sent from a unique queue belonging to the sending application
	* Included in the message as "Source Address" pointing to the bus and the queue
	* Prefixed with computer name + application name + some generated id
## Messages are removed from the destination queue when:
	* The message is sent to a Consumer, and runs to Completion (Task.RanToCompletion). 
	* Until then - the message will be reserved/locked so that no other may consume it.
## Messages are moved to the _skipped queue when
	* When a receive endpoint gets a message but doesnt have any connected consumers
	* When a receive endpoint gets a message type but doesnt have any connected consumers that can handle that concrete type. (FooBarTwo gets sent, but we only have Consume<FooBar>)
	* When there are several receive endpoints sharing the same queue in the same application, DONT DO THIS! You probably want load balancing here!
	* When consumer doesn't know how to handle the message because you changed the class in one project but the receiver didnt get the same change (contract was broken)
	* When the namespace of the message type is changed in either sender/receiver and the other didnt get the same change
## Messages are moved to the _errors queue when:
	* When the retry policy has been exhausted! Therefore: 
	* The Consume() Method throws an exception. 
	* The Consume() does not run to Task.RanToCompletion.
	* When a Saga gets an Event that is not expected for the current State
	* When MassTransit is unable to deserialize the message for any reason
## Messages get moved to the 'deadletter' stash when
	* Messages have been in the Queue and it expires the configured queue TTL. Says so in the docs.
	* Messages have been locked, but the Consumer never tan to Completion and therefore some expiration have occurred when the retries are exhausted
	* MassTransit could potentially move messages here for other reasons
	* Other Azure-related reasons?

#. Sending (Commands and Queries)

Starting the bus is not required

**When** using `_azureBus.GetSendEndpointAsync<IUpdateFooCommand>();`
**And** sending with `commandSendpoint.Send(new UpdateFooCommand(..));`
**Then** a queue `iupdatefoocommand` gets created (because of the send endpoint, this is our own code doing it otherwise you must define an Uri and that's a lotta work)

No topics will be created when sending commands.
Sent Commands ONLY gets sent to the Queues directly, never the queues.

# Publishing (Events)

Starting the bus is not required

**When** publishing an event (ie to a topic) with `_azureBus.Publish<IBarEvent>(new BarEvent());`
**Then** a topic-container with the name of the namespace that the message resides in gets created if not exists, e.g `myapplication.myfolder`
**And** under that topic-container a Topic with the name `ibarevent` gets created because we uses <IBarEvent>

**When** publishing an event (ie to a topic) with `_azureBus.Publish(new BarEvent());` without <IBarEvent> specified
**Then** a topic-container with the name of the namespace that the message resides in gets created if not exists, e.g `myapplication.myfolder`
**And** under that topic-container a Topic with the name `barevent` gets created

So - Important! the resulting topic name will be named as the type being sent, be that a class or interface - you decide.
Do you want to work with the general interface, and give option to the receivers that they may receive different classes that implement this interface? *More flexible, allow versioning - higher problem potential*
Or do you want to be more specific, and only work with one specific class? *Less flexible - But also less problem potential*

#. Consuming (Receiving Commands & Events)

Starting the bus IS required

A lot more happens when setting up consumers, to ensure messages get routed correctly using the MassTransit topology.
A consumer only ever listens to a Queue, never a Topic. **But! The consumer will STILL get messages from both a Topic and a Queue !!!**
Why? Because a Topic will be created, along with a Subscription that route messages to the actual Queue, and then the Consumer gets it from the Queue.
Therefore, a consumer will get both Events and Commands - this is a catch-both feature.

**When** creating bus and connecting a receive endpoint to an interface of `IUpdateFooCommand` to a `IConsumer<UpdateFooCommand>`, like so: `cfg.ReceiveEndpoint<IUpdateFooCommand>`
**And** then starting the bus
**Then** a topic-container `requestreply.shared` gets created 
**Then** a topic `iupdatefoocommand` gets created
**Then** a queue `iupdatefoocommand` gets created 
**Then** the topic `iupdatefoocommand` gets a subscription with forwarding to the queue: `IUpdateFooCommand`
**Then** a queue `somethingunique_receivervshost_bus_blablabla` gets created (unique to this application session) auto-delete after 5 mins of idling

Knowing this, therefore a Sender/Bus can either
* SendEndpoint.Send() a command directly to the queue `iupdatefoocommand` and not involving the topic
* Or Bus.Publish() an event to the topic `iupdatefoocommand` and it would get forwarded to the `iupdatefoocommand` queue
* The Consumer will still only listen on the queue and pick up messages from there, regardless of origin
* This might feel unnecessary, that a topic gets created and when you only ever .Send() this topic wouldn't get used. Yes this is true but thats how it is.

Therefore, MassTransit ensures that you can from a Senders perspective either Send Commands, or Publish Events, and those messages would hit eventually home once anyone has ever set up a consumer.

# Request-Reply (Send, Consume, and GetResponse)

Starting the bus IS required (otherwise you will never get replies)

**When** creating bus and initiating a `MessageRequestClient` with `serverbarscommand` as the Request type and `serverbarsresponse` as the Reply type
**Then** only a queue `serverbarscommand` gets created 
**And** replies will be sent to the temporary unique queue for that application/session

In this scenario, the Sender must have its bus started in order to get a temporary unique queue, where the responses are sent to after sending a command or query.
The queue where Requests are sent to, is either a string or the name of the actual request type, eg. `serverbarscommand` if you use our code.

# Sagas

## Saga Best Practices

### Use only one queue for the entire saga

* **When** working in a saga, the saga should have its own queue (endpoint), and only ONE queue - name configured by a string because this queue is not bound to one specific message, several message types will end up here.
* **Then** you can send commands directly to this queue.
* **Or** the saga can be configured to listen to any event on the bus, because that will result in created subscriptions to those topics that route messages to this saga queue.

### If possible, create a correlation id for the first message initiating a saga

* It will be easier coordinating messages this way, but you CAN correlate by other properties
* Prefer using NewId() when generating Guids (partitioning friendly, etc)

### Log stuff, especially faults

* Connect a FaultConsumer<SomeMessage> right next to the actual consumer, this is an excellent place to catch exceptions and dump to some logging
* You can also connect Observers, where you can log such for example where the events/commands actually get published
	* These you can connect after creating the bus, and there are many different observers

## Saga Gotchas

* When configuring Events in the Saga state machine with * `Event<SagaStartUpdatesCommand>` 
	* Then that results in a created topic `SagaStartUpdatesCommand` with a subscription routed to the saga queue
	* Then the saga state machine will now be able to consume and handle that message type `SagaStartUpdatesCommand`
	* A bit weird since Commands will never be sent to topics, but it is a built in catch-both-commands-and-events mechanism.

# Fails, exceptions and dead letters

Queues have a 'deadletter' stash. Messages that cannot be delivered to any receiver, or messages that could not be processed, gets moved here.
There may be various reasons why, but the messages can be peeked and you can call DeadLetterErrorDescription().
There is other metadata here such as the reason etc. See https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues
Best is probably to use FaultConsumers and log exceptions and reasons to some external log stash.

# Gotchas and FYIS

* Some times when starting up Consumers, messages that already are in the queue, don't get delivered - thats probably because they are locked
	* One reason might be the application crashed/stopped in the middle of processing a command
	* There may be other reasons

* Messages when delivered to a Consumer, will be locked until :
	* Method runs to completion, the message will be acknowledged and removed from the queue
	* Lock expires (timeout) and the message will then be moved to ..?  TODO: TEST THIS!!
	* Exception or fail, and message will be moved to dead letter or error or skipped 

* It should be fine to create you own queue manually, and add subscription to topics. Just know that when adding consumers, those consumers will either use a queue that you specify,
	but ALSO create topics that route to it. So be heedful of the naming!

* When sending commands, it is SUPER important that you know what type you send it as. Do you send it as an interface?
	* Example, using .Send( (IMyInterface) MyCommand )
	* Then you must have a Consumer that Consume<IMyInterface>, Consume<MyCommand> will not suffice and lead to the message being moved to Skipped queue.

# Do's and Dont's

* Do always start the bus if you intend on receiving messages in any way (request-reply or consuming, or listening to faults)
* Do use Bus.Publish() if you ever want several Consumers catch the Messages, since those get posted to Topics
* Do use middleware if you ALSO want other subscribers to catch the Messages, and post those to some Topic of choice, like "SomeCommandWasSentEvent"
* Do make sure that each receive endpoint works against its own queue (inside an application) otherwise there will be trouble
* Do use the same queue for a given receive endpoint for different instances of the application - to achieve 'Competing Consumer' pattern
* Avoid creating your own topics/queues for the routing - let MassTransit do it by starting up the Consumers with bus.start
* Do feel free to create your own subscriptions and forward messages sent to topics - but know that when creating MT consumers - queues and topics will be created which may cause confusion
* Do attach fault consumers with external logging to make debugging easier
* Do send Concrete Types as Commands and Events
* Do hookup Receive and Send endpoints against interfaces (or strings) - not on concrete types

# Scenarios

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

# References

http://docs.masstransit-project.com/en/latest/configuration/gotchas.html
http://docs.masstransit-project.com/en/latest/overview/underthehood.html?highlight=_skipped
http://masstransit-project.com/MassTransit/usage/request-response.html
https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues
https://github.com/MassTransit/Sample-RequestResponse