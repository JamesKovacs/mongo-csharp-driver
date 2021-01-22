/* DELETE ME  - This file contains code from the proof-of-concept project. */

using System;
using System.Runtime.InteropServices;

namespace MongoDB.Driver.Core.Authentication.Libgssapi
{
    class LibgssapiScratchPad
    {
        public static void TestyMcTestStuff()
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

            Marshal.FreeHGlobal(nameBuffer.Value);
        }
    }
}
