using Unity.Mathematics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FixedInt2Array4
{
    public const int Count = 4;
    public fixed int Data[Count * 2]; // x, y, x, y, ...

    public ref int2 this[int index]
    {
        get
        {
            if ((uint)index >= Count)
                throw new System.IndexOutOfRangeException();

            fixed (int* ptr = Data)
            {
                return ref *(int2*)(ptr + index * 2);
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FixedInt2Array8
{
    public const int Count = 8;
    public fixed int Data[Count * 2]; // x, y, x, y, ...

    public ref int2 this[int index]
    {
        get
        {
            if ((uint)index >= Count)
                throw new System.IndexOutOfRangeException();

            fixed (int* ptr = Data)
            {
                return ref *(int2*)(ptr + index * 2);
            }
        }
    }
}
public enum Direction { Down = 0, Right = 1,  Left = 2,Up = 3 ,UpRight = 4, UpLeft = 5,DownRight = 6,DownLeft = 7}
public static unsafe class Directions
{
    public static readonly FixedInt2Array4 Cardinal;
    public static readonly FixedInt2Array8 All;

    static Directions()
    {
        FixedInt2Array4 cardinal = default;
        cardinal[(int)Direction.Up] = new int2(0, +1); 
        cardinal[(int)Direction.Down] = new int2(0, -1); 
        cardinal[(int)Direction.Right] = new int2(+1, 0); 
        cardinal[(int)Direction.Left] = new int2(-1, 0);  
        Cardinal = cardinal;

        FixedInt2Array8 all = default;
        all[(int)Direction.Up] = new int2(0, +1);   
        all[(int)Direction.Down] = new int2(0, -1);  
        all[(int)Direction.Right] = new int2(+1, 0);   
        all[(int)Direction.Left] = new int2(-1, 0);   
        all[(int)Direction.UpRight] = new int2(+1, +1);   
        all[(int)Direction.UpLeft] = new int2(-1, +1);  
        all[(int)Direction.DownRight] = new int2(+1, -1);   
        all[(int)Direction.DownLeft] = new int2(-1, -1);   
        All = all;
    }
}
