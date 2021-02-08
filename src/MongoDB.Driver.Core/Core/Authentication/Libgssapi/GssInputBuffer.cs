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
    internal sealed class GssInputBuffer : IDisposable
    {
        private  ulong length;
        private IntPtr value;

        public GssInputBuffer(string inputString)
        {
            length = (ulong)inputString.Length;
            value = Marshal.StringToHGlobalAnsi(inputString);
        }

        public GssInputBuffer(byte[] inputBytes)
        {
            if (inputBytes == null)
            {
                length = 0;
                value = default;
                return;
            }

            int numBytes = inputBytes.Length;
            var unmanagedArray = Marshal.AllocHGlobal(numBytes);
            Marshal.Copy(inputBytes, 0, unmanagedArray, numBytes);

            length = (ulong)numBytes;
            value = unmanagedArray;
        }

        ~GssInputBuffer()
        {
            InnerDispose();
        }

        public void Dispose()
        {
            InnerDispose();
            GC.SuppressFinalize(this);
        }

        private void InnerDispose()
        {
            if (value != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(value);
                length = 0;
                value = IntPtr.Zero;
            }
        }
    }
}
