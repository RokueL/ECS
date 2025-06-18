using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// 2차원 배열이 아닌 1차워의 연속된 메모리할당으로 캐시 최적화 
/// </summary>
/// <typeparam name="T"> 제네릭~~ </typeparam>
///
/// 기본 원리는 간단함 2차원 배열을 스퀘어라 했을때 밑에서부터 오른쪽 한층식 인덱스 ++
/// MaxX = 5, MaxY = 7 일경우 
/// 6:    30  31  32  33  34
/// 5:    25  26  27  28  29
/// 4:    20  21  22  23  24
/// 3:    15  16  17  18  19
/// 2:    10  11  12  13  14
/// 1:    5   6   7   8   9
/// 0:    0   1   2   3   4       
///
/// 일경우 기존 [x , y ]에서 [4 , 5] 의 값은 29
/// 배열을 [MaxX,MaxY]가 아닌 [MaxX * MaxY] 로 일차원으로 할 시 
/// [x,y] == [ y * MaxX + x ] 이며
/// [4, 5]에 대입시 [5*5+4] == [29] 이며 값은 해당인덱스로 그대로 초기화 했으니 29 로 같다 . 어떤데
///
/// [NativeContainer]
/// [NativeContainerSupportsMinMaxWriteRestriction]
/// [NativeContainerSupportsDeallocateOnJobCompletion]
public class CacheOptimizedGrid <T>: IDisposable where T : unmanaged{
   
    /// <summary> 넓이 </summary>
    private readonly protected int width;
    /// <summary> 높이 </summary>
    private readonly protected  int height ;
    /// <summary> 버퍼 </summary>
    [NativeDisableParallelForRestriction]
    public NativeArray<T> Buffer;

    private Allocator allocator;
    protected CacheOptimizedGrid( int width,  int height, Allocator allocator) {
        this.width = width;
        this.height = height;
        this.allocator = allocator;
        this.Buffer  = new NativeArray<T>(width * height, allocator, NativeArrayOptions.ClearMemory);
    }
    
    // /// <summary>
    // /// 인덱서 ( 1차원이지만 기존 2차원 처럼 사용 가능 ) 
    // /// </summary>
    // /// <param name="x">x</param>
    // /// <param name="y">y</param>
    // public T this[int x, int y] {
    //     get {
    //         int index = y * width + x;  
    //         return Buffer[index];    
    //     }
    //     set {
    //         int index = y * width + x; 
    //         Buffer[index] = value;  
    //     }
    // }
    
    /// <summary>
    /// 참조 반환 
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    //TODO JIT관점에서는 좀 뒷떨어지니 만약 프레임떨어질시  [x,y]가 아닌 쓰던곳 전부 [y * width + x] 수정 
    //    public ref T this[int x, int y] => ref Buffer[y * width + x];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ref  T GetRef( int x,  int y)
    {
        void* ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(Buffer);
        return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, y * width + x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ref readonly T GetRef_ReadOnly( int x,  int y)
    {
        void* ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(Buffer);
        return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, y * width + x);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ref  T GetRef(in int2 pos )
    {
        void* ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(Buffer);
        return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, pos.y * width + pos.x);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ref readonly T GetRef_ReadOnly(in int2 pos )
    {
        void* ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(Buffer);
        return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, pos.y * width + pos.x);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref  T GetRef( int x, int y , int width,in NativeArray<T> buffer)
    {
        void* ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(buffer);
        return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, y * width + x);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref readonly T GetRef_ReadOnly( int x,  int y , int width,in NativeArray<T> buffer)
    {
        void* ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(buffer);
        return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, y * width + x);
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref  T GetRef(in int2 pos, int width, in NativeArray<T> buffer)
    {
        void* ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(buffer);
        return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, pos.y * width + pos.x);
    }
    
      
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref readonly T GetRef_ReadOnly(in int2 pos, int width, in NativeArray<T> buffer)
    {
        void* ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(buffer);
        return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, pos.y * width + pos.x);
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref readonly T GetRef( int index,in NativeArray<T> buffer)
    {
        void* ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(buffer);
        return ref UnsafeUtility.ArrayElementAsRef<T>(ptr,index );
    }
    
    

    /// <summary>
    /// Set Value
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <param name="value"> 할당값 </param>
    public void SetValue( int x, int y, T value) {
        int index = y * width + x;  
        Buffer[index] = value;   
    }

    /// <summary>
    /// Get Value
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <param name="value"> 할당값 </param>
    public T GetCellValue( int x, int y) {
        int index = y * width + x;  
        return Buffer[index];   
    }

    /// <summary>
    /// 각 차원 별 길이 반환 ( 2차원을 1차원이여서 높이 , 넓이 반환 )
    /// </summary>
    /// <param name="index"> 차원 주소 ( 0, 1 )</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public int GetLength( uint index)
    {
        if (index == 0)
            return width;
        else if (index == 1)
            return height;
        else
            throw new Exception($"BufferManager의 GetLength에 올바르지 못한 값 ( 최대 : 1 ( 2차원 배열 ) )\n 잘못된 값 : {index}");
    }

    /// <summary>
    /// 주위 8칸의 Cell들 반환 ( Foreach용 )
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public IEnumerable<T> GetNeighbors( int x,  int y)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
    
                int nx = x + dx;
                int ny = y + dy;
    
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    yield return Buffer[ny * width + nx];
                }
            }
        }
    }

    public  void Dispose()
    {
        if(Buffer.IsCreated)
            Buffer.Dispose();
    }
    public  void Dispose(JobHandle jobHandle)
    {
        if(Buffer.IsCreated)
            Buffer.Dispose(jobHandle);
    }
}


