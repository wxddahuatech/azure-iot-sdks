// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.Client
{
#if !WINDOWS_UWP
    public
#endif
    delegate T ContinuationFactory<out T>(IPipelineContext context);

#if !WINDOWS_UWP
    public
#endif
    interface IContinuationProvider<T> where T: class
    {
        ContinuationFactory<T> ContinuationFactory { get; set; }
    }
}