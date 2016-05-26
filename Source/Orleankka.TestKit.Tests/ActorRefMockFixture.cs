using System;

using NUnit.Framework;

namespace Orleankka.TestKit
{
    [TestFixture]
    public class ActorRefMockFixture
    {
        ActorRefMock actor;

        [SetUp]
        public void SetUp()
        {
            actor = new ActorRefMock(ActorPath.From("Orleankka.TestKit.TestActor", Guid.NewGuid().ToString("D")));
        }

        [Test]
        public async void Returns_default_result_for_unmatched_queries()
        {
            Assert.AreEqual(default(int), await actor.Ask<int>(new TestQuery()));
            Assert.AreEqual(default(object), await actor.Ask<object>(new TestQuery()));
        }

        [Test]
        public void Unconditionally_matches_commands_by_type()
        {
            actor
                .ExpectTell<TestCommand>()
                .Throw(new ApplicationException("boo!"));

            Assert.Throws<ApplicationException>(
                async () => await actor.Tell(new TestCommand()));
        }
        
        [Test]
        public async void Unconditionally_matches_queries_by_type()
        {
            actor
                .ExpectAsk<TestQuery>()
                .Return(111);

            Assert.That(await actor.Ask<int>(new TestQuery()), 
                Is.EqualTo(111));
        }

        [Test]
        public async void Matches_when_expression_match()
        {
            actor
                .ExpectAsk<TestQuery>(x => 
                    x.Field == "foo" && 
                    x.AnotherField == "bar")
                 .Return(111);

            var query = new TestQuery {Field = "foo", AnotherField = "bar"};
            Assert.That(await actor.Ask<int>(query), 
                Is.EqualTo(111));
        }

        [Test]
        public async void Does_not_match_when_expression_doesnt()
        {
            actor
                .ExpectAsk<TestQuery>(x => 
                    x.Field == "foo" && 
                    x.AnotherField == "^_^")
                .Return(111);

            var query = new TestQuery {Field = "foo", AnotherField = "bar"};
            Assert.AreEqual(default(int), await actor.Ask<int>(query));
        }

        [Test]
        public async void Matches_indefinite_number_of_times_by_default()
        {
            actor
                .ExpectAsk<TestQuery>()
                .Return(111);

            Assert.That(await actor.Ask<int>(new TestQuery()),
                Is.EqualTo(111));

            Assert.That(await actor.Ask<int>(new TestQuery()),
                Is.EqualTo(111));
        }        
        
        [Test]
        public async void Matches_specified_number_of_times_when_configured()
        {
            actor
                .ExpectAsk<TestQuery>()
                .Return(111)
                .Once();

            Assert.That(await actor.Ask<int>(new TestQuery()),
                Is.EqualTo(111));

            Assert.That(await actor.Ask<int>(new TestQuery()),
                Is.EqualTo(default(int)));
        }

        [Test]
        public async void When_multiple_expectations_match_will_use_the_first_one_in_order()
        {
            actor
                .ExpectAsk<TestQuery>()
                .Return(111)
                .Once();

            actor
                .ExpectAsk<TestQuery>()
                .Return(222);

            Assert.That(await actor.Ask<int>(new TestQuery()),
                Is.EqualTo(111));

            Assert.That(await actor.Ask<int>(new TestQuery()),
                Is.EqualTo(222));
        }

        class TestActor : Actor
        {}

        class TestCommand
        {}

        class TestQuery
        {
            public string Field;
            public string AnotherField;
        }
    }
}
