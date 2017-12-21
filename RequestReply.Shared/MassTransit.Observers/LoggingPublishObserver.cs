using System;
using System.Threading.Tasks;
using MassTransit;

namespace RequestReply.Shared.MassTransit.Observers
{
    public class LoggingPublishObserver : IPublishObserver
    {
        private readonly Action<string> _logAction;

        public LoggingPublishObserver(Action<string> logAction)
        {
            _logAction = logAction;
            
        }

        public Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            var msg = $"PrePublish: On: {context.DestinationAddress}";
            _logAction?.Invoke(msg);
            return Task.CompletedTask;
        }

        public Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            return Task.CompletedTask;
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            var msg = $"PublishFault (!!!): On: {context.DestinationAddress}";
            _logAction?.Invoke(msg);
            return Task.CompletedTask;
        }
    }
}
