/* Copyright 2021-present MongoDB Inc.
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

namespace MongoDB.Driver.Core.Authentication.Libgssapi
{
    internal sealed class GssapiServicePrincipalName : GssapiSafeHandle
    {
        public static GssapiServicePrincipalName Create(string service, string host, string realm)
        {
            var spn = $"{service}@{host}";
            if (!string.IsNullOrEmpty(realm))
            {
                spn += $"@{realm}";
            }

            uint majorStatus, minorStatus;
            using (var spnBuffer = new GssInputBuffer(spn))
            {
                majorStatus = NativeMethods.ImportName(out minorStatus, spnBuffer, ref Oid.NtHostBasedService, out var spnName);
                Gss.ThrowIfError(majorStatus, minorStatus);
                return new GssapiServicePrincipalName(spnName);
            }
        }

        private GssapiServicePrincipalName(IntPtr spnName)
        {
            SetHandle(spnName);
        }

        protected override bool ReleaseHandle()
        {
            uint majorStatus = NativeMethods.ReleaseName(out uint minorStatus, handle);
            return majorStatus == 0 && minorStatus == 0;
        }
    }
}
