// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client
{
    using System;
    using System.Collections.Generic;

#if !WINDOWS_UWP
    public
#endif
    class DeviceClientPipelineBuilder : IDeviceClientPipelineBuilder
    {
        readonly List<ContinuationFactory<IDelegatingHandler>> pipeline = new List<ContinuationFactory<IDelegatingHandler>>();

        public IDeviceClientPipelineBuilder With(ContinuationFactory<IDelegatingHandler> delegatingHandlerCreator)
        {
            this.pipeline.Add(delegatingHandlerCreator);
            return this;
        }

        public IDelegatingHandler Build(IPipelineContext context)
        {
            if (this.pipeline.Count == 0)
            {
                throw new InvalidOperationException("Pipeline is not setup");
            }

            for (int i = 0; i < this.pipeline.Count - 1; i++)
            {
                ContinuationFactory<IDelegatingHandler> current = this.pipeline[i];
                ContinuationFactory<IDelegatingHandler> next = this.pipeline[i + 1];
                this.pipeline[i] = ctx =>
                {
                    IDelegatingHandler delegatingHandler = current(ctx);
                    delegatingHandler.ContinuationFactory = next;
                    return delegatingHandler;
                };
            }

            IDelegatingHandler root = this.pipeline[0](context);
            return root;
        }
    }
}