using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    class ObservableProxyMock : IObservableProxy
    {
        public bool Disposed
        {
            get; private set;
        }

        public readonly Dictionary<string, List<Callback>> Recorded =
                    new Dictionary<string, List<Callback>>();

        public Task Attach(string source, params Callback[] callbacks)
        {
            List<Callback> list;
            if (!Recorded.TryGetValue(source, out list))
            {
                list = new List<Callback>();
                Recorded[source] = list;
            }

            list.RemoveAll(x => callbacks.Any(y => x.Notification == y.Notification));
            list.AddRange(callbacks);

            return TaskDone.Done;
        }

        public Task Detach(string source, params Type[] notifications)
        {
            List<Callback> list;
            if (!Recorded.TryGetValue(source, out list))
                throw new ApplicationException("No callbacks were previously recorded for " + source);

            list.RemoveAll(x => notifications.Any(y => x.Notification == y));

            return TaskDone.Done;
        }

        public void Dispose()
        {
            Disposed = true;
        }

        public void Reset()
        {
            Recorded.Clear();
            Disposed = false;
        }
    }
}
