using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ActivationMock : IActivationService
    {
        public readonly List<RecordedDeactivationRequest> RecordedRequests = new List<RecordedDeactivationRequest>();

        void IActivation.DeactivateOnIdle()
        {
            RecordedRequests.Add(new DeactivateOnIdle());
        }

        void IActivation.DelayDeactivation(TimeSpan period)
        {
            RecordedRequests.Add(new DelayDeactivation(period));
        }
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
