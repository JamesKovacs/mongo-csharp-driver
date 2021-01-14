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
using System.Security.Authentication;

namespace MongoDB.Driver.Core.Authentication.Libgssapi
{
    class Program
    {
        public static void Main()
        {
            const string name = "drivers@LDAPTEST.10GEN.CC";
            // const string password = "powerbook17";
            const string serviceName = "mongodb";
            const string target = "ldaptest.10gen.cc";
            var spn = $"{serviceName}@{target}";

            uint minorStatus, majorStatus;

            var nameBuffer = new GssInputBuffer(name);
            majorStatus = NativeMethods.ImportName(out minorStatus, ref nameBuffer, ref Oid.NtUserName, out var gssName);
            Console.WriteLine($"{majorStatus}:{minorStatus} - ptr to imported name: {gssName}");

            GssOutputBuffer outputBuffer;
            Oid outputNameType;
            majorStatus = NativeMethods.DisplayName(out minorStatus, gssName, out outputBuffer, out outputNameType);
            var nameFromGss = Marshal.PtrToStringAnsi(outputBuffer.value);
            Console.WriteLine($"{majorStatus}:{minorStatus} - name from ptr: {nameFromGss}");

            majorStatus = NativeMethods.CanonicalizeName(out minorStatus, gssName, ref Oid.MechKrb5, out var gssCanonicalizedName);
            Console.WriteLine($"{majorStatus}:{minorStatus} - ptr to canonicalized name: {gssCanonicalizedName}");

            majorStatus = NativeMethods.DisplayName(out minorStatus, gssName, out outputBuffer, out outputNameType);
            var canonicalizedNameFromGss = Marshal.PtrToStringAnsi(outputBuffer.value);
            Console.WriteLine($"{majorStatus}:{minorStatus} - name from canonicalized ptr: {canonicalizedNameFromGss}");

            OidSet actualMechanisms;
            uint timeReceived;
            var credentialHandle = IntPtr.Zero;
            majorStatus = NativeMethods.AcquireCredential(out minorStatus, gssCanonicalizedName, uint.MaxValue, IntPtr.Zero, GssCredentialUsage.Initiate, out credentialHandle, out actualMechanisms, out timeReceived);
            Console.WriteLine($"{majorStatus}:{minorStatus} - ptr to TGT via keytab: {credentialHandle}");
            Gss.ThrowIfError(majorStatus, minorStatus);

            // var passwordBuffer = StringToGssBuffer(password);
            // var credentialHandle = IntPtr.Zero;
            // majorStatus = Gss.AcquireCredentialWithPassword(out minorStatus, gssCanonicalizedName, ref passwordBuffer, uint.MaxValue, IntPtr.Zero, GssCredentialUsage.Initiate, out credentialHandle, out actualMechanisms, out timeReceived);
            // Console.WriteLine($"{majorStatus}:{minorStatus} - ptr to TGT via password: {credentialHandle}");

            var spnBuffer = new GssInputBuffer(spn);
            majorStatus = NativeMethods.ImportName(out minorStatus, ref spnBuffer, ref Oid.NtHostBasedService, out var spnName);
            majorStatus = NativeMethods.CanonicalizeName(out minorStatus, spnName, ref Oid.MechKrb5, out var spnCanonicalizedName);
            majorStatus = NativeMethods.DisplayName(out minorStatus, spnCanonicalizedName, out outputBuffer, out outputNameType);
            var canonicalizedSpn = Marshal.PtrToStringAnsi(outputBuffer.value);
            Console.WriteLine($"{majorStatus}:{minorStatus} - canonicalized spn: {canonicalizedSpn}");

            var context = IntPtr.Zero;
            var input = new GssInputBuffer();
            GssOutputBuffer output;
            majorStatus = NativeMethods.InitializeSecurityContext(out minorStatus, credentialHandle, ref context, spnName, IntPtr.Zero, GssFlags.Mutual | GssFlags.Sequence, 0, IntPtr.Zero, ref input, out var _, out output, out var _, out var _);
            Console.WriteLine($"{majorStatus}:{minorStatus} - response from empty challenge: byte[] of size {output.length} and ptr {output.value}");

            Gss.ThrowIfError(majorStatus, minorStatus);

            Marshal.FreeHGlobal(nameBuffer.value);
        }
    }

    internal static class Gss {
        public static void ThrowIfError(uint majorStatus, uint minorStatus)
        {
            var minorErrorMsg = "";
            var majorErrorMsg = "";

            if (majorStatus != 0)
            {
                NativeMethods.DisplayStatus(out _, majorStatus, GssCode.GSS_CODE, ref Oid.NoOid, out uint _, out var outputBuffer);
                majorErrorMsg = Marshal.PtrToStringAnsi(outputBuffer.value);
                NativeMethods.ReleaseBuffer(out _, outputBuffer);
            }

            if (minorStatus != 0)
            {
                NativeMethods.DisplayStatus(out _, minorStatus, GssCode.MECH_CODE, ref Oid.NoOid, out uint _, out var outputBuffer);
                minorErrorMsg = Marshal.PtrToStringAnsi(outputBuffer.value);
                NativeMethods.ReleaseBuffer(out _, outputBuffer);
            }

            if (majorErrorMsg != null || minorErrorMsg != null)
            {
                throw new AuthenticationException($"Libgssapi failure - majorStatus: {majorErrorMsg}; minorStatus: {minorErrorMsg}");
            }
        }
    }

