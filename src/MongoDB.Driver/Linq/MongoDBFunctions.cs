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

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Used to define extension methods that can be used in LINQ queries against MongoDB.
    /// </summary>
    public sealed class MongoDBFunctions
    {
        // private static fields
        private static readonly MongoDBFunctions _instance = new MongoDBFunctions();

        // constructors
        internal MongoDBFunctions()
        {
        }

        // public static properties
        /// <summary>
        /// Gets the singleton instance of MongoDBFunctions.
        /// </summary>
        public static MongoDBFunctions Instance => _instance;
    }
}
