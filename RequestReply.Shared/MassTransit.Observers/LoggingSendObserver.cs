using System;
using System.Threading.Tasks;
using MassTransit;

namespace RequestReply.Shared.MassTransit.Observers
{
    public class LoggingSendObserver : ISendObserver
    {
        private readonly Action<string> _logAction;

        public LoggingSendObserver(Action<string> logAction)
        {
            _logAction = logAction;
        }

        public Task PreSend<T>(SendContext<T> context) where T : class
        {
            var msg = $"PreSend: On: {context.DestinationAddress}";
            _logAction?.Invoke(msg);
            return Task.CompletedTask;
        }

        public Task PostSend<T>(SendContext<T> context) where T : class
        {
            return Task.CompletedTask;
        }

        public Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
        {
            var msg = $"SendFault (!!!): On: {context.DestinationAddress}";
            _logAction?.Invoke(msg);
            return Task.CompletedTask;
        }
    }
}
