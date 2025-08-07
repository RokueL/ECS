


using System;
using Unity.Collections;

public class CellBuffer : CacheOptimizedGrid<Cell>
{
    public CellBuffer(  int width,  int height, Allocator allocator) : base( width, height, allocator)
    {
    }

    // /// <summary>
    // /// 헤딩 값이 외곽인지
    // /// </summary>
    // /// <param name="x"> x 좌표 </param>
    // /// <param name="y"> y 좌표 </param>
    // /// <returns></returns>
    // public bool IsOutskirts(int x, int y)
    // {
    //     if (CheckOutOfIndex(x,y))
    //     {
    //         throw new Exception($"CellBuffer.IsOutskirts에서 인덱스를 벗어남\n" +
    //                             $"인자 X : {x} / 넓이 {width} \n" +
    //                             $"인자 Y : {y} / 높이 {height}"
    //         );
    //     }
    //     else
    //     {
    //         return (x == 0 || x == width - 1 || y == 0 || y == height - 1);
    //     }
    // }

    
}