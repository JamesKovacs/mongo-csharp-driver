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
using System.Runtime.InteropServices;

namespace MongoDB.Driver.Core.Authentication.Libgssapi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GssOutputBuffer
    {
        public ulong length;
        public IntPtr value;

        public byte[] ToByteArray()
        {
            if (length > int.MaxValue)
            {
                throw new InvalidOperationException("GssBuffer too large to convert to array.");
            }

            var result = new byte[length];
            if (length > 0)
            {
                Marshal.Copy(value, result, 0, (int)length);
            }
            return result;
        }

        public void Dispose()
        {
            if (value != IntPtr.Zero)
            {
                NativeMethods.ReleaseBuffer(out _, ref this);
            }
        }
    }
}
