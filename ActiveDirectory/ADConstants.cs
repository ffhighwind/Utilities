namespace Utilities.ActiveDirectory
{
    /// <summary>
    /// http://www.selfadsi.org/ads-attributes/user-userAccountControl.htm
    /// </summary>
    public static class ADConstants
    {
        public const int UF_SCRIPT = 0x1;
        public const int UF_ACCOUNTDISABLE = 0x2;
        public const int UF_HOMEDIR_REQUIRED = 0x8;
        public const int UF_LOCKOUT = 0x10; // doesn't work, use LockoutTime
        public const int UF_PASSWD_NOTREQD = 0x20;
        public const int UF_PASSWD_CANT_CHANGE = 0x40; // will throw an exception if set
        public const int UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0x80;
        public const int UF_TEMP_DUPLICATE_ACCOUNT = 0x100;
        public const int UF_NORMAL_ACCOUNT = 0x200;
        public const int UF_INTERDOMAIN_TRUST_ACCOUNT = 0x800;
        public const int UF_WORKSTATION_TRUST_ACCOUNT = 0x1000;
        public const int UF_SERVER_TRUST_ACCOUNT = 0x2000;
        public const int UF_DONT_EXPIRE_PASSWD = 0x10000;
        public const int UF_MNS_LOGON_ACCOUNT = 0x20000;
        public const int UF_SMARTCARD_REQUIRED = 0x40000;
        public const int UF_TRUSTED_FOR_DELEGATION = 0x80000;
        public const int UF_NOT_DELEGATED = 0x100000;
        public const int UF_USE_DES_KEY_ONLY = 0x200000;
        public const int UF_DONT_REQUIRE_PREAUTH = 0x400000;
        public const int UF_PASSWORD_EXPIRED = 0x800000; //doesn't work if changed, must set pwdLastSet to -1. To calculate expire time use pwdLastSet + maxPwdAge
        public const int UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x1000000;
        public const ulong UF_NO_AUTH_DATA_REQUIRED = 0x2000000;
        public const ulong UF_PARTIAL_SECRETS_ACCOUNT = 0x4000000;
        public const ulong UF_USE_AES_KEYS = 0x8000000;

        public const ulong USER_MAXSTORAGE_UNLIMITED = ulong.MaxValue; //Max-Storage (User)
    }
}
