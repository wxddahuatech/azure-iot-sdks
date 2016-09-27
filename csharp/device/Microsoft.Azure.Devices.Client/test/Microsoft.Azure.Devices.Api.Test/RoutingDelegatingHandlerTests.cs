// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Azure.Devices.Client.Test
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client.Common;
    using Microsoft.Azure.Devices.Client.Exceptions;
    using Microsoft.Azure.Devices.Client.Transport;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;

    [TestClass]
    public class RoutingDelegatingHandlerTests
    {
        [TestMethod]
        [TestCategory("CIT")]
        [TestCategory("DelegatingHandlers")]
        [TestCategory("Owner [mtuchkov]")]
        public async Task TransportRouting_FirstTrySucceed_Open()
        {
            var contextMock = Substitute.For<IPipelineContext>();
            var amqpTransportSettings = Substitute.For<ITransportSettings>();
            var mqttTransportSettings = Substitute.For<ITransportSettings>();
            var innerHandler = Substitute.For<IDelegatingHandler>();
            innerHandler.OpenAsync(Arg.Is(false)).Returns(TaskConstants.Completed);
            contextMock.Get<ITransportSettings[]>().Returns(new[] { amqpTransportSettings, mqttTransportSettings });
            var sut = new ProtocolRoutingDelegatingHandler(contextMock);
            sut.ContinuationFactory = c => innerHandler;

            await sut.OpenAsync(false);
            
            await innerHandler.Received(1).OpenAsync(Arg.Is(false));
        }

        [TestMethod]
        [TestCategory("CIT")]
        [TestCategory("DelegatingHandlers")]
        [TestCategory("Owner [mtuchkov]")]
        public async Task TransportRouting_TryOpenFailedWithUnsupportedException_FailOnFirstTry()
        {
            var contextMock = Substitute.For<IPipelineContext>();
            var amqpTransportSettings = Substitute.For<ITransportSettings>();
            var mqttTransportSettings = Substitute.For<ITransportSettings>();
            var innerHandler = Substitute.For<IDelegatingHandler>();
            innerHandler.CloseAsync().Returns(TaskConstants.Completed);
            int openCallCounter = 0;
            innerHandler.OpenAsync(Arg.Is(false)).Returns(async ci =>
            {
                openCallCounter++;
                await Task.Yield();
                throw new InvalidOperationException();
            });
            contextMock.Get<ITransportSettings[]>().Returns(new[] { amqpTransportSettings, mqttTransportSettings });
            var sut = new ProtocolRoutingDelegatingHandler(contextMock);
            sut.ContinuationFactory = c => innerHandler;

            await sut.OpenAsync(Arg.Is(false)).ExpectedAsync<InvalidOperationException>();

            await innerHandler.Received(1).CloseAsync();

            Assert.AreEqual(1, openCallCounter);
        }

        [TestMethod]
        [TestCategory("CIT")]
        [TestCategory("DelegatingHandlers")]
        [TestCategory("Owner [mtuchkov]")]
        public async Task TransportRouting_TryOpenFailedWithSupportedExceptionTwoTimes_Fail()
        {
            var contextMock = Substitute.For<IPipelineContext>();
            var amqpTransportSettings = Substitute.For<ITransportSettings>();
            var mqttTransportSettings = Substitute.For<ITransportSettings>();
            var innerHandler = Substitute.For<IDelegatingHandler>();
            innerHandler.CloseAsync().Returns(TaskConstants.Completed);
            int openCallCounter = 0;
            innerHandler.OpenAsync(Arg.Is(false)).Returns(async ci =>
            {
                openCallCounter++;
                await Task.Yield();
                throw new TimeoutException();
            });
            contextMock.Get<ITransportSettings[]>().Returns(new[] { amqpTransportSettings, mqttTransportSettings });
            var sut = new ProtocolRoutingDelegatingHandler(contextMock);
            sut.ContinuationFactory = c => innerHandler;
            await sut.OpenAsync(Arg.Is(false)).ExpectedAsync<IotHubCommunicationException>();

            await innerHandler.Received(2).CloseAsync();

            Assert.AreEqual(2, openCallCounter);
        }

        [TestMethod]
        [TestCategory("CIT")]
        [TestCategory("DelegatingHandlers")]
        [TestCategory("Owner [mtuchkov]")]
        public async Task TransportRouting_TryOpenFailedWithSupportedExceptionFirstTimes_SuccessOnSecondTry()
        {
            await TransportRouting_TryOpenFailedWithSupportedExceptionFirstTimes_SuccessOnSecondTry(() => new TimeoutException());
            await TransportRouting_TryOpenFailedWithSupportedExceptionFirstTimes_SuccessOnSecondTry(() => new SocketException(1));
            await TransportRouting_TryOpenFailedWithSupportedExceptionFirstTimes_SuccessOnSecondTry(() => new IotHubCommunicationException(string.Empty));
            await TransportRouting_TryOpenFailedWithSupportedExceptionFirstTimes_SuccessOnSecondTry(() => new AggregateException(new TimeoutException()));
            await TransportRouting_TryOpenFailedWithSupportedExceptionFirstTimes_SuccessOnSecondTry(() => new AggregateException(new SocketException(1)));
            await TransportRouting_TryOpenFailedWithSupportedExceptionFirstTimes_SuccessOnSecondTry(() => new AggregateException(new IotHubCommunicationException(string.Empty)));
        }

        static async Task TransportRouting_TryOpenFailedWithSupportedExceptionFirstTimes_SuccessOnSecondTry(Func<Exception> exceptionFactory)
        {
            var contextMock = Substitute.For<IPipelineContext>();
            var amqpTransportSettings = Substitute.For<ITransportSettings>();
            var mqttTransportSettings = Substitute.For<ITransportSettings>();
            var innerHandler = Substitute.For<IDelegatingHandler>();
            innerHandler.CloseAsync().Returns(TaskConstants.Completed);
            int openCallCounter = 0;
            innerHandler.OpenAsync(Arg.Is(false)).Returns(async ci =>
            {
                openCallCounter++;
                await Task.Yield();
                if (openCallCounter == 1)
                {
                    throw exceptionFactory();
                }
            });
            contextMock.Get<ITransportSettings[]>().Returns(new[] { amqpTransportSettings, mqttTransportSettings });
            var sut = new ProtocolRoutingDelegatingHandler(contextMock);
            sut.ContinuationFactory = c => innerHandler;

            await sut.OpenAsync(Arg.Is(false));

            await innerHandler.Received(1).CloseAsync();

            Assert.AreEqual(2, openCallCounter);
        }
    }
}