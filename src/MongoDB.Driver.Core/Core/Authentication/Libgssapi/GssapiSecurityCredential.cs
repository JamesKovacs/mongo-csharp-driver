using System;
using System.Runtime.InteropServices;
using System.Security;

namespace MongoDB.Driver.Core.Authentication.Libgssapi
{
    /// <summary>
    /// A wrapper around the Libgssapi structure specifically used as a credential handle.
    /// </summary>
    public class GssapiSecurityCredential : SafeHandle
    {
        /// <summary>
        /// Acquires the TGT from the KDC.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static GssapiSecurityCredential Acquire(string username, SecureString password)
        {
            uint minorStatus, majorStatus;
            GssInputBuffer nameBuffer;

            IntPtr gssName = IntPtr.Zero;
            IntPtr gssCanonicalizedName = IntPtr.Zero;
            try
            {
                using (nameBuffer = new GssInputBuffer(username))
                {
                    majorStatus = NativeMethods.ImportName(out minorStatus, ref nameBuffer, ref Oid.NtUserName, out gssName);
                    Gss.ThrowIfError(majorStatus, minorStatus);

                    majorStatus = NativeMethods.CanonicalizeName(out minorStatus, gssName, ref Oid.MechKrb5, out gssCanonicalizedName);
                    Gss.ThrowIfError(majorStatus, minorStatus);

                    majorStatus = NativeMethods.AcquireCredential(out minorStatus, gssCanonicalizedName, uint.MaxValue, IntPtr.Zero, GssCredentialUsage.Initiate, out IntPtr credentialHandle, out OidSet actualMechanisms, out uint timeReceived);
                    Gss.ThrowIfError(majorStatus, minorStatus);
                    return new GssapiSecurityCredential(credentialHandle);
                }
            }
            finally
            {
                if (gssName != IntPtr.Zero)
                {
                    NativeMethods.ReleaseName(out uint _, gssName);
                }
                if (gssCanonicalizedName != IntPtr.Zero)
                {
                    NativeMethods.ReleaseName(out uint _, gssCanonicalizedName);
                }
            }
        }

        private GssapiSecurityCredential(IntPtr credentialHandle) : base(credentialHandle, true)
        {
        }

        /// <inheritdoc />
        protected override bool ReleaseHandle()
        {
            if (base.handle != IntPtr.Zero)
            {
                NativeMethods.ReleaseCredential(out uint _, base.handle);
            }

            return true;
        }

        /// <inheritdoc />
        public override bool IsInvalid
        {
            get { return base.IsClosed || base.handle == IntPtr.Zero; }
        }
    }
}
