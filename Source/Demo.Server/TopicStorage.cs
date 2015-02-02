using System;
using System.Globalization;
using System.Threading.Tasks;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

using Orleans;

namespace Demo
{
    public interface ITopicStorage
    {
        Task<int> ReadTotalAsync(string id);
        Task WriteTotalAsync(string id, int total);
    }

    public class TopicStorage : ITopicStorage
    {
        public static ITopicStorage Init(string connectionString)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var blobClient = account.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("topics");
            container.CreateIfNotExist();

            return new TopicStorage(container);
        }

        readonly CloudBlobContainer container;

        TopicStorage(CloudBlobContainer container)
        {
            this.container = container;
        }

        public Task<int> ReadTotalAsync(string id)
        {
            var blob = container
                .GetBlockBlobReference(GetBlobName(id));

            var contents = blob.DownloadText();

            return !string.IsNullOrWhiteSpace(contents) 
                    ? Task.FromResult(int.Parse(contents)) 
                    : Task.FromResult(0);
        }

        public Task WriteTotalAsync(string id, int total)
        {
            var blob = container.GetBlockBlobReference(GetBlobName(id));
            blob.UploadText(total.ToString(CultureInfo.InvariantCulture));
            return TaskDone.Done;
        }

        static string GetBlobName(string id)
        {
            return string.Format("topic-{0}.json", id);
        }
    }
}
