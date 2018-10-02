namespace Utilities.ActiveDirectory
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/lmaccess/ns-lmaccess-_user_info_1008
    /// http://www.selfadsi.org/ads-attributes/user-userAccountControl.htm
    /// https://msdn.microsoft.com/en-us/library/cc245515.aspx
    /// </summary>
    public class UserAccess
    {
        public UserAccess() { }

        public UserAccess(ulong value)
        {
            Value = value;
        }

        public ulong Value { get; set; }

        public AccountType AccountType => (AccountType)
            (Value & (UF_NORMAL_ACCOUNT | UF_TEMP_DUPLICATE_ACCOUNT | UF_INTERDOMAIN_TRUST_ACCOUNT | UF_WORKSTATION_TRUST_ACCOUNT | UF_SERVER_TRUST_ACCOUNT));

        public bool IsScript => (Value & UF_SCRIPT) != 0;

        public bool IsDisabled => (Value & UF_ACCOUNTDISABLE) != 0;

        public bool IsHomeDirRequired => (Value & UF_HOMEDIR_REQUIRED) != 0;

        public bool IsLocked => (Value & UF_LOCKOUT) != 0;

        public bool IsPasswordNotRequired => (Value & UF_PASSWD_NOTREQD) != 0;

        public bool IsPasswordChangable => (Value & UF_PASSWD_CANT_CHANGE) == 0;

        public bool IsEncryptedTextPasswordAllowed => (Value & UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED) != 0;

        public bool IsDontExpirePassword => (Value & UF_DONT_EXPIRE_PASSWD) != 0;

        public bool IsSmartCardRequired => (Value & UF_SMARTCARD_REQUIRED) != 0;

        public bool IsTrustedForDelegation => (Value & UF_TRUSTED_FOR_DELEGATION) != 0;

        public bool IsNotDelegated => (Value & UF_NOT_DELEGATED) != 0;

        public bool IsDESKeyOnly => (Value & UF_USE_DES_KEY_ONLY) != 0;

        public bool IsDontRequirePreauth => (Value & UF_DONT_REQUIRE_PREAUTH) != 0;

        public bool IsPasswordExpired => (Value & UF_PASSWORD_EXPIRED) != 0;

        public bool IsTrustedToAuthenticateForDelegation => (Value & UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION) != 0;

        public bool IsNoAuthDataRequired => (Value & UF_NO_AUTH_DATA_REQUIRED) != 0;

        public bool IsPartialSecretsAccount => (Value & UF_PARTIAL_SECRETS_ACCOUNT) != 0; // WorkstationTrust + ReadOnly domain controller

        public bool IsUseAESKeys => (Value & UF_USE_AES_KEYS) != 0;

        private const int UF_SCRIPT = 0x1;
        private const int UF_ACCOUNTDISABLE = 0x2;
        private const int UF_HOMEDIR_REQUIRED = 0x8;
        private const int UF_LOCKOUT = 0x10;
        private const int UF_PASSWD_NOTREQD = 0x20;
        private const int UF_PASSWD_CANT_CHANGE = 0x40;
        private const int UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0x80;
        private const int UF_DONT_EXPIRE_PASSWD = 0x10000;
        private const int UF_SMARTCARD_REQUIRED = 0x40000;
        private const int UF_TRUSTED_FOR_DELEGATION = 0x80000;
        private const int UF_NOT_DELEGATED = 0x100000;
        private const int UF_USE_DES_KEY_ONLY = 0x200000;
        private const int UF_DONT_REQUIRE_PREAUTH = 0x400000;
        private const int UF_PASSWORD_EXPIRED = 0x800000;
        private const int UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x1000000;
        private const ulong UF_NO_AUTH_DATA_REQUIRED = 0x2000000;
        private const ulong UF_PARTIAL_SECRETS_ACCOUNT = 0x4000000;
        private const ulong UF_USE_AES_KEYS = 0x8000000;

        //Account Type
        private const int UF_NORMAL_ACCOUNT = 0x200;
        private const int UF_TEMP_DUPLICATE_ACCOUNT = 0x100;
        private const int UF_INTERDOMAIN_TRUST_ACCOUNT = 0x800;
        private const int UF_WORKSTATION_TRUST_ACCOUNT = 0x1000;
        private const int UF_SERVER_TRUST_ACCOUNT = 0x2000;
        public const int UF_MNS_LOGON_ACCOUNT = 0x20000;
    }
}
