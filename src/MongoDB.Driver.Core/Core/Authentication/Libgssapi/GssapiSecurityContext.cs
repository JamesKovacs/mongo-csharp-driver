/* Copyright 2010-present MongoDB Inc.
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
using System.Runtime.InteropServices;

namespace MongoDB.Driver.Core.Authentication.Libgssapi
{
    internal class GssapiSecurityContext : SafeHandle, ISecurityContext
    {
        public bool IsInitialized { get; }
        public override bool IsInvalid { get; }

        public GssapiSecurityContext() : base(IntPtr.Zero, true)
        {
        }

        protected override bool ReleaseHandle() => throw new NotImplementedException();

        public byte[] Next(byte[] challenge) => throw new NotImplementedException();

        public byte[] DecryptMessage(int messageLength, byte[] encryptedBytes) => throw new NotImplementedException();

        public byte[] EncryptMessage(byte[] plainTextBytes) => throw new NotImplementedException();
    }
}
