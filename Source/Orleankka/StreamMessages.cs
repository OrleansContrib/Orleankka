using System;
using System.Collections.Generic;

using Orleans.Streams;

namespace Orleankka
{
    using Utility;

    /// <summary>
    /// Base interface for all kinds of <see cref="StreamRef{TItem}.Subscribe{TOptions}" /> options
    /// </summary>
    public interface SubscribeOptions {}

    /// <summary>
    /// Represents consumer will to receive items one-by-one.
    /// </summary>
    public struct SubscribeReceiveItem : SubscribeOptions, IEquatable<SubscribeReceiveItem>
    {
        /// <summary>
        /// The optional stream filter
        /// </summary>
        public readonly StreamFilter Filter;

        /// <summary>
        /// The optional stream sequence token
        /// </summary>
        public readonly StreamSequenceToken Token;

        /// <summary>
        /// Creates new instance of <see cref="SubscribeReceiveItem"/>
        /// </summary>
        /// <param name="filter">The optional stream filter to use for filtering stream items</param>
        /// <param name="token">The optional stream sequence to be used as an offset to start the subscription from</param>
        public SubscribeReceiveItem(StreamFilter filter = null, StreamSequenceToken token = null)
        {
            Filter = filter;
            Token = token;
        }

        public bool Equals(SubscribeReceiveItem other) => Equals(Filter, other.Filter) && Equals(Token, other.Token);
        public override bool Equals(object obj) => obj is SubscribeReceiveItem other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Filter != null
                    ? Filter.GetHashCode()
                    : 0) * 397) ^ (Token != null
                    ? Token.GetHashCode()
                    : 0);
            }
        }

        public static bool operator ==(SubscribeReceiveItem left, SubscribeReceiveItem right) => left.Equals(right);
        public static bool operator !=(SubscribeReceiveItem left, SubscribeReceiveItem right) => !left.Equals(right);
    }

    /// <summary>
    /// Represents consumer will to receive items in batches.
    /// </summary>
    /// <remarks>Requires support from underlying stream provider</remarks>
    public struct SubscribeReceiveBatch : SubscribeOptions, IEquatable<SubscribeReceiveBatch>
    {
        /// <summary>
        /// The optional stream sequence token
        /// </summary>
        public readonly StreamSequenceToken Token;
        
        /// <summary>
        /// Creates new instance of <see cref="SubscribeReceiveBatch"/>
        /// </summary>
        /// <param name="token">The optional stream sequence to be used as an offset to start the subscription from</param>
        public SubscribeReceiveBatch(StreamSequenceToken token = null) => 
            Token = token;

        public bool Equals(SubscribeReceiveBatch other) => Equals(Token, other.Token);
        public override bool Equals(object obj) => obj is SubscribeReceiveBatch other && Equals(other);

        public override int GetHashCode()
        {
            return (Token != null
                ? Token.GetHashCode()
                : 0);
        }

        public static bool operator ==(SubscribeReceiveBatch left, SubscribeReceiveBatch right) => left.Equals(right);
        public static bool operator !=(SubscribeReceiveBatch left, SubscribeReceiveBatch right) => !left.Equals(right);
    }

    /// <summary>
    /// Base interface for all kinds of <see cref="StreamSubscription{TItem}.Resume{TOptions}" /> options
    /// </summary>
    public interface ResumeOptions {}

    /// <summary>
    /// Represents consumer will to receive items one-by-one.
    /// </summary>
    public struct ResumeReceiveItem : ResumeOptions
    {
        /// <summary>
        /// The optional stream sequence token
        /// </summary>
        public readonly StreamSequenceToken Token;

        /// <summary>
        /// Creates new instance of <see cref="ResumeReceiveItem"/>
        /// </summary>
        /// <param name="token">The optional stream sequence to be used as an offset to resume subscription from</param>
        public ResumeReceiveItem(StreamSequenceToken token = null) => 
            Token = token;
    }

    /// <summary>
    /// Represents consumer will to receive items in batches.
    /// </summary>
    /// <remarks>Requires support from underlying stream provider</remarks>
    public struct ResumeReceiveBatch : ResumeOptions
    {
        /// <summary>
        /// The optional stream sequence token
        /// </summary>
        public readonly StreamSequenceToken Token;

        /// <summary>
        /// Creates new instance of <see cref="ResumeReceiveBatch"/>
        /// </summary>
        /// <param name="token">The optional stream sequence to be used as an offset to resume subscription from</param>
        public ResumeReceiveBatch(StreamSequenceToken token = null) => 
            Token = token;
    }

    /// <summary>
    /// Represents the base for all message types that could be published to a stream
    /// </summary>
    public interface PublishMessage
    {}

    /// <summary>
    /// Represents the next item <typeparamref name="T"/> to be published to a stream
    /// with optional <see cref="StreamSequenceToken"/>
    /// </summary>
    public struct NextItem<T> : PublishMessage, IEquatable<NextItem<T>>
    {
        /// <summary>
        /// The item
        /// </summary>
        public readonly T Item;

        /// <summary>
        /// The optional sequence token
        /// </summary>
        public readonly StreamSequenceToken Token;

        /// <summary>
        /// Creates new instance of <see cref="NextItem{T}"/>
        /// </summary>
        /// <param name="item">The item to be published</param>
        /// <param name="token">The sequence token</param>
        public NextItem(T item, StreamSequenceToken token = null)
        {
            Item = item;
            Token = token;
        }

        public bool Equals(NextItem<T> other) => EqualityComparer<T>.Default.Equals(Item, other.Item) && Equals(Token, other.Token);
        public override bool Equals(object obj) => obj is NextItem<T> other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Item) * 397) ^ (Token != null
                    ? Token.GetHashCode()
                    : 0);
            }
        }

        public static bool operator ==(NextItem<T> left, NextItem<T> right) => left.Equals(right);
        public static bool operator !=(NextItem<T> left, NextItem<T> right) => !left.Equals(right);
    }

    /// <summary>
    /// Represents the next batch of items <typeparamref name="T"/> to be published to a stream
    /// with optional <see cref="StreamSequenceToken"/>
    /// </summary>
    public struct NextItemBatch<T> : PublishMessage, IEquatable<NextItemBatch<T>>
    {
        public readonly IEnumerable<T> Items;
        public readonly StreamSequenceToken Token;

        public NextItemBatch(IEnumerable<T> items, StreamSequenceToken token = null)
        {
            Items = items;
            Token = token;
        }

        public bool Equals(NextItemBatch<T> other) => Equals(Items, other.Items) && Equals(Token, other.Token);
        public override bool Equals(object obj) => obj is NextItemBatch<T> other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Items != null
                    ? Items.GetHashCode()
                    : 0) * 397) ^ (Token != null
                    ? Token.GetHashCode()
                    : 0);
            }
        }

        public static bool operator ==(NextItemBatch<T> left, NextItemBatch<T> right) => left.Equals(right);
        public static bool operator !=(NextItemBatch<T> left, NextItemBatch<T> right) => !left.Equals(right);
    }

    /// <summary>
    /// Represents the publisher-side error.
    /// </summary>
    /// <remarks>Will invalidate all existing stream subscriptions</remarks>
    public struct NotifyStreamError : PublishMessage, IEquatable<NotifyStreamError>
    {
        public readonly Exception Exception;

        public NotifyStreamError(Exception exception)
        {
            Requires.NotNull(exception, nameof(exception));
            Exception = exception;
        }

        public bool Equals(NotifyStreamError other) => Equals(Exception, other.Exception);
        public override bool Equals(object obj) => obj is NotifyStreamError other && Equals(other);

        public override int GetHashCode()
        {
            return (Exception != null
                ? Exception.GetHashCode()
                : 0);
        }

        public static bool operator ==(NotifyStreamError left, NotifyStreamError right) => left.Equals(right);
        public static bool operator !=(NotifyStreamError left, NotifyStreamError right) => !left.Equals(right);
    }

    /// <summary>
    /// Represents the completion of a stream.
    /// </summary>
    /// <remarks>Will invalidate all existing stream subscriptions</remarks>
    public struct NotifyStreamCompleted : PublishMessage
    {}

    /// <summary>
    /// Represents the base for all message types that could be received from a stream
    /// </summary>
    public interface StreamMessage
    {
        StreamPath Path { get; }
    }

    public abstract class StreamMessage<TItem> : StreamMessage
    {
        public StreamPath Path => Stream.Path;
        public StreamRef<TItem> Stream { get; }

        protected StreamMessage(StreamRef<TItem> stream)
        {
            Requires.NotNull(stream, nameof(stream));
            Stream = stream;
        }

    }

    public class StreamItem<TItem> : StreamMessage<TItem>
    {
        public TItem Item { get; }
        public StreamSequenceToken Token { get; }

        public StreamItem(StreamRef<TItem> stream, TItem item, StreamSequenceToken token = null)
            : base(stream)
        {
            Item = item;
            Token = token;
        }
    }

    public class StreamItemBatch<TItem> : StreamMessage<TItem>
    {
        public IList<SequentialItem<TItem>> Items { get; }

        public StreamItemBatch(StreamRef<TItem> stream, IList<SequentialItem<TItem>> items)
            : base(stream)
        {
            // ReSharper disable PossibleMultipleEnumeration
            Requires.NotNull(items, nameof(items));
            Items = items;
            // ReSharper restore PossibleMultipleEnumeration
        }
    }

    public class StreamError : StreamMessage
    {
        public StreamPath Path { get; }
        public Exception Exception { get; }

        public StreamError(StreamPath path, Exception exception)
        {
            Requires.NotNull(exception, nameof(exception));
            if (path == StreamPath.Empty)
                throw new InvalidOperationException("The stream path is empty");

            Path = path;
            Exception = exception;
        }
    }

    public class StreamCompleted : StreamMessage
    {
        public StreamPath Path { get; }

        public StreamCompleted(StreamPath path)
        {
            if (path == StreamPath.Empty)
                throw new InvalidOperationException("The stream path is empty");
            Path = path;
        }
    }
}