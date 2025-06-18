using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;



public struct Spawner : IComponentData
{
    public Entity Prefab;
    public float SpawnInterval;
    public float Timer;
    public bool IsSpawning; // ← 토글 상태 추가
}