﻿/* Copyright 2010-present MongoDB Inc.
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

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Contains MongoDB specific functions that can be used in LINQ queries.
    /// </summary>
    public static class MongoDBFunctions
    {
        /// <summary>
        /// Tests whether a field is missing.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <returns>True if the field is missing.</returns>
        public static bool IsMissing<TField>(TField field)
        {
            throw new NotSupportedException("This method is not functional. It is only usable in conjunction with MongoDB.");
        }

        /// <summary>
        /// Tests whether a field is null or missing.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="field">The field.</param>
        /// <returns>True if the field is null missing.</returns>
        public static bool IsNullOrMissing<TField>(TField field)
        {
            throw new NotSupportedException("This method is not functional. It is only usable in conjunction with MongoDB.");
        }
    }
}
