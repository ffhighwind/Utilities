namespace Utilities.ActiveDirectory
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/a-instancetype
    /// </summary>
    public class InstanceType
    {
        public int Value { get; set; }

        public InstanceType() { }

        public InstanceType(int value)
        {
            Value = value;
        }

        public bool IsHead => (Value & Head) != 0;

        public bool IsReplica => (Value & Replica) != 0;

        public bool IsWritable => (Value & Writable) != 0;

        public bool IsContextHeld => (Value & Held) != 0;

        public bool IsConstructing => (Value & Constructing) != 0;

        public bool IsDeleting => (Value & Deleting) != 0;

        private const int Head = 0x00000001; // The head of naming context.
        private const int Replica = 0x00000002; // This replica is not instantiated.
        private const int Writable = 0x00000004; // The object is writable on this directory.
        private const int Held = 0x00000008; // The naming context above this one on this directory is held.
        private const int Constructing = 0x00000010; // The naming context is in the process of being constructed for the first time by using replication.
        private const int Deleting = 0x00000020; // The naming context is in the process of being removed from the local DSA.
    }
}
