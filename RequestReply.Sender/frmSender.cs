using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using MassTransit;
using Messaging.Infrastructure.ServiceBus.BusConfigurator;
using RequestReply.Shared.FooBar.Messages;
using RequestReply.Shared.MassTransit.Observers;
using RequestReply.Shared.Shared.Tools;
using RequestReply.Shared.UpdateProducts.Saga.Messages;

namespace RequestReply.Sender
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable UseObjectOrCollectionInitializer
    public partial class frmSender : Form
    {
        private IBusControl _azureBus;
        private Guid _lastSagaCorrelationId;
        private readonly string _sagaQueueName = "update_products_saga";
        private ISendEndpoint _sagaSendPoint;
        private LoggingPublishObserver _publishObserver;
        private LoggingSendObserver _sendObserver;

        public frmSender()
        {
            InitializeComponent();

            PopulateUiElements();

            ConfigureBus();

            ConnectObservers();
        }

        private void ConnectObservers()
        {
            _publishObserver = new LoggingPublishObserver(LogObserver);
            _sendObserver = new LoggingSendObserver(LogObserver);
            _azureBus.ConnectPublishObserver(_publishObserver);
            _azureBus.ConnectSendObserver(_sendObserver);
        }

        /// <summary>
        /// 1.  The bus needs to be started in order to receive Request-Reply replies.
        /// 
        /// If not intending to use Request-Reply, or Fault-To, or Reply-To, no need to start the bus with .Start(),
        /// just configure it and use it to publish Events or get send-endpoints to send Commands/Queries.
        /// </summary>
        private void ConfigureBus()
        {
            try
            {
                // You must configure this in the a config.json file (see the example)
                var connstring = new JsonConfigFileReader().GetValue("AzureSbConnectionString");
                _azureBus = new AzureSbBusConfigurator(connstring).CreateBus();

                LogLine("AzureSB Bus started. ConnString: " + connstring);

                // This is necessary in order to receive replies from the request/reply mechanism. 
                // LAB: Try turn it off and see what happens when you send request/replies ..
                //await _azureBus.StartAsync();

                LogLine($"Bus Created. Bus.Adress: {_azureBus.Address}");
            }
            catch (Exception ex)
            {
                LogError("Exception starting Azure Bus!! " + ex.Message);
            }
        }

        /// <summary>
        /// Use the bus to Publish() Events that any listeners will catch up on. Messages will be posted to a Topic in AzureSb.
        /// </summary>
        private async void btnPublishEvent_Click(object sender, EventArgs e)
        {
            try
            {
                await _azureBus.Publish<IBarEvent>(new BarEvent
                {
                    Id = Guid.NewGuid(),
                    Text = txtMessageText.Text,
                    TimeStampSent = DateTime.Now
                });

                LogLine($"Message ({nameof(BarEvent)}) sent OK.");
            }
            catch (Exception ex)
            {
                LogError($"Exception! \n ExType: {ex.GetType().Name}\r\n ExMessage: {ex.Message}\r\n");
            }
        }

        /// <summary>
        /// Use the bus to get a Send Endpoint that in turn will be used for sending Commands/Queries.
        /// </summary>
        private async void btnSendCommand_Click(object sender, EventArgs e)
        {
            try
            {
                // Need a Send Endpoint in order to know where to deliver the messages.
                _sagaSendPoint = await _azureBus.GetSendEndpointAsync(_sagaQueueName);

                // Create a command to send, depending on what is chosen in the dropdown. 
                // Note: The created object is still declared as IUpdateFooCommand, which will have its effect on Masstransit when sending unless you convert it upon sending!
                IUpdateFooCommand commandToSend = ChooseCommandByDropdown();
                commandToSend.Id = Guid.NewGuid();
                commandToSend.Text = txtMessageText.Text;
                commandToSend.TimeStampSent = DateTime.Now;

                // Note: Best is NOT to Send Commands as an interface, but the concrete type. So the intended Consumer actually get the message.
                // Because if sent as an interface - Consumers would only understand and be able to consume that very interface, not any concrete class.
                // So if you have a Consumer<ConcreteClass> configured to listen on an EndPoint, but send as Send<Interface>, the message would not be understooud
                // and hence moved to the _skipped queue.
                switch (drpCommandType.SelectedIndex)
                {
                    case 0:_sagaSendPoint = await _azureBus.GetSendEndpointAsync(_sagaQueueName);
                        await _sagaSendPoint.Send((UpdateFooCommand)commandToSend);
                        LogLine($"Message ({nameof(UpdateFooCommand)}) sent OK.");
                        break;
                    case 1:
                        await _sagaSendPoint.Send((UpdateFooVersion2Command)commandToSend);
                        LogLine($"Message ({nameof(UpdateFooVersion2Command)}) sent OK.");
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                LogError($"Exception! \r\n ExType: {ex.GetType().Name}\r\n ExMessage: {ex.Message}\r\n");
            }
        }

        private async void btnSagaStart_Click(object sender, EventArgs e)
        {
            _lastSagaCorrelationId = Guid.NewGuid();
            try
            {
                if (string.IsNullOrEmpty(txtSagaCorrelate.Text.Trim()))
                    throw new ArgumentNullException($"{nameof(txtSagaCorrelate)}");

                _sagaSendPoint = await _azureBus.GetSendEndpointAsync(_sagaQueueName);

                await _sagaSendPoint.Send(new SagaStartUpdatesCommand
                {
                    // We can either something pretty unique to correlate against. OrderId could be a candidate.
                    UniqueText = txtSagaCorrelate.Text.Trim(),
                    // OR! If the Saga-starting message is under your control, you can simplify things by generating your own CorrelationId.
                    CorrelationId = _lastSagaCorrelationId
                });

                LogLine($"Message ({nameof(SagaStartUpdatesCommand)}) sent OK \r\n");
            }
            catch (Exception ex)
            {
                LogError($"Exception! \r\n ExType: {ex.GetType().Name}\r\n ExMessage: {ex.Message}\r\n");
            }
        }

        private async void btnSagaUpdateProducts_Click(object sender, EventArgs e)
        {
            try
            {
                _sagaSendPoint = await _azureBus.GetSendEndpointAsync(_sagaQueueName);
                // ReSharper disable once UseObjectOrCollectionInitializer
                var cmd = new SagaUpdateProductsBatchCommand();
                //cmd. = txtSagaCorrelate.Text.Trim();
                cmd.CorrelationId = _lastSagaCorrelationId;
                cmd.Products.Add(new ProductData());
                cmd.Products.Add(new ProductData());
                cmd.Products.Add(new ProductData());
                cmd.Products.Add(new ProductData());
                cmd.Products.Add(new ProductData());

                await _sagaSendPoint.Send(cmd);

                LogLine($"Message ({nameof(SagaUpdateProductsBatchCommand)}) sent OK \r\n");
            }
            catch (Exception ex)
            {
                LogError($"Exception);ption! \r\n ExType: {ex.GetType().Name}\r\n ExMessage: {ex.Message}\r\n");
            }
        }

        private async void btnSagaRollback_Click(object sender, EventArgs e)
        {
            try
            {
                _sagaSendPoint = await _azureBus.GetSendEndpointAsync(_sagaQueueName);
                await _sagaSendPoint.Send(new SagaRollbackUpdatesCommand()
                {
                    CorrelationId = _lastSagaCorrelationId
                });

                LogLine($"Message ({nameof(SagaRollbackUpdatesCommand)}) sent OK \r\n");
            }
            catch (Exception ex)
            {
                LogError($"Exception! \r\n ExType: {ex.GetType().Name}\r\n ExMessage: {ex.Message}\r\n");
            }
        }

        private async void btnSagaCommit_Click(object sender, EventArgs e)
        {
            try
            {
                _sagaSendPoint = await _azureBus.GetSendEndpointAsync(_sagaQueueName);
                await _sagaSendPoint.Send(new SagaCommitUpdatesCommand
                {
                    CorrelationId = _lastSagaCorrelationId
                });

                LogLine($"Message ({nameof(SagaCommitUpdatesCommand)}) sent OK \r\n");
            }
            catch (Exception ex)
            {
                LogError($"Exception! \r\n ExType: {ex.GetType().Name}\r\n ExMessage: {ex.Message}\r\n");
            }
        }

        private void btnSendRequestReply_Click(object sender, EventArgs e)
        {
            try
            {
                // Setup local variables for the loop
                var requestTasks = new List<Task>();
                var totalBarsToSend = int.Parse(drpTotalMessagesToSend.Text);
                var barsInEach = int.Parse(drpBatchSize.Text);
                var totalBarsSent = 0;

                // Use our custom code (extension method) to figure out where to deliver a request-reply style of a command/query.
                // You can do it manually but it requires lot of code.
                var client = _azureBus.CreateRequestClient<ServeBarsCommand, ServeBarsResponse>(TimeSpan.FromSeconds(5));

                // Basic validation
                if (barsInEach > totalBarsToSend)
                    throw new Exception("Error! Batchsize greater than the total. Makes no sense!");

                // Start loop until all bars are delivered
                while (ShouldKeepSending(totalBarsSent, totalBarsToSend))
                {
                    // Figure out how many bars to send in the next command
                    var thisBatchSize = NextBatchSize(totalBarsSent: totalBarsSent, barsInEachCommand: barsInEach, totalBarsToSend: totalBarsToSend);

                    // Declare a command and fill it with next batch of bars
                    var command = new ServeBarsCommand();
                    command.BarOwner = $"John Doe-{totalBarsSent}";
                    command.Bars = GetBarsToUpdate(thisBatchSize);

                    // Send the command in a task, and log when we get a reply
                    var task = client
                        .Request(command)
                        .ContinueWith(async (t) =>
                        {
                            var response = await t;

                            // Output on main thread since we're in a task continuation.
                            Invoke(new Action(() =>
                            {
                                LogLine($"Reply ({response.ServedCounter}): {response.AckText}\r\n");
                            }));
                        });
                    requestTasks.Add(task);

                    totalBarsSent += thisBatchSize;
                }

                LogLine($"Sent {totalBarsSent} Commands, now waiting for all to complete ..\r\n");

                // Wait for all to complete
                Task.WaitAll(requestTasks.ToArray());
            }
            catch (Exception ex)
            {
                LogError($"Exception when doing Request/Reply. \r\n ExType: {ex.GetType().Name}\r\n ExMessage: {ex.Message}\r\n");
            }
        }

        #region Helper methods (No need to look at this code)

        private void PopulateUiElements()
        {
            drpTotalMessagesToSend.Items.Add(1);
            drpTotalMessagesToSend.Items.Add(5);
            drpTotalMessagesToSend.Items.Add(20);
            drpTotalMessagesToSend.Items.Add(50);
            drpTotalMessagesToSend.SelectedIndex = 0;

            drpBatchSize.Items.Add(1);
            drpBatchSize.Items.Add(5);
            drpBatchSize.Items.Add(10);
            drpBatchSize.SelectedIndex = 0;

            drpCommandType.Items.Add("UpdateFooCommand");
            drpCommandType.Items.Add("UpdateFooVersion2Command");
            drpCommandType.SelectedIndex = 0;
        }

        private List<Bar> GetBarsToUpdate(int thisBatchSize)
        {
            var ret = new List<Bar>(thisBatchSize);
            for (var i = 0; i < thisBatchSize; i++)
                ret.Add(new Bar { Flavour = "Beet" });
            return ret;
        }

        private int NextBatchSize(int totalBarsSent, int barsInEachCommand, int totalBarsToSend)
        {
            var remaining = totalBarsToSend - totalBarsSent;

            if (remaining > barsInEachCommand)
            {
                return barsInEachCommand;
            }
            else
            {
                // batchsize bigger than remaining
                return remaining;
            }
        }

        private bool ShouldKeepSending(int totalSent, int totalToSend)
        {
            return totalSent < totalToSend;
        }

        private IUpdateFooCommand ChooseCommandByDropdown()
        {
            IUpdateFooCommand commandToSend;
            if (drpCommandType.SelectedIndex == 0)
                commandToSend = new UpdateFooCommand();
            else if (drpCommandType.SelectedIndex == 1)
                commandToSend = new UpdateFooVersion2Command();
            else
                throw new NotImplementedException($"{nameof(drpCommandType)}");
            return commandToSend;
        }

        private void LogError(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> {text}\r\n");
                }));
            }
            else
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> {text}\r\n");
            }

        }

        private void LogLine(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> {text}\r\n");
                }));
            }
            else
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> {text}\r\n");
            }
        }

        private void LogObserver(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    txtObserverLog.AppendText($"{DateTime.Now:HH:mm:ss}> {text}\r\n");
                }));
            }
            else
            {
                txtObserverLog.AppendText($"{DateTime.Now:HH:mm:ss}> {text}\r\n");
            }
        }

        #endregion



    }
}