    internal static class NativeMethods
    {
        private const string GSSAPI_LIBRARY = @"libgssapi_krb5";

        [DllImport(GSSAPI_LIBRARY, EntryPoint = "gss_import_name")]
        public static extern uint ImportName(out uint minorStatus, ref GssInputBuffer name, ref Oid nameType, out IntPtr outputName);

        [DllImport(GSSAPI_LIBRARY, EntryPoint = "gss_display_name")]
        public static extern uint DisplayName(out uint minorStatus, IntPtr inputName, out GssOutputBuffer outputBuffer, out Oid outputNameType);

        [DllImport(GSSAPI_LIBRARY, EntryPoint = "gss_canonicalize_name")]
        public static extern uint CanonicalizeName(out uint minorStatus, IntPtr inputName, ref Oid mechType, out IntPtr outputName);

        [DllImport(GSSAPI_LIBRARY, EntryPoint = "gss_release_name")]
        public static extern uint ReleaseName(out uint minorStatus, IntPtr name);

        [DllImport(GSSAPI_LIBRARY, EntryPoint = "gss_acquire_cred_with_password")]
        public static extern uint AcquireCredentialWithPassword(out uint minorStatus, IntPtr name, ref GssInputBuffer password, uint timeRequested, IntPtr desiredMechanisms, GssCredentialUsage credentialUsage, out IntPtr credentialHandle, out OidSet actualMechanisms, out uint timeReceived);

        [DllImport(GSSAPI_LIBRARY, EntryPoint = "gss_acquire_cred")]
        public static extern uint AcquireCredential(out uint minorStatus, IntPtr name, uint timeRequested, IntPtr desiredMechanisms, GssCredentialUsage credentialUsage, out IntPtr credentialHandle, out OidSet actualMechanisms, out uint timeReceived);

        [DllImport(GSSAPI_LIBRARY, EntryPoint = "gss_display_status")]
        public static extern uint DisplayStatus(out uint minorStatus, uint status, GssCode statusType, ref IntPtr mechType, out uint messageContext, out GssOutputBuffer statusString);

        [DllImport(GSSAPI_LIBRARY, EntryPoint = "gss_init_sec_context")]
        public static extern uint InitializeSecurityContext(out uint minorStatus, IntPtr credentialHandle, ref IntPtr context, IntPtr spnName, IntPtr inputMechType, GssFlags requestFlags, uint timeRequested, IntPtr inputChannelBindings, ref GssInputBuffer inputToken, out IntPtr actualMechType, out GssOutputBuffer outputToken, out GssFlags returnedFlags, out uint timeReceived);

        [DllImport(GSSAPI_LIBRARY, EntryPoint = "gss_release_cred")]
        public static extern uint ReleaseCredential(out uint minorStatus, IntPtr credentialHandle);

        [DllImport(GSSAPI_LIBRARY, EntryPoint = "gss_release_buffer")]
        public static extern uint ReleaseBuffer(out uint minorStatus, GssOutputBuffer buffer);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GssInputBuffer : IDisposable
    {
        public ulong length;
        public IntPtr value;

        public GssInputBuffer(string inputString)
        {
            length = (ulong) inputString.Length;
            value = Marshal.StringToHGlobalAnsi(inputString);
        }

        public void Dispose()
        {
            if (value != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(value);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GssOutputBuffer : IDisposable
    {
        public ulong length;
        public IntPtr value;

        public void Dispose()
        {
            NativeMethods.ReleaseBuffer(out _, this);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Oid
    {
        public uint length;
        public IntPtr elements;

        public static IntPtr NoOid = IntPtr.Zero;
        public static Oid NtUserName = Create(0x2a, 0x86, 0x48, 0x86, 0xf7, 0x12, 0x01, 0x02, 0x01, 0x01);
        public static Oid MechKrb5 = Create(0x2a, 0x86, 0x48, 0x86, 0xf7, 0x12, 0x01, 0x02, 0x02);
        public static Oid NtHostBasedService = Create(0x2a, 0x86, 0x48, 0x86, 0xf7, 0x12, 0x01, 0x02, 0x01, 0x04);

        private static Oid Create(params byte[] oidBytes)
        {
            var ntUserNameHandle = GCHandle.Alloc(oidBytes, GCHandleType.Pinned);
            return new Oid {elements = ntUserNameHandle.AddrOfPinnedObject(), length = (uint) oidBytes.Length};
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct OidSet
    {
        public ulong count;
        public IntPtr elements;
    }

    internal enum GssCode
    {
        GSS_CODE = 1,
        MECH_CODE = 2
    }

    internal enum GssCredentialUsage
    {
        Both = 0,
        Initiate = 1,
        Accept = 2
    }

    [Flags]
    internal enum GssFlags
    {
        Mutual = 2,
        Sequence = 8
    }

    internal enum GssStatus : uint
    {
        Complete = 0,
        ContinueNeeded = 1
    }
}
