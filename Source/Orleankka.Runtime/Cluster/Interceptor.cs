namespace Orleankka.Cluster
{
    public interface IInterceptor
    {
        void Install(IInvocationPipeline pipeline, object properties);
    }

    public abstract class Interceptor<TProperties> : IInterceptor
    {
        void IInterceptor.Install(IInvocationPipeline pipeline, object properties) => 
            Install(pipeline, (TProperties)properties);

        protected abstract void Install(IInvocationPipeline pipeline, TProperties properties);
    }
}