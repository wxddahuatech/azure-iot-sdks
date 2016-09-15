// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client.Common;

#if !WINDOWS_UWP
    public abstract 
#endif
    class DefaultDelegatingHandler : IDelegatingHandler
    {
        static readonly Task<Message> DummyResultObject = Task.FromResult((Message)null);

        int innerHandlerInitializing;
        int innerHandlerInitialized;
        IDelegatingHandler innerHandler;

        protected DefaultDelegatingHandler(IPipelineContext context)
        {
            this.Context = context;
        }

        public IPipelineContext Context { get; protected set; }

        public ContinuationFactory<IDelegatingHandler> ContinuationFactory { get; set; }

        public IDelegatingHandler InnerHandler
        {
            get
            {
                if (Volatile.Read(ref this.innerHandlerInitialized) == 1)
                {
                    this.EnsureInnerHandlerInitialized();
                }
                return Volatile.Read(ref this.innerHandler);
            }
            protected set
            {
                Volatile.Write(ref this.innerHandler, value);
            }
        }

        public virtual Task OpenAsync(bool explicitOpen)
        {
            return this.InnerHandler?.OpenAsync(explicitOpen) ?? TaskConstants.Completed;
        }
        
        public virtual Task CloseAsync()
        {
            if (this.InnerHandler == null)
            {
                return TaskConstants.Completed;
            }
            else
            {
                Task closeTask = this.InnerHandler.CloseAsync();
                closeTask.ContinueWith(t => GC.SuppressFinalize(this), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
                return closeTask;
            }
        }

        public virtual Task<Message> ReceiveAsync()
        {
            return this.InnerHandler?.ReceiveAsync() ?? DummyResultObject;
        }

        public virtual Task<Message> ReceiveAsync(TimeSpan timeout)
        {
            return this.InnerHandler?.ReceiveAsync(timeout) ?? DummyResultObject;
        }

        public virtual Task CompleteAsync(string lockToken)
        {
            return this.InnerHandler?.CompleteAsync(lockToken) ?? TaskConstants.Completed;
        }

        public virtual Task AbandonAsync(string lockToken)
        {
            return this.InnerHandler?.AbandonAsync(lockToken) ?? TaskConstants.Completed;
        }

        public virtual Task RejectAsync(string lockToken)
        {
            return this.InnerHandler?.RejectAsync(lockToken) ?? TaskConstants.Completed;
        }

        public virtual Task SendEventAsync(Message message)
        {
            return this.InnerHandler?.SendEventAsync(message) ?? TaskConstants.Completed;
        }

        public virtual Task SendEventAsync(IEnumerable<Message> messages)
        {
            return this.InnerHandler?.SendEventAsync(messages) ?? TaskConstants.Completed;
        }

        protected virtual void Dispose(bool disposing)
        {
            this.innerHandler?.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);   
            GC.SuppressFinalize(this);
        }

        ~DefaultDelegatingHandler()
        {
            this.Dispose(false);
        }

        void EnsureInnerHandlerInitialized()
        {
            if (Interlocked.CompareExchange(ref this.innerHandlerInitializing, 1, 0) == 0)
            {
                IDelegatingHandler result = this.ContinuationFactory?.Invoke(this.Context);
                Volatile.Write(ref this.innerHandler, result);
                Volatile.Write(ref this.innerHandlerInitialized, 1);
            }

            SpinWait.SpinUntil(() => Volatile.Read(ref this.innerHandlerInitialized) != 1);
        }
    }
}