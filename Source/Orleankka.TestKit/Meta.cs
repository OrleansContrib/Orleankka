using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Orleankka.Meta;

namespace Orleankka.TestKit
{
    namespace Meta
    {
        public static class Extensions
        {
            public static CommandExpectation<TCommand> ExpectCommand<TCommand>(
                this ActorRefMock mock,
                Expression<Func<TCommand, bool>> match = null)
                where TCommand : Command
            {
                return mock.ExpectTell(match);
            }

            public static QueryExpectation<TQuery> ExpectQuery<TQuery>(
                this ActorRefMock mock,
                Expression<Func<TQuery, bool>> match = null)
                where TQuery : Query
            {
                return mock.ExpectAsk(match);
            }

            public static void Reset(this ActorRefMock mock)
            {
                mock.Received.Clear();
            }

            public static IEnumerable<Command> Commands(this ActorRefMock mock)
            {
                return mock.Received.Select(x => x.Message).Cast<Command>();
            }

            public static IEnumerable<Query> Queries(this ActorRefMock mock)
            {
                return mock.Received.Select(x => x.Message).Cast<Query>();
            }

            public static bool DidNotReceiveAnyCommands(this ActorRefMock mock)
            {
                return !mock.Commands().Any();
            }

            public static bool DidNotReceiveAnyQueries(this ActorRefMock mock)
            {
                return !mock.Queries().Any();
            }

            public static TCommand FirstCommand<TCommand>(this ActorRefMock mock)
            {
                return mock.Commands().OfType<TCommand>().FirstOrDefault();
            }

            public static TQuery FirstQuery<TQuery>(this ActorRefMock mock)
            {
                return mock.Queries().OfType<TQuery>().FirstOrDefault();
            }

            public static IEnumerable<Event> Events(this ObserverCollectionMock mock)
            {
                return mock.RecordedNotifications.Cast<Event>();
            }

            public static TEvent FirstEvent<TEvent>(this ObserverCollectionMock mock) where TEvent : Event
            {
                return mock.Events().OfType<TEvent>().FirstOrDefault();
            }
        }
    }
}
