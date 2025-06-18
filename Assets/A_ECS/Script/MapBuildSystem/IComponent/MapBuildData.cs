using Unity.Entities;
using UnityEngine.Serialization;

public struct MapBuildData : IComponentData
{
    /// <summary> 맵 넓이 </summary>
    [FormerlySerializedAs("width")] public int Width;
    /// <summary> 맵 높이 </summary>
    [FormerlySerializedAs("height")] public int Height;
    /// <summary> 맵 생성 시드 </summary>
    [FormerlySerializedAs("seed")] public ulong Seed;
    /// <summary> 맵 생성 랜덤할당 </summary>
    [FormerlySerializedAs("useRandomSeed")] public bool UseRandomSeed;
    /// <summary> 맵 생성시 공간 비율 </summary>
    public int randomFillPercent;
    /// <summary> 맵 Smooth 로직 횟수 </summary>
    public int SmoothNum;

    public Entity Entity;
    public bool a;
}
