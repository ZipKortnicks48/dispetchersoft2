using System;
using System.Security.Permissions;
using System.Security.Principal;
using System.Runtime.InteropServices;

[assembly: SecurityPermissionAttribute(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: PermissionSetAttribute(SecurityAction.RequestMinimum, Name = "FullTrust")]
namespace FinMonMvkLoader
{
    public class WindowsIdentityEx : IDisposable, IIdentity
    {
        #region Unmanaged Code
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
        int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
        [DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private unsafe static extern int FormatMessage(int dwFlags, ref IntPtr lpSource,
        int dwMessageId, int dwLanguageId, ref String lpBuffer, int nSize, IntPtr* Arguments);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle,
        int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        public unsafe static string GetErrorMessage(int errorCode)
        {
            int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
            int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
            int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
            int messageSize = 255;
            String lpMsgBuf = "";
            int dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;
            IntPtr ptrlpSource = IntPtr.Zero;
            IntPtr prtArguments = IntPtr.Zero;
            int retVal = FormatMessage(dwFlags, ref ptrlpSource, errorCode, 0, ref lpMsgBuf, messageSize, &prtArguments);
            if (0 == retVal)
            {
                throw new Exception("Failed to format message for error code " + errorCode + ". ");
            }
            return lpMsgBuf;
        }
        #endregion
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        private const int SecurityImpersonation = 2;

        private bool _disposed = false;
        private IntPtr _tokenHandle = new IntPtr(0);
        private IntPtr _dupeTokenHandle = new IntPtr(0);
        private string _userName = string.Empty;
        private string _domainName = string.Empty;
        private string _password = string.Empty;
        private WindowsIdentity _windowsIdentity = null;
        private void Init()
        {
            _tokenHandle = IntPtr.Zero;
            _dupeTokenHandle = IntPtr.Zero;
        }
        public WindowsIdentityEx(string userName, string domainName, string password)
        {
            this.Init();
            bool returnValue = LogonUser(userName, domainName, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref _tokenHandle);
            if (false == returnValue)
            {
                int ret = Marshal.GetLastWin32Error();
                int errorCode = 0x5; //ERROR_ACCESS_DENIED
                throw new System.ComponentModel.Win32Exception(errorCode, string.Format("\nError: [{0}] {1}\n", ret, GetErrorMessage(ret)));
            }
            bool retVal = DuplicateToken(_tokenHandle, SecurityImpersonation, ref _dupeTokenHandle);
            if (false == retVal)
            {
                CloseHandle(_tokenHandle);
                throw new Exception("Unable to duplicate token");
            }
            _windowsIdentity = new WindowsIdentity(_dupeTokenHandle);
        }
        public WindowsIdentityEx(IntPtr userToken)
        {
            this.Init();
            _windowsIdentity = new WindowsIdentity(userToken);
        }
        public WindowsIdentityEx(string sUserPrincipalName)
        {
            this.Init();
            _windowsIdentity = new WindowsIdentity(sUserPrincipalName);
        }
        public WindowsIdentityEx(IntPtr userToken, string type)
        {
            this.Init();
            _windowsIdentity = new WindowsIdentity(type);
        }
        public WindowsIdentityEx(string sUserPrincipalName, string type)
        {
            this.Init();
            _windowsIdentity = new WindowsIdentity(sUserPrincipalName, type);
        }
        public WindowsIdentityEx(IntPtr userToken, string type, WindowsAccountType acctType)
        {
            this.Init();
            _windowsIdentity = new WindowsIdentity(userToken, type, acctType);
        }
        public WindowsIdentityEx(IntPtr userToken, string type, WindowsAccountType acctType, bool isAuthenticated)
        {
            this.Init();
            _windowsIdentity = new WindowsIdentity(userToken, type, acctType, isAuthenticated);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing && _windowsIdentity != null)
                {
                    _windowsIdentity.Dispose();
                }
                if (_tokenHandle != IntPtr.Zero)
                {
                    CloseHandle(_tokenHandle);
                    _tokenHandle = IntPtr.Zero;
                }
                if (_dupeTokenHandle != IntPtr.Zero)
                {
                    CloseHandle(_dupeTokenHandle);
                    _dupeTokenHandle = IntPtr.Zero;
                }
            }
            _disposed = true;
        }
        ~WindowsIdentityEx()
        {
            Dispose(false);
        }
        string IIdentity.AuthenticationType
        {
            get { return _windowsIdentity.AuthenticationType; }
        }
        bool IIdentity.IsAuthenticated
        {
            get { return _windowsIdentity.IsAuthenticated; }
        }
        string IIdentity.Name
        {
            get { return _windowsIdentity.Name; }
        }
        public TokenImpersonationLevel ImpersonationLevel
        {
            get { return _windowsIdentity.ImpersonationLevel; }
        }
        public virtual bool IsAnonymous
        {
            get { return _windowsIdentity.IsAnonymous; }
        }
        public virtual bool IsGuest
        {
            get { return _windowsIdentity.IsGuest; }
        }
        public virtual bool IsSystem
        {
            get { return _windowsIdentity.IsSystem; }
        }
        public SecurityIdentifier Owner
        {
            get { return _windowsIdentity.Owner; }
        }
        public virtual IntPtr Token
        {
            get { return _windowsIdentity.Token; }
        }
        public SecurityIdentifier User
        {
            get { return _windowsIdentity.User; }
        }
        public WindowsIdentity WindowsIdentity
        {
            get { return _windowsIdentity; }
        }
        public virtual WindowsImpersonationContext Impersonate()
        {
            return _windowsIdentity.Impersonate();
        }
        public static WindowsImpersonationContext Impersonate(IntPtr userToken)
        {
            return WindowsIdentity.Impersonate(userToken);
        }
        public static WindowsIdentity GetAnonymous()
        {
            return WindowsIdentity.GetAnonymous();
        }
        public static WindowsIdentity GetCurrent()
        {
            return WindowsIdentity.GetCurrent();
        }
        public static WindowsIdentity GetCurrent(bool ifImpersonating)
        {
            return WindowsIdentity.GetCurrent(ifImpersonating);
        }
        public static WindowsIdentity GetCurrent(TokenAccessLevels desiredAccess)
        {
            return WindowsIdentity.GetCurrent(desiredAccess);
        }
    }
}