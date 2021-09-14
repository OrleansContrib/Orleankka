using System.Threading.Tasks;

namespace Orleankka
{
    public interface IStreamRefMiddleware
    {
        Task Publish<TMessage>(StreamPath path, TMessage message, Receive<TMessage> receiver) where TMessage : PublishMessage;
        Task Receive<TMessage>(StreamPath path, TMessage message, Receive<TMessage> receiver) where TMessage : StreamMessage;
    }

    public abstract class StreamRefMiddleware : IStreamRefMiddleware
    {
        readonly IStreamRefMiddleware next;

        protected StreamRefMiddleware(IStreamRefMiddleware next = null) =>
            this.next = next ?? DefaultStreamRefMiddleware.Instance;

        public virtual Task Publish<TMessage>(StreamPath path, TMessage message, Receive<TMessage> receiver) where TMessage : PublishMessage => 
            next.Publish(path, message, receiver);

        public virtual Task Receive<TMessage>(StreamPath path, TMessage message, Receive<TMessage> receiver) where TMessage : StreamMessage => 
            next.Receive(path, message, receiver);
    }

    public class DefaultStreamRefMiddleware : IStreamRefMiddleware
    {
        public static readonly DefaultStreamRefMiddleware Instance = new DefaultStreamRefMiddleware();

        public Task Publish<TMessage>(StreamPath path, TMessage message, Receive<TMessage> receiver) where TMessage : PublishMessage => 
            receiver(message);

        public Task Receive<TMessage>(StreamPath path, TMessage message, Receive<TMessage> receiver) where TMessage : StreamMessage => 
            receiver(message);
    }
}