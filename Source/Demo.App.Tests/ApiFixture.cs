using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using NUnit.Framework;

using Orleankka.TestKit;
using Orleankka.TestKit.Meta;

namespace Demo
{
    [TestFixture]
    public class ApiFixture : ActorFixture
    {
        static readonly Search query = new Search("ПТН ПНХ");

        Api api;
        MockApiWorker worker;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            worker = new MockApiWorker();

            api = new Api("facebook", System, Timers, Observers, worker);
        }

        [Test]
        public void Locks_itself_and_notifies_when_failure_rate_exceeds_defined_threshold()
        {
            worker.ThrowException = true;

            Assert.Throws<ApiUnavailableException>(async ()=> await api.Handle(query));
            Assert.Throws<ApiUnavailableException>(async ()=> await api.Handle(query));
            Assert.Throws<ApiUnavailableException>(async ()=> await api.Handle(query));
            
            IsTrue(()=> Observers.Events().Count() == 1);
            IsTrue(()=> Observers.FirstEvent<AvailabilityChanged>().Available == false);
            IsTrue(()=> Timers.Count() == 1);

            var timer = Timer("check");
            IsTrue(() => timer.Callback == api.CheckAvailability);
            IsTrue(() => timer.Due == TimeSpan.FromSeconds(1));
            IsTrue(() => timer.Period == TimeSpan.FromSeconds(1));
        }

        class MockApiWorker : IApiWorker
        {
            public bool ThrowException;

            public Task<int> Search(string subject)
            {
                if (ThrowException)
                    throw new HttpException(500, "facebook.com is down");

                return Task.FromResult(1);
            }
        }
    }
}