h1. Instructions

* Make sure you have an AzureBus (Standard), copy the Shared access policy connection string to clipboard
* Edit config.json and paste that connection string (see config.example.json)
* Compile & run!

h2. Tools

* Download Azure Service Bus Explorer
* File - Connect - choose "Enter Connection String" - paste the same connection string from above

h2. Conclusions

h3. Sending (Commands)
**When** only using `_azureBus.GetSendEndpointAsync<IUpdateFooCommand>();`
**And** NOT calling `Bus.Start();`
**And** sending with `commandSendpoint.Send(new UpdateFooCommand(..));`
**Then** only a queue `iupdatefoocommand` gets created and nothing else

h4. Publishing (Events)
**When** publishing an event (ie to a topic) with `await _azureBus.Publish<IUpdateFooCommand>(new UpdateFooCommand());`
**And** NOT calling `Bus.Start();`
**Then** a topic-container with name `requestreply.shared` gets created 
**And** under that topic-container a Topic with the name `iupdatefoocommand` gets created (with no subscribers)

h4. Receiving (Commands)
**When** only creating bus and connecting a receive endpoint to an interface of `IUpdateFooCommand` to a `IConsumer<UpdateFooCommand>`
**And** then starting the bus
**Then** a topic-container `requestreply.shared` gets created 
**Then** a topic `updatefoocommand`
**Then** a queue `iupdatefoocommand` gets created 
**Then** a queue `adlsth052_requestreplyreceivervshost_bus_j3zyyyr15ryx6r4ebdkmjqqxgh` gets created (specific to this application instance probably) auto-delete after 5 mins of idling
Also
**Then** the topic `updatefoocommand` gets a subscription with forwarding to the queue: `IUpdateFooCommand`
