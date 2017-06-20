h1. Instructions

* Make sure you have an AzureBus (Standard), copy the Shared access policy connection string to clipboard
* Edit config.json and paste that connection string (see config.example.json)
* Compile & run!

h2. Tools

* Download Azure Service Bus Explorer
* File - Connect - choose "Enter Connection String" - paste the same connection string from above

h2. Conclusions

**Queues** will receive the messages sent by using SendEndpoint.Send() directly 
**Topics** will receive the messages send via Bus.Publish()
**Consumers** tell what class/interface they consume by code, and that will become the name of the Topic AND Queue that they will listen on.
**Consumers** will consume both Commands and Events because of how MassTransit works (see below)
**Subcription** Names are equal to the queue name, hooked up by MassTransit.
**Queue Names** are the SendEndpoint/ReceiveEndpoint queue names (configured either as a string, or or the interface name if you're using our code - an extension method)
**Parallell Execution** Is on by default. That is - several consumers will run and handle messages in parallell. You can control this by configuring PrefetchCount.

The message is reserved and when it gets routed to some Consumer, and while the Consumer works - no other Consumer will get this message.
The message is acknowledged and removed from the Queue when the Consume() method runs to Completion (Task.RanToCompletion)

* Messages are removed from the Queue when:
	* The message is sent to a Consumer, and runs to Completion (Task.RanToCompletion) TODO: Test by debugging and peeking while debugging
* Messages are moved to the _skipped queue when
	* When a receive endpoint gets a message but doesnt have any connected consumers
	* When a receive endpoint gets a message type but doesnt have any connected consumers that can handle that type. (FooBarTwo gets sent, but we only have Consume<FooBar>) TODO: Test!
	* When there are several receive endpoints sharing the same queue, DONT DO THIS!
	* When consumer doesn't know how to handle the message for whatever reason
* Messages are moved to the _errors queue when:
	* Only gets moved when the retry policy has been exhausted! Therefore: 
	* The Consume() Method throws an exception										TODO: Test throw excpetion
	* The Consume() does not run to Task.RanToCompletion after retries				TODO: Test return Task.Incomplete
* Messages get moved to the 'deadletter' stash when
	* Messages have been in the Queue and it expires the TTL. Says so in the docs  TODO: Test by setting a low TTL
	* MassTransit could potentially move messages here for other reasons

h3. Sending (Commands)

Starting the bus is not required or even recommended.

**When** only using `_azureBus.GetSendEndpointAsync<IUpdateFooCommand>();`
**And** sending with `commandSendpoint.Send(new UpdateFooCommand(..));`
**Then** only a queue `iupdatefoocommand` gets created (because of the send endpoint, this is our own code doing it otherwise you must define an Uri and that's a lotta work)

No topics will be created when sending commands.
Sent Commands ONLY gets sent to the Queues directly, never the queues.

h3. Publishing (Events)

Starting the bus is not required or even recommended.

**When** publishing an event (ie to a topic) with `await _azureBus.Publish<IBarEvent>(new BarEvent());`
**Then** a topic-container with name `requestreply.shared` gets created 
**And** under that topic-container a Topic with the name `ibarevent` gets created (with no subscribers)
**If** You instead  would send with `await _azureBus.Publish(new BarEvent());`  (using the concrete type, not the interface)
**Then** A topic with name `barevent` would be created

So - the resulting topic will be named as the type being sent, be that a class or interface - you decide.

h3. Consuming (Receiving BOTH Commands and Events)

A lot more happens when setting up consumers, to ensure messages get routed correctly, using the MassTransit topology.
A consumer only ever listens to a Queue, never a Topic. But! The consumer will STILL get messages from both a Topic and a Queue.
Why? Because a Topic will be created, along with a Subscription that route messages to the actual Queue, and then the Consumer gets it from the Queue.

**When** creating bus and connecting a receive endpoint to an interface of `IUpdateFooCommand` to a `IConsumer<UpdateFooCommand>`, like so: `cfg.ReceiveEndpoint<IUpdateFooCommand>`
**And** then starting the bus
**Then** a topic-container `requestreply.shared` gets created 
**Then** a topic `updatefoocommand` gets created
**Then** a queue `iupdatefoocommand` gets created 
**Then** the topic `updatefoocommand` gets a subscription with forwarding to the queue: `IUpdateFooCommand`
**Then** a queue `somethingunique_receivervshost_bus_blablabla` gets created (unique to this application session) auto-delete after 5 mins of idling, because of MassTransit load balancing

Knowing this, therefore a Sender/Bus can either
* SendEndpoint.Send() a command directly to the queue `iupdatefoocommand` skipping the topic
* Or Bus.Publish() an event to the topic `updatefoocommand` and it would get forwarded to the `iupdatefoocommand` queue, 
* The Consumer will still only listen on the queue and pick up messages from there
* This might feel unnecessary, that a topic gets created and when you only ever .Send() this topic wouldn't get used. Yes this is true but thats how it is to ensure both options.

Therefore, MassTransit ensures that you can from a Senders perspective either Send Commands, or Publish Events, and those messages would hit eventually home once anyone has ever set up a consumer.

h3. Fails, exceptions and dead letters

Queues have a 'deadletter' stash. Messages that cannot be delivered to any receiver, or messages that could not be processed, gets moved here.
There may be various reasons why, but the messages can be peeked and you can call DeadLetterErrorDescription().
There is other metadata here such as the reason etc. 

h3. Gotchas and FYIS

* Some times when starting up Consumers, messages that already are in the queue, don't get delivered?? Reason? Dunno! Can't reproduce right now.
* It should be fine to create you own queue manually, and add subscription to topics. Just know that when adding consumers, those consumers will either use a queue that you specify,
but ALSO create topics that route to it. So be heedful of the naming!

http://docs.masstransit-project.com/en/latest/configuration/gotchas.html
https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues
http://docs.masstransit-project.com/en/latest/overview/underthehood.html?highlight=_skipped