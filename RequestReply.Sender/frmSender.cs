using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MassTransit;
using Messaging.Infrastructure.ServiceBus.BusConfigurator;
using RequestReply.Shared;
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

        private void StartAzureBus()
        {
            try
            {
                var connstring = new JsonConfigFileReader().GetValue("AzureSbConnectionString");
                _azureBus = AzureSbBusConfigurator.CreateBus(connstring);
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}> AzureSB Bus started. " + connstring + " \n");
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

        private async void btnSend_Click(object sender, EventArgs e)
        {
            var commandSendpoint = await _azureBus.GetSendEndpointAsync<IUpdateFooCommand>();
            IUpdateFooCommand commandToSend = CreateCommandBasedOnDropdown();

            commandToSend.Id = Guid.NewGuid();
            commandToSend.Text = txtMessageText.Text;
            commandToSend.TimeStampSent = DateTime.Now;

            // Best is not to Send as an interface, but the concrete type.
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
