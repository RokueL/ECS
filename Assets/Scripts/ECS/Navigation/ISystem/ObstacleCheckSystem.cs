using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(NavGridGeneratorSystem))]
public partial struct ObstacleCheckSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("콜라이더 체크");
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        //state.Enabled = false;
        var nodeQuery = SystemAPI.QueryBuilder().WithAll<NavNode>().Build();
        var nodes = nodeQuery.ToEntityArray(Allocator.Temp);
        var nodeLookup = SystemAPI.GetComponentLookup<NavNode>(true);
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        // 관리용 엔티티 생성 (단 1개)
        var managerEntity = ecb.CreateEntity();
        ecb.AddComponent<NodeGroupTag>(managerEntity);
        var navNodeBuffer = ecb.AddBuffer<NodeGroup>(managerEntity);
        
        foreach (var entity in nodes)
        {
            var node = nodeLookup[entity];
            NavNode WriteNode = node;

            float3 worldPos = new float3(node.WorldPos.x, 0f, node.WorldPos.z);
            float3 halfSize = new float3(0.3f, 0, 0.3f);

            // 예: 구조물은 레이어 1, 플레이어는 레이어 2라고 가정
            var filter = new CollisionFilter
            {
                BelongsTo = 1 << 2,         // 나는 플레이어 레이어에 속하고
                CollidesWith = 0,      // 구조물과만 충돌하고 싶다
                GroupIndex = 0
            };
            
            var input = new OverlapAabbInput
            {
                Aabb = new Aabb
                {
                    Min = worldPos - halfSize,
                    Max = worldPos + halfSize
                },
                Filter = CollisionFilter.Default
            };

            NativeList<int> dummy = new(Allocator.Temp);
            bool hasHit = physicsWorld.OverlapAabb(input, ref dummy);
            WriteNode.Walkable = !hasHit;


            ecb.SetComponent(entity, WriteNode);
            navNodeBuffer.Add(new NodeGroup()
            {
                Entity = entity,
                Node = WriteNode
            });
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        state.Enabled = false;
    }
}