using System;
using System.Globalization;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Demo
{
    public interface ITopicStorage
    {
        Task<TopicState> ReadStateAsync(string id);
        Task WriteStateAsync(string id, TopicState state);
    }

    public class TopicStorage : ITopicStorage
    {
        public static ITopicStorage Instance
        {
            get; private set;
        }

        public static void Init(CloudStorageAccount account)
        {
            var blobClient = account.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("topics");
            container.CreateIfNotExists();

            Instance = new TopicStorage(container);
        }

        readonly CloudBlobContainer container;

        TopicStorage(CloudBlobContainer container)
        {
            this.container = container;
        }

        public async Task<TopicState> ReadStateAsync(string id)
        {
            var state = new TopicState();

            var blob = container.GetBlockBlobReference(GetBlobName(id));
            if (!(await blob.ExistsAsync()))
                return state;

            var contents = await blob.DownloadTextAsync();
            if (string.IsNullOrWhiteSpace(contents))
                return state;

            state.Total = int.Parse(contents);
            return state;
        }

        public Task WriteStateAsync(string id, TopicState state)
        {
            var blob = container.GetBlockBlobReference(GetBlobName(id));
            return blob.UploadTextAsync(state.Total.ToString(CultureInfo.InvariantCulture));
        }

        static string GetBlobName(string id)
        {
            return string.Format("topic-{0}.json", id);
        }
    }
}
