using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MassTransit;
using Messaging.Infrastructure.ServiceBus.BusConfigurator;
using RequestReply.Shared;
using RequestReply.Shared.Messages;
using RequestReply.Shared.Tools;

namespace RequestReply.Sender
{
    // ReSharper disable once InconsistentNaming
    public partial class frmSender : Form
    {
        private IBusControl _azureBus;

        public frmSender()
        {
            InitializeComponent();

            PopulateUiElements();

            StartAzureBus();
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

        private async void StartAzureBus()
        {
            try
            {
                var connstring = new JsonConfigFileReader().GetValue("AzureSbConnectionString");
                _azureBus = AzureSbBusConfigurator.CreateBus(connstring);
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> AzureSB Bus started. " + connstring + " \n");

                // This is necessary in order to receive replies from the request/reply mechanism.
                await _azureBus.StartAsync();
                
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Bus started. Bus.Adress: {_azureBus.Address} \n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Exception starting Azure Bus!! " + ex.Message + " \n");
            }
        }

        private async void btnPublishEvent_Click(object sender, EventArgs e)
        {
            await _azureBus.Publish(new BarEvent
            {
                Id = Guid.NewGuid(),
                Text = txtMessageText.Text,
                TimeStampSent = DateTime.Now
            });

            txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Message ({nameof(BarEvent)}) sent OK \n");

        }

        private async void btnSendCommand_Click(object sender, EventArgs e)
        {
            var commandSendpoint = await _azureBus.GetSendEndpointAsync<IUpdateFooCommand>();

            // Create a command to send, depending on what is chosen in the dropdown.
            // Note that the object is still declared as IUpdateFooCommand which will have its effect on Masstransit when sending.
            IUpdateFooCommand commandToSend = CreateCommandBasedOnDropdown();
            commandToSend.Id = Guid.NewGuid();
            commandToSend.Text = txtMessageText.Text;
            commandToSend.TimeStampSent = DateTime.Now;

            // Note: Best is NOT to Send as an interface, but the concrete type.
            // Because if sent as an interface - Consumers would only understand and be able to consume that very interface, not any concrete class,
            // And that would be an error that could lead to messages being sent to the _skipped queue.
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

        private async void btnSendRequestReply_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Either create a client manually, by figuring out the address you want the replies to:
                //var helloRequestUri = new Uri(_azureBus.ExtractBusAddress() + "/" + nameof(HelloQuery));
                //var client = _azureBus.CreateRequestClient<HelloQuery, HelloResponse>(helloRequestUri, TimeSpan.FromSeconds(5));

                // 2. Or easier, use our code:
                var client = _azureBus.CreateRequestClient<HelloQuery, HelloResponse>(TimeSpan.FromSeconds(5));

                var response = await client.Request(new HelloQuery() { MyName = "John Doe" });

                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Reply ({response.Counter}): {response.HelloText}\n" );
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> Exception when doing Request/Reply. \n ExType: {ex.GetType().Name}\n ExMessage: {ex.Message}\n");
            }
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
    }
}
