/* Copyright 2017-present MongoDB Inc.
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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Operations
{
    internal static class RetryableWriteOperationExecutor
    {
        // public static methods
        public static TResult Execute<TResult>(IRetryableWriteOperation<TResult> operation, IWriteBinding binding, bool retryRequested, CancellationToken cancellationToken)
        {
            using (var context = RetryableWriteContext.Create(binding, retryRequested, cancellationToken))
            {
                return Execute(operation, context, cancellationToken);
            }
        }

        public static TResult Execute<TResult>(IRetryableWriteOperation<TResult> operation, RetryableWriteContext context, CancellationToken cancellationToken)
        {
            Exception originalException = null;
            Exception retryException = null;
            long? transactionNumber = null;
            bool executeNoRetries = false;
            int attempt = 1;

            for (int iteration = 0; iteration < 2; iteration++)
            {
                try
                {
                    if (iteration == 0)
                    {
                        context.InitializeChannel(cancellationToken);
                    }
                    else
                    {
                        context.ReplaceChannelSource(context.Binding.GetWriteChannelSource(cancellationToken));
                        context.ReplaceChannel(context.ChannelSource.GetChannel(cancellationToken));
                    }

                    if (!AreRetriesAllowed(operation, context))
                    {
                        executeNoRetries = true;
                        break;
                    }

                    if (transactionNumber == null)
                    {
                        transactionNumber = context.Binding.Session.AdvanceTransactionNumber();
                    }

                    return operation.ExecuteAttempt(context, attempt++, transactionNumber.Value, cancellationToken);
                }
                catch (Exception ex)
                {
                    RetryabilityHelper.AddRetryableWriteErrorLabelOnCheckoutRetryableWrite(ex);

                    if (RetryabilityHelper.IsRetryableWriteException(ex))
                    {
                        if (originalException == null)
                        {
                            originalException = ex;
                        }
                        else
                        {
                            retryException = ex;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (executeNoRetries)
            {
                return operation.ExecuteAttempt(context, 1, null, cancellationToken);
            }

            var exceptionToThrow = ShouldThrowOriginalException(retryException) ? originalException : retryException;
            throw exceptionToThrow;
        }

        public static async Task<TResult> ExecuteAsync<TResult>(IRetryableWriteOperation<TResult> operation, RetryableWriteContext context, CancellationToken cancellationToken)
        {
            Exception originalException = null;
            Exception retryException = null;
            long? transactionNumber = null;
            bool executeNoRetries = false;
            int attempt = 1;

            for (int iteration = 0; iteration < 2; iteration++)
            {
                try
                {
                    if (iteration == 0)
                    {
                        await context.InitializeChannelAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        context.ReplaceChannelSource(await context.Binding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false));
                        context.ReplaceChannel(await context.ChannelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false));
                    }

                    if (!AreRetriesAllowed(operation, context))
                    {
                        executeNoRetries = true;
                        break;
                    }

                    if (transactionNumber == null)
                    {
                        transactionNumber = context.Binding.Session.AdvanceTransactionNumber();
                    }

                    return await operation.ExecuteAttemptAsync(context, attempt++, transactionNumber.Value, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    RetryabilityHelper.AddRetryableWriteErrorLabelOnCheckoutRetryableWrite(ex);

                    if (RetryabilityHelper.IsRetryableWriteException(ex))
                    {
                        if (originalException == null)
                        {
                            originalException = ex;
                        }
                        else
                        {
                            retryException = ex;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (executeNoRetries)
            {
                return await operation.ExecuteAttemptAsync(context, 1, null, cancellationToken).ConfigureAwait(false);
            }

            var exceptionToThrow = ShouldThrowOriginalException(retryException) ? originalException : retryException;
            throw exceptionToThrow;
        }

        public async static Task<TResult> ExecuteAsync<TResult>(IRetryableWriteOperation<TResult> operation, IWriteBinding binding, bool retryRequested, CancellationToken cancellationToken)
        {
            using (var context = await RetryableWriteContext.CreateAsync(binding, retryRequested, cancellationToken).ConfigureAwait(false))
            {
                return await ExecuteAsync(operation, context, cancellationToken).ConfigureAwait(false);
            }
        }

        // privates static methods
        private static bool AreRetriesAllowed<TResult>(IRetryableWriteOperation<TResult> operation, RetryableWriteContext context)
        {
            return IsOperationAcknowledged(operation) && DoesContextAllowRetries(context);
        }

        private static bool AreRetryableWritesSupported(ConnectionDescription connectionDescription)
        {
            return
                connectionDescription.IsMasterResult.LogicalSessionTimeout != null &&
                connectionDescription.IsMasterResult.ServerType != ServerType.Standalone;
        }

        private static bool DoesContextAllowRetries(RetryableWriteContext context)
        {
            return
                context.RetryRequested &&
                AreRetryableWritesSupported(context.Channel.ConnectionDescription) &&
                context.Binding.Session.Id != null &&
                !context.Binding.Session.IsInTransaction;
        }

        private static bool IsOperationAcknowledged<TResult>(IRetryableWriteOperation<TResult> operation)
        {
            var writeConcern = operation.WriteConcern;
            return
                writeConcern == null || // null means use server default write concern which implies acknowledged
                writeConcern.IsAcknowledged;
        }

        private static bool ShouldThrowOriginalException(Exception retryException) =>
            retryException == null ||
            retryException is MongoException && !(retryException is MongoConnectionException || retryException is MongoPoolPausedException);
    }
}
