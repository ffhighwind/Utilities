namespace Utilities.ActiveDirectory
{
    public class SystemFlags
    {
        public SystemFlags() { }

        public SystemFlags(int value)
        {
            Value = value;
        }

        public int Value { get; set; }

        public bool IsAttributeNotReplicated => (Value & NotReplicated) != 0;

        public bool IsCrossRefInNTDS => (Value & NotReplicated) != 0;

        public bool IsAttributeReplicated => (Value & Replicated) != 0;

        public bool IsCrossRefDomain => (Value & NotReplicated) != 0;

        public bool IsBaseSchema => (Value & _IsBaseSchema) != 0;

        public bool IsDeletedImmediately => (Value & _IsDeletedImmediately) != 0;

        public bool IsNotMovable => (Value & _IsNotMovable) != 0;

        public bool IsNotRenamable => (Value & _IsNotRenamable) != 0;

        public bool IsMovementRestricted => (Value & _IsMovementRestricted) != 0;

        public bool IsConfigurationMovable => (Value & _IsConfigurationMovable) != 0;

        public bool IsConfigurationRenamable => (Value & _IsConfigurationRenamable) != 0;

        public bool IsNotDeletable => (Value & _IsNotDeletable) != 0;

        private const int NotReplicated = 0x00000001; // When applied to an attribute, the attribute will not be replicated. When applied to a Cross-Ref object, the naming context is in NTDS.
        private const int Replicated = 0x00000002; // When applied to an attribute, the attribute will be replicated to the global catalog. When applied to a Cross-Ref object, the naming context is a domain.
        private const int Constructed = 0x00000004; // When applied to an attribute, the attribute is constructed.
        private const int _IsBaseSchema = 0x00000010;	// When set, indicates the object is a category 1 object. A category 1 object is a class or attribute that is included in the base schema included with the system.
        private const int _IsDeletedImmediately = 0x02000000; // The object is not moved to the Deleted Objects container when it is deleted. It will be deleted immediately.
        private const int _IsNotMovable = 0x04000000; // The object cannot be moved.
        private const int _IsNotRenamable = 0x08000000; // The object cannot be renamed.
        private const int _IsMovementRestricted = 0x10000000; // For objects in the configuration partition, if this flag is set, the object can be moved with restrictions; otherwise, the object cannot be moved. By default, this flag is not set on new objects created under the configuration partition. This flag can only be set during object creation.
        private const int _IsConfigurationMovable = 0x20000000; // For objects in the configuration partition, if this flag is set, the object can be moved; otherwise, the object cannot be moved. By default, this flag is not set on new objects created under the configuration partition. This flag can only be set during object creation.
        private const int _IsConfigurationRenamable = 0x40000000; // For objects in the configuration partition, if this flag is set, the object can be renamed; otherwise, the object cannot be renamed. By default, this flag is not set on new objects created under the configuration partition. This flag can only be set during object creation.
        private const uint _IsNotDeletable = 0x80000000; // The object cannot be deleted.
    }
}
