using Authen.Application.EventBus;
using MassTransit;
namespace Authen.Infrastructure.EventBus
{
    /// <summary>
    /// Temporary event bus implementation to satisfy DI.
    /// Replace with a real message broker publisher 
    /// </summary>
    public class MassTransitEventBus : IEventBus
    {

        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitEventBus(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishAsync<T>(T @event)
        {
            await _publishEndpoint.Publish(@event);
        }
    }

}
