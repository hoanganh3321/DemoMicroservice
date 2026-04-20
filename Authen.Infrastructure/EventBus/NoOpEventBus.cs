using Authen.Application.EventBus;

namespace Authen.Infrastructure.EventBus
{
    /// <summary>
    /// Development-safe event bus used when MassTransit license is not configured.
    /// </summary>
    public class NoOpEventBus : IEventBus
    {
        public Task PublishAsync<T>(T @event)
        {
            return Task.CompletedTask;
        }
    }
}
