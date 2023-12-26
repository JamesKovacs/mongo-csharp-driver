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

using System.Reflection;

namespace MongoDB.Driver.Linq.Linq3Implementation.Reflection
{
    internal static class MongoDBFunctionsExtensionsMethod
    {
        // private static fields
        private static readonly MethodInfo __exists;
        private static readonly MethodInfo __isMissing;
        private static readonly MethodInfo __isNullOrMissing;

        // static constructor
        static MongoDBFunctionsExtensionsMethod()
        {
            __exists = ReflectionInfo.Method((MongoDBFunctions functions, object field) => functions.Exists(field));
            __isMissing = ReflectionInfo.Method((MongoDBFunctions functions, object field) => functions.IsMissing(field));
            __isNullOrMissing = ReflectionInfo.Method((MongoDBFunctions functions, object field) => functions.IsNullOrMissing(field));
        }

        // public properties
        public static MethodInfo Exists => __exists;
        public static MethodInfo IsMissing => __isMissing;
        public static MethodInfo IsNullOrMissing => __isNullOrMissing;
    }
}
