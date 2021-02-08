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

using System.Runtime.InteropServices;

namespace MongoDB.Driver.Core.Authentication.Libgssapi
{
    internal static class Gss
    {
        public static void ThrowIfError(uint majorStatus, uint minorStatus)
        {
            string majorMessage = null;
            string minorMessage = null;

            if (majorStatus != (uint)GssStatus.GSS_S_COMPLETE && majorStatus != (uint)GssStatus.GSS_S_CONTINUE_NEEDED)
            {
                _ = NativeMethods.DisplayStatus(out _, majorStatus, GssCode.GSS_C_GSS_CODE, in Oid.GSS_C_NO_OID, out uint _, out var outputBuffer);
                majorMessage = Marshal.PtrToStringAnsi(outputBuffer.Value);
                outputBuffer.Dispose();
            }

            if (minorStatus != 0)
            {
                _ = NativeMethods.DisplayStatus(out _, minorStatus, GssCode.GSS_C_MECH_CODE, in Oid.GSS_C_NO_OID, out uint _, out var outputBuffer);
                minorMessage = Marshal.PtrToStringAnsi(outputBuffer.Value);
                outputBuffer.Dispose();
            }

            if (!string.IsNullOrEmpty(majorMessage) || !string.IsNullOrEmpty(minorMessage))
            {
                throw new LibgssapiException(majorMessage, minorMessage);
            }
        }
    }
}
