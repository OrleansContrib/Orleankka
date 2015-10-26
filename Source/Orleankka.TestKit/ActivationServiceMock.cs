using System;
using System.Collections.Generic;

namespace Orleankka.TestKit
{
    using Services;

    public class ActivationServiceMock : IActivationService
    {
        readonly List<RecordedDeactivationRequest> requests = new List<RecordedDeactivationRequest>();

        void IActivationService.DeactivateOnIdle()
        {
            requests.Add(new DeactivateOnIdle());
        }

        void IActivationService.DelayDeactivation(TimeSpan period)
        {
            requests.Add(new DelayDeactivation(period));
        }

        public IEnumerable<RecordedDeactivationRequest> RecordedRequests => requests;
        public void Reset() => requests.Clear();
    }

    public abstract class RecordedDeactivationRequest
    {}

    public class DeactivateOnIdle : RecordedDeactivationRequest
    {}

    public class DelayDeactivation : RecordedDeactivationRequest
    {
        public readonly TimeSpan Period;

        public DelayDeactivation(TimeSpan period)
        {
            Period = period;
        }
    }
}
