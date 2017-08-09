using Orleankka;

namespace Example
{
    using System;

    public interface ISomeService
    {
        void SayHello(string msg, ActorRef actor);
    }
    
    public class SomeService : ISomeService
    {
        public class Options
        {
            public readonly string ConnectionString;

            public Options(string connectionString)
            {
                ConnectionString = connectionString;
            }
        }

        readonly Options options;

        public SomeService(Options options)
        {
            this.options = options;
        }

        public void SayHello(string msg, ActorRef actor)
        {
            Console.WriteLine("{0}, from actor resolved via native DI! My name is: {1}", msg, actor.Path);
            Console.WriteLine("Service connection string is: {0}", options.ConnectionString);
        }
    }
}
