using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Orleankka.Meta
{
    using TestKit;

    public static class Extensions
    {
        public static TellExpectation<TCommand> ExpectCommand<TCommand>(
            this ActorRefMock mock,
            Expression<Func<TCommand, bool>> match = null)
            where TCommand : Command
        {
            return mock.ExpectTell(match);
        }

        public static AskExpectation<TQuery> ExpectQuery<TQuery>(
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
            return !Commands(mock).Any();
        }

        public static bool DidNotReceiveAnyQueries(this ActorRefMock mock)
        {
            return !Queries(mock).Any();
        }

        public static TCommand FirstCommand<TCommand>(this ActorRefMock mock)
        {
            return Commands(mock).OfType<TCommand>().FirstOrDefault();
        }

        public static TQuery FirstQuery<TQuery>(this ActorRefMock mock)
        {
            return Queries(mock).OfType<TQuery>().FirstOrDefault();
        }

        public static IEnumerable<Event> Events(this ObserverCollectionMock mock)
        {
            return mock.RecordedNotifications.Cast<Event>();
        }

        public static TEvent FirstEvent<TEvent>(this ObserverCollectionMock mock) where TEvent : Event
        {
            return Events(mock).OfType<TEvent>().FirstOrDefault();
        }
    }
}
