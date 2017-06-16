using Messaging.Infrastructure.ServiceBus.Models;

namespace RequestReply.Shared.Tools
{
    public static class AzureSbConfigCreator
    {
        public static BusConfiguration CreateAzureSbConfig()
        {
            var rdr = new JsonConfigFileReader();
            return new BusConfiguration
            {
                ConnectionUri = rdr.GetValue("AzureSbConnectionString"),
                Login = "notneeded",
                Password = "notneeded"
            };
        }
    }
}
