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
            return TypeId.GetHashCode() ^ Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XRecord);
        }

        public bool Equals(XRecord obj)
        {
            return obj != null && obj.Name == Name && obj.TypeId == TypeId;
        }

        public static bool operator ==(XRecord left, XRecord right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            if (ReferenceEquals(right, null)) return false;

            return left.Equals(right);
        }

        public static bool operator !=(XRecord left, XRecord right)
        {
            return !(left == right);
        }
    }
}