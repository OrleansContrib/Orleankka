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
        readonly string connectionString;

        public SomeService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void SayHello(string msg, ActorRef actor)
        {
            Console.WriteLine("{0}, from Autofac'ed actor! My name is: {1}", msg, actor.Path);
            Console.WriteLine("Service connection string is: {0}", connectionString);
        }
    }
}
