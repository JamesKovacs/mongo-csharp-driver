﻿/* Copyright 2021-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson.TestHelpers;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Core.Misc
{
    public class SemaphoreSlimSignalableTests
    {
        [Theory]
        [ParameterAttributeData]
        public void SemaphoreSlimSignalable_constructor_should_check_arguments([Values(-2, -1, 1025)]int count)
        {
            var exception = Record.Exception(() => new SemaphoreSlimSignalable(count));

            var e = exception.Should().BeOfType<ArgumentOutOfRangeException>().Subject;
            e.ParamName.Should().Be("count");
            e.Message.Should().StartWith("Value is not between");
        }

        [Theory]
        [ParameterAttributeData]
        public async Task SemaphoreSlimSignalable_wait_should_enter(
            [Values(true, false)] bool async,
            [Values(true, false)] bool isSignaledWait,
            [Values(0, 1, 2)] int initialCount,
            [Values(2, 4)] int threadsCount)
        {
            var semaphore = new SemaphoreSlimSignalable(initialCount);

            var resultsTask = WaitAsync(semaphore, async, isSignaledWait, threadsCount, Timeout.InfiniteTimeSpan);

            for (int i = 0; i < threadsCount - initialCount; i++)
            {
                semaphore.Release();
            }

            var results = await resultsTask;

            Assert(results, SemaphoreSlimSignalable.SemaphoreWaitResult.Entered);
        }

        [Theory]
        [ParameterAttributeData]
        public async Task SemaphoreSlimSignalable_wait_should_timeout(
            [Values(true, false)] bool async,
            [Values(true, false)] bool isSignaledWait,
            [Values(5, 10)]int timeoutMS)
        {
            const int threadsCount = 4;
            var semaphore = new SemaphoreSlimSignalable(0);

            var results = await WaitAsync(semaphore, async, isSignaledWait, threadsCount, TimeSpan.FromMilliseconds(timeoutMS));
            Assert(results, SemaphoreSlimSignalable.SemaphoreWaitResult.TimedOut);
        }

        [Theory]
        [ParameterAttributeData]
        public async Task SemaphoreSlimSignalable_wait_should_signal(
            [Values(true, false)] bool async,
            [Values(true, false)] bool signalBeforeWait)
        {
            const int threadsCount = 4;
            var semaphore = new SemaphoreSlimSignalable(0);

            if (signalBeforeWait)
            {
                semaphore.Signal();
            }
            var waitTask = WaitAsync(semaphore, async, true, threadsCount, Timeout.InfiniteTimeSpan);

            if (!signalBeforeWait)
            {
                semaphore.Signal();
            }

            var results = await waitTask;
            Assert(results, SemaphoreSlimSignalable.SemaphoreWaitResult.Signaled);
        }

        [Theory]
        [ParameterAttributeData]
        public async Task SemaphoreSlimSignalable_wait_should_cancel(
            [Values(true, false)] bool async,
            [Values(true, false)] bool isSignaledWait)
        {
            const int threadsCount = 4;
            var semaphore = new SemaphoreSlimSignalable(0);

            var cancelationTokenSource = new CancellationTokenSource();

            var tasks = CreateWaitTasks(semaphore, async, isSignaledWait, threadsCount, Timeout.InfiniteTimeSpan, cancelationTokenSource.Token);

            cancelationTokenSource.Cancel();

            foreach (var task in tasks)
            {
                var exception = await Record.ExceptionAsync(() => task);

                exception.Should().BeOfType<OperationCanceledException>();
            }
        }

        // private methods
        private Task<SemaphoreSlimSignalable.SemaphoreWaitResult[]> WaitAsync(
            SemaphoreSlimSignalable semaphore,
            bool async,
            bool isSignaledWait,
            int threadsCount,
            TimeSpan timeout,
            CancellationToken cancellationToken = default) =>
            Task.WhenAll(CreateWaitTasks(semaphore, async, isSignaledWait, threadsCount, timeout, cancellationToken));

        private Task<SemaphoreSlimSignalable.SemaphoreWaitResult>[] CreateWaitTasks(
            SemaphoreSlimSignalable semaphore,
            bool async,
            bool isSignaledWait,
            int threadsCount,
            TimeSpan timeout,
            CancellationToken cancellationToken = default) =>
            async ?
                TasksUtils.CreateTasks(threadsCount, _ => isSignaledWait ? semaphore.WaitSignaledAsync(timeout, cancellationToken) : semaphore.WaitAsync(timeout, cancellationToken)) :
                TasksUtils.CreateTasksOnOwnThread(threadsCount, _ => isSignaledWait ? semaphore.WaitSignaled(timeout, cancellationToken) : semaphore.Wait(timeout, cancellationToken));

        private void Assert(SemaphoreSlimSignalable.SemaphoreWaitResult[] actual, SemaphoreSlimSignalable.SemaphoreWaitResult expected) =>
            actual.All(r => r == expected).Should().BeTrue();
    }
}
