using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using MassTransit;
using Messaging.Infrastructure.ServiceBus.BusConfigurator;
using RequestReply.Shared.FooBar.Messages;
using RequestReply.Shared.Shared.Tools;
using RequestReply.Shared.UpdateProducts.Saga.Messages;

namespace RequestReply.Sender
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable UseObjectOrCollectionInitializer
    public partial class frmSender : Form
    {
        private IBusControl _azureBus;
        private Guid _correlationId;
        private ISendEndpoint _sagaUpdateProductsBatchSendPoint;
        private ISendEndpoint _sagaStartSendpoint;
        private ISendEndpoint _sagaRollbackSendPoint;
        private ISendEndpoint _sagaCommitSendPoint;

        public frmSender()
        {
            InitializeComponent();

            PopulateUiElements();

            StartAzureBus();
        }

        /// <summary>
        /// 1.  The bus needs to be started in order to receive Request-Reply replies.
        /// 
        /// If not intending to use Request-Reply, or Fault-To, or Reply-To, no need to start the bus with .Start(),
        /// just configure it and use it to publish Events or get send-endpoints to send Commands/Queries.
        /// </summary>
        private async void StartAzureBus()
        {
            try
            {
                // You must configure this in the a config.json file (see the example)
                var connstring = new JsonConfigFileReader().GetValue("AzureSbConnectionString");
                _azureBus = AzureSbBusConfigurator.CreateBus(connstring);

                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> AzureSB Bus started. " + connstring + " \n");

                // This is necessary in order to receive replies from the request/reply mechanism. 
                // LAB: Try turn it off and see what happens when you send request/replies ..
                await _azureBus.StartAsync();

                _sagaStartSendpoint = await _azureBus.GetSendEndpointAsync<IStartUpdatingProductsCommand>();
                _sagaUpdateProductsBatchSendPoint = await _azureBus.GetSendEndpointAsync<IUpdateProductsBatchCommand>();
                _sagaRollbackSendPoint = await _azureBus.GetSendEndpointAsync<IRollbackUpdatingProductsCommand>();
                _sagaCommitSendPoint = await _azureBus.GetSendEndpointAsync<ICommitUpdatingProductsCommand>();

                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Bus started. Bus.Adress: {_azureBus.Address} \n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Exception starting Azure Bus!! " + ex.Message + " \n");
            }
        }

        /// <summary>
        /// Use the bus to Publish() Events that any listeners will catch up on. Messages will be posted to a Topic in AzureSb.
        /// </summary>
        private async void btnPublishEvent_Click(object sender, EventArgs e)
        {
            try
            {
                await _azureBus.Publish(new BarEvent
                {
                    Id = Guid.NewGuid(),
                    Text = txtMessageText.Text,
                    TimeStampSent = DateTime.Now
                });

                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Message ({nameof(BarEvent)}) sent OK \n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Exception! \n ExType: {ex.GetType().Name}\n ExMessage: {ex.Message}\n");
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
                var commandSendpoint = await _azureBus.GetSendEndpointAsync<IUpdateFooCommand>();

                // Create a command to send, depending on what is chosen in the dropdown. 
                // Note: The created object is still declared as IUpdateFooCommand, which will have its effect on Masstransit when sending unless you convert it upon sending!
                IUpdateFooCommand commandToSend = CreateCommandBasedOnDropdown();
                commandToSend.Id = Guid.NewGuid();
                commandToSend.Text = txtMessageText.Text;
                commandToSend.TimeStampSent = DateTime.Now;

                // Note: Best is NOT to Send Commands as an interface, but the concrete type. So the intended Consumer actually get the message.
                // Because if sent as an interface - Consumers would only understand and be able to consume that very interface, not any concrete class.
                // So if you have a Consumer<ConcreteClass> configured to listen on an EndPoint, but send as Send<Interface>, the message would not be understooud
                // and hence moved to the _skipped queue.
                switch (drpCommandType.SelectedIndex)
                {
                    case 0:
                        await commandSendpoint.Send((UpdateFooCommand)commandToSend);
                        break;
                    case 1:
                        await commandSendpoint.Send((UpdateFooVersion2Command)commandToSend);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Message ({nameof(UpdateFooCommand)}) sent OK \n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Exception! \n ExType: {ex.GetType().Name}\n ExMessage: {ex.Message}\n");
            }
        }

        private async void btnSagaStart_Click(object sender, EventArgs e)
        {
            _correlationId = Guid.NewGuid();
            try
            {
                if (string.IsNullOrEmpty(txtSagaCorrelate.Text.Trim()))
                    throw new ArgumentNullException($"{nameof(txtSagaCorrelate)}");

                await _sagaStartSendpoint.Send(new StartUpdatingProductsCommand
                {
                    // We need *something* pretty unique to correlate against
                    UniqueText = txtSagaCorrelate.Text.Trim(),
                    // If the Saga-starting message is under your control, you can simplify things by generating your own CorrelationId
                    CorrelationId = _correlationId
                });

                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Message ({nameof(StartUpdatingProductsCommand)}) sent OK \n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Exception! \n ExType: {ex.GetType().Name}\n ExMessage: {ex.Message}\n");
            }
        }

        private async void btnSagaUpdateProducts_Click(object sender, EventArgs e)
        {
            try
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                var cmd = new UpdateProductsBatchCommand();
                //cmd. = txtSagaCorrelate.Text.Trim();
                cmd.CorrelationId = _correlationId;
                cmd.Products.Add(new ProductData());
                cmd.Products.Add(new ProductData());
                cmd.Products.Add(new ProductData());
                cmd.Products.Add(new ProductData());
                cmd.Products.Add(new ProductData());

                await _sagaUpdateProductsBatchSendPoint.Send(cmd);

                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Message ({nameof(UpdateProductsBatchCommand)}) sent OK \n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Exception! \n ExType: {ex.GetType().Name}\n ExMessage: {ex.Message}\n");
            }
        }

        private async void btnSagaRollback_Click(object sender, EventArgs e)
        {
            try
            {
                await _sagaRollbackSendPoint.Send(new RollbackUpdatingProductsCommand()
                {
                    CorrelationId = _correlationId
                });

                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Message ({nameof(RollbackUpdatingProductsCommand)}) sent OK \n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Exception! \n ExType: {ex.GetType().Name}\n ExMessage: {ex.Message}\n");
            }
        }

        private async void btnSagaCommit_Click(object sender, EventArgs e)
        {
            try
            {
                await _sagaCommitSendPoint.Send(new CommitUpdatingProductsCommand
                {
                    CorrelationId = _correlationId
                });

                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Message ({nameof(CommitUpdatingProductsCommand)}) sent OK \n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Exception! \n ExType: {ex.GetType().Name}\n ExMessage: {ex.Message}\n");
            }
        }

        #region Helper methods (No need to look at this code)

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
                                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Reply ({response.ServedCounter}): {response.AckText}\n");
                            }));
                        });
                    requestTasks.Add(task);

                    totalBarsSent += thisBatchSize;
                }

                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Sent {totalBarsSent} Commands, now waiting for all to complete ..\n");

                // Wait for all to complete
                Task.WaitAll(requestTasks.ToArray());
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Exception when doing Request/Reply. \n ExType: {ex.GetType().Name}\n ExMessage: {ex.Message}\n");
            }
        }

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

        private IUpdateFooCommand CreateCommandBasedOnDropdown()
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

        #endregion



    }
}
