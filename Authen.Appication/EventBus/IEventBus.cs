using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Application.EventBus
{
    public interface IEventBus
    {
        Task PublishAsync<T>(T @event);
    }
}
