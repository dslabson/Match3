using System;

[Serializable]
public struct IntVector2
{
    public int x;
    public int y;


    public IntVector2(int x = 0, int y = 0)
    {
        this.x = x;
        this.y = y;
    }


    public IntVector2 ExchangeCoordinates()
    {
        return new IntVector2(y, x);
    }


    public override string ToString()
    {
        return string.Format("({0}, {1})", x, y);
    }

    
    public static IntVector2 Zero { get { return new IntVector2(); } }


    public override bool Equals(Object obj)
    {
        return obj is IntVector2 && this == (IntVector2)obj;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }


    public static bool operator ==(IntVector2 a, IntVector2 b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(IntVector2 a, IntVector2 b)
    {
        return !(a == b);
    }

    public static IntVector2 operator +(IntVector2 a, IntVector2 b)
    {
        return new IntVector2(a.x + b.x, a.y + b.y);
    }

    public static IntVector2 operator -(IntVector2 a, IntVector2 b)
    {
        return new IntVector2(a.x - b.x, a.y - b.y);
    }

    public static IntVector2 operator *(IntVector2 a, IntVector2 b)
    {
        return new IntVector2(a.x * b.x, a.y * b.y);
    }

    public static IntVector2 operator *(IntVector2 a, int b)
    {
        return new IntVector2(a.x * b, a.y * b);
    }
}