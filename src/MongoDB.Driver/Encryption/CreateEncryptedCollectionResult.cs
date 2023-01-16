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

using MongoDB.Bson;

namespace MongoDB.Driver.Encryption
{
    /// <summary>
    /// Represents the result of a create encrypted collection.
    /// </summary>
    public class CreateEncryptedCollectionResult
    {
        private readonly BsonDocument _encryptedFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateEncryptedCollectionResult"/> class.
        /// </summary>
        /// <param name="encryptedFields">The encrypted fields document.</param>
        public CreateEncryptedCollectionResult(BsonDocument encryptedFields) => _encryptedFields = encryptedFields;

        /// <summary>
        /// The encrypted fields document.
        /// </summary>
        public BsonDocument EncryptedFields => _encryptedFields;
    }
}
