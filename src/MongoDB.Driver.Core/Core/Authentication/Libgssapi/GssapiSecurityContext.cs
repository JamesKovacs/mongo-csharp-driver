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
using MongoDB.Driver.Core.Authentication.Sspi;

namespace MongoDB.Driver.Core.Authentication.Libgssapi
{
    internal class GssapiSecurityContext : SafeHandle, ISecurityContext
    {
        private readonly string _servicePrincipalName;
        private SspiSecurityCredential _credential;
        private bool _isDisposed;

        public bool IsInitialized { get; private set; }

        public override bool IsInvalid
        {
            get { return base.IsClosed || handle == IntPtr.Zero; }
        }

        public GssapiSecurityContext(string servicePrincipalName, SspiSecurityCredential credential) : base(IntPtr.Zero, true)
        {
            _servicePrincipalName = servicePrincipalName;
            _credential = credential;
        }

        protected override bool ReleaseHandle()
        {
            var majorStatus = NativeMethods.ReleaseCredential(out var minorStatus, handle);
            return majorStatus == 0 && minorStatus == 0;
        }

        public byte[] Next(byte[] challenge)
        {
            GssOutputBuffer outputToken = new GssOutputBuffer();
            try
            {
                GssInputBuffer spnBuffer, inputToken;
                using (spnBuffer = new GssInputBuffer(_servicePrincipalName))
                using (inputToken = new GssInputBuffer(challenge))
                {
                    uint majorStatus, minorStatus;

                    majorStatus = NativeMethods.ImportName(out minorStatus, ref spnBuffer, ref Oid.NtHostBasedService, out var spnName);
                    Gss.ThrowIfError(majorStatus, minorStatus);
                    majorStatus = NativeMethods.CanonicalizeName(out minorStatus, spnName, ref Oid.MechKrb5, out var spnCanonicalizedName);
                    Gss.ThrowIfError(majorStatus, minorStatus);

                    var context = IntPtr.Zero;
                    const GssFlags authenticationFlags = GssFlags.Mutual | GssFlags.Sequence;
                    majorStatus = NativeMethods.InitializeSecurityContext(out minorStatus, handle, ref context, spnCanonicalizedName, IntPtr.Zero, authenticationFlags, 0, IntPtr.Zero, ref inputToken, out var _, out outputToken, out var _, out var _);
                    Gss.ThrowIfError(majorStatus, minorStatus);

                    var output = outputToken.ToByteArray();
                    IsInitialized = true;
                    return output;
                }
            }
            finally
            {
                outputToken.Dispose();
            }
        }

        public byte[] DecryptMessage(int messageLength, byte[] encryptedBytes) => throw new NotImplementedException();

        public byte[] EncryptMessage(byte[] plainTextBytes) => throw new NotImplementedException();

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                _credential?.Dispose();
                _credential = null;
                _isDisposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
