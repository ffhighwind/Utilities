namespace Utilities.ActiveDirectory
{
    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/cc234251.aspx
    /// </summary>
    public class EffectiveRights
    {
        public int Value { get; set; }

        public EffectiveRights() { }

        public EffectiveRights(int value)
        {
            Value = value;
        }

        public bool IsOwner => (Value & OWNER_SECURITY_INFORMATION) != 0;
        public bool IsGroup => (Value & GROUP_SECURITY_INFORMATION) != 0;
        public bool IsDiscretionaryAccessControlList => (Value & DACL_SECURITY_INFORMATION) != 0;
        public bool IsSystemAccessControlList => (Value & SACL_SECURITY_INFORMATION) != 0;

        private const int OWNER_SECURITY_INFORMATION = 0x00000001;
        private const int GROUP_SECURITY_INFORMATION = 0x00000002;
        private const int DACL_SECURITY_INFORMATION = 0x00000004;
        private const int SACL_SECURITY_INFORMATION = 0x00000008;
    }
}
