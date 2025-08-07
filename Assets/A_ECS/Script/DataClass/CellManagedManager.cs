using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct CellManagedManager : IDisposable
{
    /// <summary> 맵 넓이 </summary>
    [Header(" 맵 넓이 ")]
    private int width;
    /// <summary> 맵 높이 </summary>
    [Header(" 맵 높이 ")]
    private int height;
    /// <summary> 버퍼 </summary>
    [NativeDisableParallelForRestriction]
    public NativeArray<Entity> Buffer;
    
    
    public void Dispose()
    {
      
    }
}
