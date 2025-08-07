using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CellBufferManager : IDisposable
{
    /// <summary> 맵 넓이 </summary>
    [Header(" 맵 넓이 ")]
    private int width;
    /// <summary> 맵 높이 </summary>
    [Header(" 맵 높이 ")]
    private int height;
    
    /// <summary> 읽기 버퍼 </summary>
    public CellBuffer ReadCellBuffer;
    
    /// <summary> 쓰기버퍼 </summary>
    public CellBuffer WriteCellBuffer;
    
    /// <summary> 더티 매니저 </summary>
    private DirtyCellManager dirtyCellManager;
    
    /// <summary> 현재 DiryCell </summary>
    public int CurrentDirtyCount = 0;

    private Allocator allocator;



    

    /// <summary>
    /// 초기 버퍼의 크기를 정하고 각 버퍼 을 초기화합니다.( 2차원 배열을 1차원으로 함 )
    /// </summary>
    /// <param name="width"> 넓이 </param>
    /// <param name="height"> height </param>
    /// <param name="dirtyCellSamplingRate"> dirtyCellManager의 크기 초기 할당 = (width*height)/dirtyCellSamplingRate </param>
    /// <param name="allocator"></param>
    public CellBufferManager( int width, int height, int dirtyCellSamplingRate, Allocator allocator)
    {
        this.width = width;
        this.height = height;
        this.allocator = allocator;
        ReadCellBuffer = new CellBuffer( width, height, allocator);
        WriteCellBuffer = new CellBuffer( width, height, allocator);
        dirtyCellManager = new DirtyCellManager( width,  height,  dirtyCellSamplingRate,  allocator);
    }

    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // ReadBuffer 로직 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    
        
    #region Read 버퍼 
    
    /// <summary>
    /// ReadBuffer에서 Ref가져오기 
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <returns></returns>
    public ref readonly Cell GetRefReadCell(  int x, int y)
    {
        return ref ReadCellBuffer.GetRef_ReadOnly( x,  y);
    }
    
    /// <summary>
    ///  ReadBuffer에서 Ref가져오기 
    /// </summary>
    /// <param name="pos"> 좌표 </param>
    /// <returns></returns>
    public ref readonly Cell GetRefReadCell(in int2 pos)
    {
        return ref ReadCellBuffer.GetRef_ReadOnly( pos.x, pos.y);
    }
    
    
    /// <summary>
    /// ReadBuffer에서 Cell 가져오기 (struct 복사)
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <returns></returns>
    public   Cell GetReadCell( int x,  int y)
    {
        return  ReadCellBuffer.Buffer[y*width+x];
    }
    
    /// <summary>
    /// ReadBuffer에서 Cell 가져오기 (struct 복사)
    /// </summary>
    /// <param name="pos"> 좌표 </param>
    /// <returns></returns>
    public  Cell GetReadCell(in int2 pos)
    {
        return  ReadCellBuffer.Buffer[pos.y*width+pos.x];
    }

    #endregion
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // WriteBuffer 로직 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    
    #region write 버퍼 
    /// <summary>
    /// WriteBuffer에서 Ref가져오기 
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <returns></returns>
    public ref Cell GetRefWriteCell( int x, int y)
    {
        return ref WriteCellBuffer.GetRef( x,  y);
    }
    
    /// <summary>
    ///  WriteBuffer에서 Ref가져오기 
    /// </summary>
    /// <param name="pos"> 좌표 </param>
    /// <returns></returns>
    public  Cell GetWriteCell(  int x,    int y)
    {
        return  WriteCellBuffer.Buffer[y*width+x];
    }
    
    /// <summary>
    /// WriteBuffer에서 Cell 가져오기 (struct 복사)
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <returns></returns>
    public ref Cell GetRefWriteCell(in int2 pos)
    {
        return ref WriteCellBuffer.GetRef( pos.x, pos.y);
    }
    
    /// <summary>
    /// WriteBuffer에서 Cell 가져오기 (struct 복사)
    /// </summary>
    /// <param name="pos"> 좌표 </param>
    /// <returns></returns>
    public  Cell GetWriteBufferCell(in int2 pos)
    {
        return  WriteCellBuffer.Buffer[pos.y*width+pos.x];
    }

    #endregion
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // Job Thread 용 로직 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
  
    /// <summary>
    /// Job Thread용 CellDirty만들기
    /// </summary>
    /// <exception cref="Exception"> 인덱스 범위 Out </exception>
    public void MarkDirtyForJobThread( int x, int y)
        => dirtyCellManager.MarkDirtyForJobThread( x, y);
    
    /// <summary>
    /// Job Thread용 CellDirty만들기
    /// </summary>
    /// <exception cref="Exception"> 인덱스 범위 Out </exception>
    public void MarkDirtyForJobThread(in int2 pos)
        =>  dirtyCellManager.MarkDirtyForJobThread(in pos);

    /// <summary>
    ///  JobThread용 주변 셀 Dirty 상태로 만들기 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void MarkNeighborsDirtyForJobThread( int x,  int y)
      =>  dirtyCellManager.MarkNeighborsDirtyForJobThread( x,  y);
     
    /// <summary>
    ///  JobThread용 주변 셀 Dirty 상태로 만들기 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void MarkNeighborsDirtyForJobThread(in int2 pos)
       => dirtyCellManager.MarkNeighborsDirtyForJobThread(in pos);
    
    /// <summary>
    /// HashSet이였던것을 Array로 반환 ( IJobParallelFor )
    /// </summary>
    /// <returns></returns>
    public NativeList<int2> GetDirtyCellsList()
        => dirtyCellManager.GetDirtyCellsList();

    
    /// <summary>
    /// Job Thread내부에서 사용하는 Set용 Writer ( 공유자원 보장 )
    /// </summary>
    /// <returns></returns>
    public NativeParallelHashSet<int2>.ParallelWriter GetNextDirtyWriter()
        => dirtyCellManager.GetNextWriter();
    
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // Main Thread 용 로직 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================

    /// <summary>
    /// Main Thread용 CellDirty만들기
    /// </summary>
    /// <exception cref="Exception"> 인덱스 범위 Out </exception>    
    public void MarkDirtyForMainThread( int x,  int y)
        => dirtyCellManager.MarkDirtyForMainThread( x,  y);

    /// <summary>
    /// Main Thread용 CellDirty만들기
    /// </summary>
    /// <exception cref="Exception"> 인덱스 범위 Out </exception>  
    public void MarkDirtyForMainThread(in int2 pos)
        => dirtyCellManager.MarkDirtyForMainThread(in pos);
    
    /// <summary>
    /// Main Thread용 주변 셀 Dirty 상태로 만들기 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void MarkNeighborsDirtyForMainThread( int x,  int y)
        => dirtyCellManager.MarkNeighborsDirtyForMainThread( x,  y);
    
     
    /// <summary>
    /// Main Thread용 주변 셀 Dirty 상태로 만들기 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void MarkNeighborsDirtyForMainThread(in int2 pos)
        => dirtyCellManager.MarkNeighborsDirtyForMainThread(in pos);
    
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // 사용자 정의 함수 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
  
    /// <summary>
    /// 현재 Dirty Cell 반환
    /// </summary>
    /// <returns></returns>
    public NativeParallelHashSet<int2> GetDirtyCells()
        => dirtyCellManager.GetDirtyCells();

      
    /// <summary>
    /// Write 버퍼를 Read로 스왑 
    /// </summary>
    /// <returns></returns>
    public void SwapBuffer()
    {
        SwapCellBuffer();
        dirtyCellManager.SwapBuffers();
        CurrentDirtyCount = dirtyCellManager.GetDirtyCells().Count();
        UpdatingWriteBuffer();
    }
    
    /// <summary>
    /// Read버퍼에 있던 데이터를 write버퍼에 업데이트
    /// </summary>
    private void UpdatingWriteBuffer()
    {
        // 두 버퍼를 Swap하면서 사용 중이여서 write버퍼는 스왑 후 전 데이터이기에  업데이트 헤준다. 
        foreach (int2 dirtyCell in dirtyCellManager.GetDirtyCells())
        {
            // Debug.Log($"Dirty {dirtyCell.x} : {dirtyCell.y}");
            ref Cell cell = ref ReadCellBuffer.GetRef(in dirtyCell);
            WriteCellBuffer.GetRef(in dirtyCell).SetValue(in cell);
        }
    }
    
    /// <summary>
    /// Read와 Write 버퍼 교체하기 ( Read 버퍼 최신화)
    /// </summary>
    private void SwapCellBuffer()
      =>  (ReadCellBuffer, WriteCellBuffer) = (WriteCellBuffer, ReadCellBuffer);

    /// <summary>
    /// 현재 Cell 전부 Dirty화 
    /// </summary>
    public void MakeAllDirty()
        => dirtyCellManager.MarkAllDirty();
    
    
    /// <summary>
    /// 해당 셀이 범위에서 벗어나는지 체크 
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <returns></returns>
    public bool CheckOutOfIndex( int x, int y)
    => x < 0 || y < 0 || x >= width || y >= height;

    /// <summary>
    /// 해당 셀이 범위에서 벗어나는지 체크 
    /// </summary>
    /// <param name="pos"> 좌표 </param>
    /// <returns></returns>
    public bool CheckOutOfIndex(in int2 pos)
        => pos.x < 0 || pos.y < 0 || pos.x >= width || pos.y >= height;

        
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // 인터페이스 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================

    
    public void Dispose()
    {
        ReadCellBuffer?.Dispose();
            
         WriteCellBuffer?.Dispose();
        
        dirtyCellManager?.Dispose();
    }
}
