using System;
using System.Collections.Generic;
using System.Linq;

using Orleankka.Services;

namespace Orleankka.TestKit
{
    public class ActivationServiceMock : IActivationService
    {
        public readonly List<RecordedDeactivationRequest> RecordedRequests = new List<RecordedDeactivationRequest>();

        void IActivationService.DeactivateOnIdle()
        {
            RecordedRequests.Add(new DeactivateOnIdle());
        }

        void IActivationService.DelayDeactivation(TimeSpan period)
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
