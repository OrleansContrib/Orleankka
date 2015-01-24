using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Orleankka;
using Orleans.Concurrency;

namespace Demo
{
    [Immutable, Serializable]
    public class CreateTopic : Command
    {
        public readonly string Query;
        public readonly IReadOnlyDictionary<string, TimeSpan> Schedule;

        public CreateTopic(string query, IDictionary<string, TimeSpan> schedule)
        {
            Query = query;
            Schedule = new ReadOnlyDictionary<string, TimeSpan>(schedule);
        }
    }

    public interface ITopic : IActor {}
}