using System.Globalization;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Demo
{
    public interface ITopicStorage
    {
        Task<int> ReadTotalAsync(string id);
        Task WriteTotalAsync(string id, int total);
    }

    public class TopicStorage : ITopicStorage
    {
        public static async Task<ITopicStorage> Init(CloudStorageAccount account)
        {
            var blobClient = account.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("topics");
            await container.CreateIfNotExistsAsync();

            return new TopicStorage(container);
        }

        readonly CloudBlobContainer container;

        TopicStorage(CloudBlobContainer container)
        {
            this.container = container;
        }

        public async Task<int> ReadTotalAsync(string id)
        {
            var blob = container.GetBlockBlobReference(GetBlobName(id));
            if (!(await blob.ExistsAsync()))
                return 0;

            var contents = await blob.DownloadTextAsync();
            return !string.IsNullOrWhiteSpace(contents)
                    ? int.Parse(contents)
                    : 0;
        }

        public Task WriteTotalAsync(string id, int total)
        {
            var blob = container.GetBlockBlobReference(GetBlobName(id));
            return blob.UploadTextAsync(total.ToString(CultureInfo.InvariantCulture));
        }

        static string GetBlobName(string id) => $"topic-{id}.json";
    }
}
