using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_diff.Common.EventAggregator
{
    public interface IEventAggregator
    {
        void Subscribe<T>(Action<T> action) where T : IApplicationEvent;
        void Unsubscribe<T>(Action<T> action) where T : IApplicationEvent;
        void Publish<T>(T message) where T : IApplicationEvent;
    }
}
