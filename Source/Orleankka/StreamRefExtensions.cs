using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans.Streams;

namespace Orleankka
{
    public static class StreamRefExtensions
    {
        public static Task Publish<TItem>(this StreamRef<TItem> stream, TItem item, StreamSequenceToken token = null) => 
            stream.Publish(new NextItem<TItem>(item, token));

        public static Task Publish<TItem>(this StreamRef<TItem> stream, IEnumerable<TItem> batch, StreamSequenceToken token = null) =>
            stream.Publish(new NextItemBatch<TItem>(batch, token));

        public static Task<StreamSubscription<TItem>> Subscribe<TItem>(
            this StreamRef<TItem> stream,
            Action<TItem, StreamSequenceToken> callback,
            StreamSequenceToken token = null) =>
            stream.Subscribe((i, t) =>
            {
                callback(i, t);
                return Task.CompletedTask;
            }, 
            token);
        

        public static Task<StreamSubscription<TItem>> Subscribe<TItem>(
            this StreamRef<TItem> stream,
            Func<TItem, StreamSequenceToken, Task> callback, 
            StreamSequenceToken token = null)
        {
            Task Handler(StreamMessage message) =>
                message is StreamItem<TItem> x
                    ? callback(x.Item, x.Token)
                    : Task.CompletedTask;

            return stream.Subscribe(Handler, new SubscribeReceiveItem(token));
        }

        public static Task<StreamSubscription<TItem>> Subscribe<TItem>(
            this StreamRef<TItem> stream,
            Action<IList<SequentialItem<TItem>>> callback,
            StreamSequenceToken token = null) =>
            stream.Subscribe(b =>
            {
                callback(b);
                return Task.CompletedTask;
            }, 
            token);


        public static Task<StreamSubscription<TItem>> Subscribe<TItem>(
            this StreamRef<TItem> stream,
            Func<IList<SequentialItem<TItem>>, Task> callback,
            StreamSequenceToken token = null)
        {
            Task Handler(StreamMessage message) =>
                message is StreamItemBatch<TItem> x
                    ? callback(x.Items)
                    : Task.CompletedTask;

            return stream.Subscribe(Handler, new SubscribeReceiveBatch(token));
        }
    }
}