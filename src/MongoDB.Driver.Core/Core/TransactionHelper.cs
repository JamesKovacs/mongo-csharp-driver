/* Copyright 2019–present MongoDB Inc.
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
using MongoDB.Driver.Core.Bindings;

namespace MongoDB.Driver.Core
{
    internal static class TransactionHelper
    {
#pragma warning disable CS0618 // Type or member is obsolete
        internal static void UnpinServerIfNeededOnCommandException(ICoreSession session, Exception exception)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            if (session.IsInTransaction && ShouldUnpinServerOnCommandException(exception))
            {
                session.CurrentTransaction.UnpinAll();
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        internal static void UnpinServerIfNeededOnRetryableCommitException(CoreTransaction transaction, Exception exception)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            if (ShouldUnpinServerOnRetryableCommitException(exception))
            {
                transaction.UnpinAll();
            }
        }

        private static bool ShouldUnpinServerOnCommandException(Exception exception)
        {
            return
                exception is MongoException mongoException &&
                (mongoException.HasErrorLabel("TransientTransactionError") ||
                 mongoException.HasErrorLabel("UnknownTransactionCommitResult"));
        }

        private static bool ShouldUnpinServerOnRetryableCommitException(Exception exception)
        {
            return
                exception is MongoException mongoException &&
                mongoException.HasErrorLabel("UnknownTransactionCommitResult");
        }
    }
}
