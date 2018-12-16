using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Behaviors;
using Orleankka.Meta;

using Orleans.Core;

using IOPath = System.IO.Path;

namespace Example
{
    [Serializable] public class Start    : Command<ICopier> {}
    [Serializable] public class Suspend  : Command<ICopier> {}
    [Serializable] public class Continue : Command<ICopier> {}
    [Serializable] public class Cancel   : Command<ICopier> {}
    [Serializable] public class Reset    : Command<ICopier> {}

    public interface ICopier : IActorGrain {}
    
    public class Copier : ActorGrain
    {
        readonly IStorage<CopierData> data;
        readonly Behavior behavior;

        public Copier(IStorage<CopierData> data)
        {
            this.data = data;

            Receive Durable(Receive next)
            {
                return async message =>
                {
                    var result = await next(message);
                    if (message is Become)
                        await Save();
                    return result;
                };
            }

            Task<object> Active(object x)   => TaskResult.Unhandled;
            Task<object> Inactive(object x) => TaskResult.Unhandled;

            var supervision = new Supervision(this);
            
            var fsm = new StateMachine()
                .State(Active, supervision.On)
                    .Substate(Preparing,   Durable)
                    .Substate(Copying,     Durable, trait: Cancellable)
                    .Substate(Compressing, Durable, trait: Cancellable)                    
                    .Substate(Cleaning,    Durable)
                .State(Inactive, supervision.Off)
                    .Substate(Initial)
                    .Substate(Suspended,   Durable)
                    .Substate(Completed,   Durable, trait: Resetable)
                    .Substate(Canceled,    Durable, trait: Resetable);

            behavior = new Behavior(fsm);
            behavior.Initial(Initial);
        }

        public override Task<object> Receive(object message) 
            => behavior.Receive(message);

        async Task Become(Receive other)
        {
            try
            {
                await behavior.Become(other);
            }
            catch (Exception)
            {
                Activation.DeactivateOnIdle();
                throw;
            }
        }
        
        async Task<object> Initial(object message)
        {
            switch (message)
            {
                case Start _:
                    await Become(Preparing);
                    return Done;
                default: 
                    return Unhandled;
            }
        }

        async Task<object> Preparing(object message)
        {
            switch (message)
            {
                case Become _:
                    await Prepare();
                    return Done;
                default:
                    return Unhandled;
            }

            async Task Prepare()
            {
                var source = SourceFileName();
                var target = TargetFileName();
            
                File.Create(source);
                File.Delete(target);

                await File.AppendAllLinesAsync(source, Enumerable.Range(0, 50000).Select(x => x.ToString()));
            }
        }

        string SourceFileName() => IOPath.Combine(IOPath.GetTempPath(), $"{Id}.txt");
        string TargetFileName() => IOPath.Combine(IOPath.GetTempPath(), $"{Id}-copy.txt");

        Task<object> Copying(object message)
        {
            throw new NotImplementedException();
        }

        Task<object> Compressing(object message)
        {
            throw new NotImplementedException();
        }

        Task<object> Cleaning(object message)
        {
            throw new NotImplementedException();
        }

        Task<object> Completed(object message)
        {
            throw new NotImplementedException();
        }

        Task<object> Canceled(object message)
        {
            throw new NotImplementedException();
        }
        
        Task<object> Suspended(object message)
        {
            throw new NotImplementedException();
        }

        async Task<object> Cancellable(object message)
        {
            switch (message)
            {
                case Cancel _:
                    await Become(Preparing);
                    return Done;
                default: 
                    return Unhandled;
            }
        }
        
        async Task<object> Resetable(object message)
        {
            switch (message)
            {
                case Cancel _:
                    await Become(Preparing);
                    return Done;
                default: 
                    return Unhandled;
            }
        }
    }
}