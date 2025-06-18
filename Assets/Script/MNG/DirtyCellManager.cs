using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

public class DirtyCellManager: System.IDisposable
{
    public static void CopyMapToArrays<TKey, TValue>(
        NativeParallelHashMap<TKey, TValue> map,
        NativeArray<TKey> keys,
        NativeArray<TValue> values)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        int i = 0;
        foreach (var kv in map)
        {
            keys[i] = kv.Key;
            values[i] = kv.Value;
            i++;
        }
    }
    
    /// <summary> 현재 DirtyCell들 (해당 애들만 Simulate )</summary>
    private  NativeParallelHashSet<int2> currentDirty;
    
    /// <summary> JobThread용 다음 DirtyCell들 (해당 애들 마지막에 Swap )</summary>
    private NativeParallelHashSet<int2> nextDirtyForJobThread;
    
    /// <summary> MainThread용 다음 DirtyCell들 (해당 애들 마지막에 Swap )</summary>
    private HashSet<int2> nextDirtyForMainThread;

    /// <summary> IJobParallelFor용 네이티브 리스트  </summary>
    private NativeList<int2> dirtyCellList;
 
    
    /// <summary> 그리드 크기 </summary>
    private readonly int width, height;
    
    private Allocator allocator;

    private bool disposed;
    
    /// <summary>
    /// 초기 버퍼의 크기를 정하고 각 Native Collections을 초기화합니다.( 2차원 배열을 1차원으로 함 )
    /// </summary>
    /// <param name="width"> 넓이 </param>
    /// <param name="height"> height </param>
    /// <param name="dirtyCellSamplingRate">  크기 초기 할당 = (width*height)/dirtyCellSamplingRate </param>
    /// <param name="allocator"></param>
    public DirtyCellManager( int width, int height, int dirtyCellSamplingRate, Allocator allocator)
    {
        this.width = width;
        this.height = height;
        var initialCapacity = (width * height) / dirtyCellSamplingRate;
        currentDirty = new NativeParallelHashSet<int2>(initialCapacity, allocator);
        nextDirtyForJobThread = new NativeParallelHashSet<int2>(initialCapacity, allocator);
        nextDirtyForMainThread = new HashSet<int2>(initialCapacity);
        dirtyCellList = new NativeList<int2>(initialCapacity,allocator);
        this.allocator = allocator;
    }
    
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // Job Thread 로직 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    
    /// <summary>
    /// Job Thread용 CellDirty만들기
    /// </summary>
    /// <exception cref="Exception"> 인덱스 범위 Out </exception>
    public void MarkDirtyForJobThread( int x, int y)
    {
        if (!CheckOutOfIndex( x, y))
            nextDirtyForJobThread.Add(new int2(x,y));
        else
            throw new Exception($"인덱스 범위 벗어남 MarkDirty X = {x} / Y = {y}");
    }
    
    /// <summary>
    /// Job Thread용 CellDirty만들기
    /// </summary>
    /// <exception cref="Exception"> 인덱스 범위 Out </exception>    
    public void MarkDirtyForJobThread(in int2 pos)
    {
        if (!CheckOutOfIndex( pos.x,  pos.y))
            nextDirtyForJobThread.Add(pos);
        else
            throw new Exception($"인덱스 범위 벗어남 MarkDirty X = {pos.x} / Y = {pos.y}");
    }
 
    /// <summary>
    ///  JobThread용 주변 셀 Dirty 상태로 만들기 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void MarkNeighborsDirtyForJobThread( int x,  int y)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) 
                    continue; // 자기 자신 제외
                int nx = x + dx;
                int ny = y + dy;
                
                // 범위 벗어나는지 체크 
                if (CheckOutOfIndex(nx,ny))
                    continue;
    
                nextDirtyForJobThread.Add(new int2(nx, ny));
    
            }
        }
    }
    
     
    /// <summary>
    ///  JobThread용 주변 셀 Dirty 상태로 만들기 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void MarkNeighborsDirtyForJobThread(in int2 pos)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) 
                    continue; // 자기 자신 제외
                int nx = pos.x + dx;
                int ny = pos.y + dy;
                
                // 범위 벗어나는지 체크 
                if (CheckOutOfIndex(nx,ny))
                    continue;
    
                nextDirtyForJobThread.Add(new int2(nx, ny));
    
            }
        }
    }
    
    /// <summary>
    /// HashSet이였던것을 Array로 반환 ( IJobParallelFor )
    /// </summary>
    /// <returns></returns>
    public NativeList<int2> GetDirtyCellsList()
    {
        dirtyCellList.Clear();
        foreach (var pos in currentDirty)
        {
            dirtyCellList.Add(pos);
        }
        return dirtyCellList;
    }
    
    /// <summary>
    /// Job Thread내부에서 사용하는 Set용 Writer ( 공유자원 보장 )
    /// </summary>
    /// <returns></returns>
    public NativeParallelHashSet<int2>.ParallelWriter GetNextWriter()
    {
        return nextDirtyForJobThread.AsParallelWriter();
    }
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // Main Thread 용
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    
    /// <summary>
    /// Main Thread용 CellDirty만들기
    /// </summary>
    /// <exception cref="Exception"> 인덱스 범위 Out </exception>    
    public void MarkDirtyForMainThread( int x,  int y)
    {
        if (!CheckOutOfIndex( x,  y))
            nextDirtyForMainThread.Add(new int2(x,y));
        else
            throw new Exception($"인덱스 범위 벗어남 MarkDirty X = {x} / Y = {y}");
    }
    
    /// <summary>
    /// Main Thread용 CellDirty만들기
    /// </summary>
    /// <exception cref="Exception"> 인덱스 범위 Out </exception>  
    public void MarkDirtyForMainThread( in int2 pos)
    {
        if (!CheckOutOfIndex(pos.x, pos.y))
            nextDirtyForMainThread.Add(pos);
        else
            throw new Exception($"인덱스 범위 벗어남 MarkDirty X = {pos.x} / Y = {pos.y}");
    }
    
    /// <summary>
    /// Main Thread용 주변 셀 Dirty 상태로 만들기 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void MarkNeighborsDirtyForMainThread( int x,  int y, bool includeSelf = true)
    {
        if (includeSelf)
        {
            MarkDirtyForMainThread(x, y);
        }
        for (int i = 0; i < FixedInt2Array4.Count; i++)
        {
            int nx = x + Directions.Cardinal[i].x;
            int ny = y + Directions.Cardinal[i].y;
            // 범위 벗어나는지 체크 
            if (CheckOutOfIndex(nx,ny))
                continue;
            nextDirtyForMainThread.Add(new int2(nx, ny) );
        }
     
    }
    
     
    /// <summary>
    /// Main Thread용 주변 셀 Dirty 상태로 만들기 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void MarkNeighborsDirtyForMainThread(in int2 pos , bool includeSelf = true)
    {
        if (includeSelf)
        {
            MarkDirtyForMainThread(pos);
        }
        
        for (int i = 0; i < FixedInt2Array4.Count; i++)
        {
            // 범위 벗어나는지 체크 
            if (CheckOutOfIndex(pos + Directions.Cardinal[i]))
                continue;
            nextDirtyForMainThread.Add(pos + Directions.Cardinal[i] );
        }

    }


    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // 사용자 함수 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
   
    /// <summary>
    /// 현재 모든 Cell를 Dirty로 만들기 
    /// </summary>
    public void MarkAllDirty()
    {
        currentDirty.Clear();
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            currentDirty.Add(new int2(x, y));
    }
    
    /// <summary>
    /// 다음 더티 셀들을 현재로 옮기기 
    /// </summary>
    public void SwapBuffers()
    {
        currentDirty.Clear();

        foreach (var pos in nextDirtyForJobThread)
            currentDirty.Add(pos);
        foreach (var pos in nextDirtyForMainThread)
        {
            currentDirty.Add(pos);
        }
        nextDirtyForMainThread.Clear();
        nextDirtyForJobThread.Clear();
    }
   
    /// <summary>
    /// 현재 Dirty Cell 반환
    /// </summary>
    /// <returns></returns>
    public NativeParallelHashSet<int2> GetDirtyCells()
    {
        return currentDirty;
    }
    
    
    /// <summary>
    /// 해당 셀이 범위에서 벗어나는지 체크 
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <returns></returns>
    private bool CheckOutOfIndex( int x, int y)
    {
        return x < 0 || y < 0 || x >= width || y >= height;
    }
    /// <summary>
    /// 해당 셀이 범위에서 벗어나는지 체크 
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <returns></returns>
    private bool CheckOutOfIndex(in int2 pos)
    {
        return pos.x < 0 || pos.y < 0 || pos.x >= width || pos.y >= height;
    }

    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // 인터페이스 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    
    
    public void Dispose()
    {
        // TODO release managed resources here
        if (currentDirty.IsCreated)
            currentDirty.Dispose();
        if (nextDirtyForJobThread.IsCreated)
            nextDirtyForJobThread.Dispose();
        if (dirtyCellList.IsCreated)
            dirtyCellList.Dispose();
    }
    
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================

}