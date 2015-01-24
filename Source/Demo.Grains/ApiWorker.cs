using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Demo
{
    public interface IApiWorker
    {
        Task<int> Search(string subject);
    }

    static class ApiWorkerFactory
    {
        static readonly IDictionary<string, IApiWorker> registry = new Dictionary<string, IApiWorker>
        {
            {"facebook", new Faulty(new FacebookApiWorker("facebook.com"))},
            {"twitter",  new Faulty(new TwitterApiWorker("twitter.com"))},
        };

        public static Func<IApiWorker> Create(Func<string> api)
        {
            return () => registry[api()];
        }
    }

    abstract class ApiWorkerBase : IApiWorker
    {
        public string EndPoint { get; private set; }

        protected ApiWorkerBase(string endPoint)
        {
            EndPoint = endPoint;
        }

        public abstract Task<int> Search(string subject);
    }

    class FacebookApiWorker : ApiWorkerBase
    {
        readonly Random random = new Random();

        public FacebookApiWorker(string endPoint)
            : base(endPoint)
        {}

        public override Task<int> Search(string subject)
        {
            return Task.FromResult(random.Next(0, 50));
        }
    }

    class TwitterApiWorker : ApiWorkerBase  
    {
        readonly Random random = new Random();

        public TwitterApiWorker(string endPoint)
            : base(endPoint)
        {}

        public override Task<int> Search(string subject)
        {
            return Task.FromResult(random.Next(0, 50));
        }
    }

    class Faulty : ApiWorkerBase
    {
        readonly ApiWorkerBase api;

        bool faulted;
        long requests;

        public Faulty(ApiWorkerBase api) : base(api.EndPoint)
        {
            this.api = api;
        }

        public override Task<int> Search(string subject)
        {
            requests++;

            if (faulted)
            {
                if (requests % 5 == 0)
                    faulted = false;

                throw new HttpException(500, api.EndPoint + " is down");
            }
            
            if (requests % 10 == 0)
                faulted = true;
            
            return api.Search(subject);
        }
    }
}
