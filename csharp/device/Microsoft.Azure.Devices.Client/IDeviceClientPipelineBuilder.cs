// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client
{
#if !WINDOWS_UWP
    public
#endif
    interface IDeviceClientPipelineBuilder
    {
        IDeviceClientPipelineBuilder With(ContinuationFactory<IDelegatingHandler> delegatingHandlerCreator);

        IDelegatingHandler Build(IPipelineContext context);
    }
}