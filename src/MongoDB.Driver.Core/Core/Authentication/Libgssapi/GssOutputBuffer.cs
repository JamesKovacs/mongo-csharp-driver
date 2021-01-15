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
    internal struct GssOutputBuffer : IDisposable
    {
        public ulong length;
        public IntPtr value;
        private bool _isDisposed;

        public byte[] ToByteArray()
        {
            if (length > int.MaxValue)
            {
                throw new InvalidOperationException("GssBuffer too large to convert to array.");
            }

            if (length == 0 || value == IntPtr.Zero)
            {
                throw new ArgumentException($"GssBuffer was unexpectedly empty: {length} / {value}");
            }
            var result = new byte[length];
            Marshal.Copy(value, result, 0, (int)length);
            return result;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                NativeMethods.ReleaseBuffer(out _, this);
                length = 0;
                value = IntPtr.Zero;
                _isDisposed = true;
            }
        }
    }
}
