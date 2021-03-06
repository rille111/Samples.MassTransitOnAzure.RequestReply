# Sagaworkshop

## Vocabulary & Concepts

* The `Saga` is a long lived workflow / transaction. 
	* Is is in fact a serializable state class that is persisted to some storage.
* The `Saga State Machine` is the class (machine) containing the behaviour and coordination of the saga instances.
* A `Receive Endpoint` is something that is configured by code, and is the connection to the actual queue.
* A `Send Endoint` is what the application will use to send commands, it is the connection to the actual queue.
* A `Message Type` is the actual class being sent as a message, and it can be either a Command (sent to queue) or an Event (sent to a topic that gets routed to a queue)

## Topology in Azure Service Bus

* A saga flow should always have its own dedicated queue, and not be split up to other queues.
* That saga will handle all messages that arrive to that queue
* Either commands sent directly to the queue,
* Or events that get sent to topics (that in turn will be forwarded to that queue as subscriptions, that the State machine will set up)
	
## Configure

* Decide upon a queue name, all saga-related stuff (commands AND events coming from topics, will be routed here)
* Configure a Receive Endpoint to that saga queue, hooking up the Saga State machine to that Endpoint (Only one endpoint!!)
* Look at the code for the State Machine:
	* Configure the possible Events to respond to (Events in this case are either Commands sent directly to the queue, or Events sent to topics since those get routed to that queue)
	* Configure the states that the Saga instances can actually be inside of
	* Decide how to correlate messages (by what property) and set this up
	* Set up the behaviour, starting with Initially (something needs to kick off the saga!)

## Demo

* Start only the Receiver and observe the bus
	* See the topics and its routings to that queue
	* Verify that you can actually delete the topics since we're using commands
* Delete all entities in the bus
* Start the sender and do a saga with both apps running
* Start only the sender and do multiple sagas, then start the receiver
* Try sending an unknown message to the saga queue (use Service Bus Explorer) and see what happens
	
## Troubleshooting common problems

* Hook up Observers (good for logging for example)
