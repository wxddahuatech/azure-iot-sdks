// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

#if !WINDOWS_UWP
    public
#endif
    interface IDelegatingHandler : IContinuationProvider<IDelegatingHandler>, IDisposable
    {
        Task AbandonAsync(string lockToken, CancellationToken cancellationToken);
        Task CloseAsync();
        Task CompleteAsync(string lockToken, CancellationToken cancellationToken);
        Task OpenAsync(bool explicitOpen, CancellationToken cancellationToken);
        Task<Message> ReceiveAsync(CancellationToken cancellationToken);
        Task<Message> ReceiveAsync(TimeSpan timeout, CancellationToken cancellationToken);
        Task RejectAsync(string lockToken, CancellationToken cancellationToken);
#if WINDOWS_UWP
        [Windows.Foundation.Metadata.DefaultOverload]
#endif
        Task SendEventAsync(Message message, CancellationToken cancellationToken);
        Task SendEventAsync(IEnumerable<Message> messages, CancellationToken cancellationToken);
    }
}