using Unity.Entities;
using UnityEngine;
class MapBuildAuthoring : MonoBehaviour
{
    /// <summary> 맵 넓이 </summary>
    [SerializeField]  [Header(" 맵 넓이 ")]
    public int width;
    /// <summary> 맵 높이 </summary>
    [SerializeField] [Header(" 맵 높이 ")]
    public int height;
  
    /// <summary> 맵 생성 시드 </summary>
    [SerializeField][Header(" 맵 생성 시드 ")] 
    public ulong seed;
    /// <summary> 맵 생성 랜덤할당 </summary>
    [SerializeField][Header(" 맵 생성 랜덤할당 ")]  
    public bool useRandomSeed;

    /// <summary> 맵 생성시 공간 비율 </summary>
    [Range(0, 100)] [SerializeField][Header(" 맵 생성시 공간 비율 ")]  
    public int randomFillPercent;
    
    /// <summary> 맵 Smooth 로직 횟수 </summary>
    [SerializeField] [Header(" 맵 Smooth 로직 횟수 ")]  
    public int smoothNum;


    public GameObject ett;

}

class MapBuildAuthoringBaker : Baker<MapBuildAuthoring>
{
    public override void Bake(MapBuildAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new MapBuildData
        {
            Width = authoring.width,
            Height= authoring.height,
            Seed= authoring.seed,
            UseRandomSeed= authoring.useRandomSeed,
            randomFillPercent= authoring.randomFillPercent,
            SmoothNum= authoring.smoothNum,
            Entity =  GetEntity(authoring.ett,TransformUsageFlags.Renderable)
        });
    }
    
    
}
