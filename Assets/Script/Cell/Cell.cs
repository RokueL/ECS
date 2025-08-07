



using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 셀 타입 
/// </summary>
public enum ElementTile 
{
    Empty = 0 ,
    Stone = 1,
    Water = 2 ,
    Magma = 3, 
}


/// <summary>
/// 셀 원소 
/// </summary>
[System.Flags]
public enum ElementType : ulong
{
    Empty = 0 ,
    Stone = 1 << 0, // 1
    Water = 1 << 1, // 2
    Magma = 1 << 2, // 4
}

/// <summary>
/// 셀 객체
/// </summary>
public struct Cell
{
    /// <summary> 더티 여부 </summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool IsDirty;
    
    /// <summary> 해당 셀 좌표  </summary>
    public int2 Pos;
    
    /// <summary> 해당 셀 원소  </summary>
    public ElementType ElementType;
    

    
    // 원소별 공통 변수 ( 예 : 양, 힘, 방향, 밝기 등 ) 
    #region 공통 변수

    /// <summary> 셀의 양 (1= 100000) </summary>
    public int Amount; 

    #endregion
    
    // 셀의 원소 타입별 필요한 변수 ( NullAble ) 
    #region 개별 변수

    // /// <summary> 셀의 양 </summary>
    // private float? amount; 
    // public float Amount {
    //     get => ElementType == ElementType.Water ? amount.GetValueOrDefault() : 0f;
    //     set { if (ElementType == ElementType.Water) amount = value; }
    // }

    #endregion

    // // 인터페이스로 구현시 인터페이스에서 해당 함수를 호출하는데에 오버해드가 있어 델리게이트로 다형성 구현 
    // /// <summary> 셀 시뮬레이트 델리게이트 </summary>
    // public CellSimulationDelegate Simulate;


    #region 원소 설정 
    /// <summary>
    /// 원소 설정 
    /// </summary>
    /// <param name="elementType"> 세울거 </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetElementType( ElementType elementType)
    {
        this.ElementType = elementType;
    }
    
    /// <summary>
    /// 원소 플래그 세우기
    /// </summary>
    /// <param name="elementType"> 세울거 </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetElementTypeOn( ElementType elementType)
    {
        this.ElementType |= elementType;
    }
    /// <summary>
    /// 원소 플래그 끄기 
    /// </summary>
    /// <param name="elementType"> 끌거 </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetElementTypeOff( ElementType elementType)
    {
        this.ElementType &= ~elementType;
    }

    /// <summary>
    /// 인자로 준게 모드 켜져있어야함 
    /// </summary>
    /// <param name="elementType"></param>
    /// <returns></returns>
    public bool HasElementTypeAll( ElementType elementType)
    {
        return (this.ElementType & elementType) == elementType;
    }
    
    /// <summary>
    /// 인자로 준게 하나만 켜져 있어도됨
    /// </summary>
    /// <param name="elementType"></param>
    /// <returns></returns>
    public bool HasElementTypeOnce( ElementType elementType)
    {
        return (this.ElementType & elementType) != 0;
    }


    public int GetElementTypeValue()
    {
        // 하나의 비트만 켜져 있는지 확인
        if ((ElementType & (ElementType - 1)) == 0)
        {
            // 하나의 비트만 켜져 있으면 해당 값 반환
            return (int)ElementType; 
        }
        return 0;  // 둘 이상의 비트가 켜져 있으면 0 반환 (다중 비트가 켜져 있을 때)
    }
    #endregion
   
    public Cell(in int2 pos,  ElementType elementType) : this()
    {
        Pos = pos;
        this.ElementType = elementType;
        
        
        // if (ElementType == ElementType.Water)
        //     Amount = amount;
        // else
        //     this.amount = null; 
    }
    
    public Cell( int x,  int y,  ElementType elementType) : this()
    {
        Pos = new int2(x, y);
        this.ElementType = elementType;
        
        
        // if (ElementType == ElementType.Water)
        //     Amount = amount;
        // else
        //     this.amount = null; 
    }
    
    
    /// <summary> 주변 Cell들 </summary>
    private static readonly Vector2Int[] neighborOffsets = new Vector2Int[]
    {
        new(-1, 0), new(1, 0), new(0, -1), new(0, 1),
        new(-1, -1), new(-1, 1), new(1, -1), new(1, 1)
    };

    public override string ToString()
    {
        return $"Cell 좌표 = {Pos}\n" +
               $"Cell Element = {ElementType}\n" +
               $"Cell Amount = {Amount}\n" +
               $"";
    }

    public void SetValue(in Cell cell)
    {
        Pos = cell.Pos;
        ElementType = cell.ElementType;
        Amount = cell.Amount;
  
    }

}


/// <summary>
/// 버스트용 흐름 정보 
/// </summary>
public struct FlowInfo
{
    public ElementType ElementType;
    /// <summary> 방향별 흐름 </summary>
    public int4 FlowAmount;
    
    public int TotalFlowAmount =>FlowAmount.x + FlowAmount.y + FlowAmount.z + FlowAmount.w;
}