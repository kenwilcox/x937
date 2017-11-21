using System;

namespace x937.Meta
{
    public struct Range: IEquatable<Range>
    {
        public readonly int Start;
        public readonly int End;

        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }

        public override bool Equals(object obj)
        {
            return Equals((Range)obj);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode();
        }

        public bool Equals(Range obj)
        {
            return obj.Start == Start && obj.End == End;
        }

        public static bool operator ==(Range left, Range right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Range left, Range right)
        {
            return !(left == right);
        }
    }
}