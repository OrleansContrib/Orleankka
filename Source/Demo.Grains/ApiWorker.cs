using System;
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
        public static IApiWorker Create(string api)
        {
            if (api == "facebook")
                return new FacebookApiWorker();

            if (api == "twitter")
                return new TwitterApiWorker();

            throw new InvalidOperationException("Unknown api: " + api);
        }
    }

    class FacebookApiWorker : IApiWorker
    {
        readonly Random random = new Random();
        
        public Task<int> Search(string subject)
        {
            return Task.FromResult(random.Next(0, 100));
        }
    }

    class TwitterApiWorker : IApiWorker
    {
        readonly Random random = new Random();

        public Task<int> Search(string subject)
        {
            return Task.FromResult(random.Next(0, 100));
        }
    }

    class FaultyDemoWorker : IApiWorker
    {
        readonly Random random = new Random();
        readonly string api;

        bool faulted;
        long requests;

        public FaultyDemoWorker(string api)
        {
            this.api = api;
        }

        public Task<int> Search(string subject)
        {
            requests++;

            if (faulted)
            {
                if (requests % 5 == 0)
                    faulted = false;

                throw new HttpException(500, api + " is down");
            }
            
            if (requests % 20 == 0)
                faulted = true;
            
            return Task.FromResult(random.Next(0, 100));
        }
    }
}
