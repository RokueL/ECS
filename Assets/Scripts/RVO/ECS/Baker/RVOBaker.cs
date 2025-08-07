using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class RVOBaker : MonoBehaviour
{
    public int SpawnCount;
    public Transform[] SpawnPosition;
}

class RVOBakerBaker : Baker<RVOBaker>
{
    public override void Bake(RVOBaker authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity,new RVOData()
        {
            SpawnCount = authoring.SpawnCount
        });
        var buffer = AddBuffer<SpawnPositionData>(entity);

        foreach (var transform in authoring.SpawnPosition)
        {
            buffer.Add(new SpawnPositionData() { SpawnPosition = transform.position });
        }
    }
}

public struct RVOData : IComponentData
{
    public int SpawnCount;
}

public struct SpawnPositionData : IBufferElementData
{
    public float3 SpawnPosition;
}
