using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;
using Orleans.Providers.Streams.SimpleMessageStream;

namespace Orleankka.TestKit
{
    [TestFixture]
    public class StreamRefStubFixture
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            stub = new StreamRefStub(StreamPath.From(typeof(SimpleMessageStreamProvider), Guid.NewGuid().ToString("D")));
        }

        #endregion

        StreamRefStub stub;

        [Test]
        public async Task When_on_completed()
        {
            var onCompleted = false;

            await stub.SubscribeAsync<object>((o, token) => TaskDone.Done, (e) => TaskDone.Done, () =>
            {
                onCompleted = true;
                return TaskDone.Done;
            });

            await stub.OnCompletedAsync();

            Assert.IsTrue(onCompleted);
        }

        [Test]
        public async Task When_on_error()
        {
            var onError = false;
            var exception = new Exception("");

            await stub.SubscribeAsync<object>((o, token) => TaskDone.Done,
                                              e =>
                                              {
                                                  onError = true;
                                                  Assert.AreEqual(exception, e);
                                                  return TaskDone.Done;
                                              });

            await stub.OnErrorAsync(exception);

            Assert.IsTrue(onError);
        }

        [Test]
        public async Task When_on_next()
        {
            var onNext = false;

            await stub.SubscribeAsync<string>((o, token) =>
            {
                onNext = true;

                Assert.IsEmpty(o);

                return TaskDone.Done;
            });

            await stub.OnNextAsync(String.Empty);

            Assert.IsTrue(onNext);
        }

        [Test]
        public async Task When_subscribe_subscription_is_stored()
        {
            var sub = await stub.SubscribeAsync<object>((o, token) => TaskDone.Done);
            var handles = await stub.GetAllSubscriptionHandles();

            Assert.AreEqual(1, handles.Count);
            Assert.AreEqual(sub, handles.First());
        }

        [Test]
        public async Task When_multiple_subscriptions()
        {
            var onNext1 = false;
            var onNext2 = false;

            var sub1 = await stub.SubscribeAsync<object>((o, token) =>
            {
                onNext1 = true;
                return TaskDone.Done;
            });
            var sub2 = await stub.SubscribeAsync<object>((o, token) =>
            {
                onNext2 = true;
                return TaskDone.Done;
            });

            Assert.AreNotEqual(sub1, sub2);

            var handles = await stub.GetAllSubscriptionHandles();

            Assert.AreEqual(2, handles.Count);

            await stub.OnNextAsync(String.Empty);

            Assert.IsTrue(onNext1);
            Assert.IsTrue(onNext2);
        }

        [Test]
        public async Task When_unsubscribe_subscription_is_deleted()
        {
            var sub = await stub.SubscribeAsync<object>((o, token) => TaskDone.Done);

            var handles = await stub.GetAllSubscriptionHandles();
            Assert.AreEqual(1, handles.Count);

            await sub.UnsubscribeAsync();

            handles = await stub.GetAllSubscriptionHandles();
            Assert.AreEqual(0, handles.Count);
        }
    }
}