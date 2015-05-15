namespace Example
{
    using System;

    public interface ISomeService
    {
        void SayHello();
    }

    public class SomeService : ISomeService
    {
        public void SayHello()
        {
            Console.WriteLine("Hello from injected service.");
        }
    }
}
