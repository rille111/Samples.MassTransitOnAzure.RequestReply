h1. Instructions

* Make sure you have an AzureBus (Standard), copy the Shared access policy connection string to clipboard
* Edit config.json and paste that connection string (see config.example.json)
* Compile & run!

h2. Tools

* Download Azure Service Bus Explorer
* File - Connect - choose "Enter Connection String" - paste the same connection string from above

h2. Conclusions

**When** only using `_azureBus.GetSendEndpointAsync<IUpdateFooCommand>();`
**And** sending with `commandSendpoint.Send(new UpdateFooCommand(..));`
**Then** a queue with name `iupdatefoocommand` gets created and nothing else

