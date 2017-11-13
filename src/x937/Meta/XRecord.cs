using System;

namespace x937.Meta
{
    public class XRecord: IEquatable<XRecord>
    {
        public readonly string Name;
        public readonly string TypeId;

        public XRecord(string name, string typeId)
        {
            Name = name;
            TypeId = typeId;
        }

        public override int GetHashCode()
        {
            return TypeId.GetHashCode() * Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XRecord);
        }

        public bool Equals(XRecord obj)
        {
            return obj != null && obj.Name == Name && obj.TypeId == TypeId;
        }
    }
}