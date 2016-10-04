// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client.Transport
{
    using System.Diagnostics;
    using System.Threading.Tasks;

#if !WINDOWS_UWP
    public
#endif
    abstract class TransportHandler : DefaultDelegatingHandler
    {
        protected ITransportSettings TransportSettings;

        protected TransportHandler(IPipelineContext context, ITransportSettings transportSettings)
            : base(context)
        {
            this.TransportSettings = transportSettings;
        }

        public override Task<Message> ReceiveAsync()
        {
            return this.ReceiveAsync(this.TransportSettings.DefaultReceiveTimeout);
        }
    }
}