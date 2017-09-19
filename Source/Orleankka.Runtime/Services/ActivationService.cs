﻿using System;
using System.Linq;

namespace Orleankka.Services
{
    using Core;

    /// <summary>
    /// Manages actor activation lifetime
    /// </summary>
    public interface IActivationService
    {
        /// <summary>
        /// Deactivate this activation of the actro after the current actor method call is completed.
        /// This call will mark this activation of the current actor to be deactivated and removed at the end of the current method.
        /// The next call to this actor will result in a different activation to be used, which typical means a new activation will be created automatically by the runtime.
        /// </summary>
        void DeactivateOnIdle();

        /// <summary>
        /// Delay Deactivation of this activation at least for the specified time duration.
        /// DeactivateOnIdle method would undo / override any current “keep alive” setting,
        /// making this actor immediately available for deactivation.
        /// </summary>
        /// <param name="period">
        /// <para>A positive value means “prevent GC of this activation for that time span”</para> 
        /// <para>A negative value means “unlock, and make this activation available for GC again”</para>
        /// </param>
        void DelayDeactivation(TimeSpan period);
    }

    /// <summary>
    /// Default runtime-bound implementation of <see cref="IActivationService"/>
    /// </summary>
    public class ActivationService : IActivationService
    {
        readonly IActorHost host;

        internal ActivationService(IActorHost host)
        {
            this.host = host;
        }

        void IActivationService.DeactivateOnIdle()
        {
            host.DeactivateOnIdle();
        }

        void IActivationService.DelayDeactivation(TimeSpan period)
        {
            host.DelayDeactivation(period);
        } 
    }
}