using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(UnitSpawn))]
public partial struct NavGridGeneratorSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        
        Debug.Log("그리드");
        
        const int gridSizeX = 100;
        const int gridSizeY = 100;
        const float spacing = 1.0f;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Entity node = ecb.CreateEntity();
                NavNode nodeData = new NavNode()
                {
                    Entity = node,
                    GridPos = new int2(x, y),
                    WorldPos = new float3(x * spacing, 0, y * spacing),
                    Walkable = true, // 기본적으로 모두 걷기 가능
                };
                ecb.AddComponent(node, nodeData);
                ecb.AddBuffer<NavNodeInnerEntity>(node);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        state.Enabled = false; // 1회성 실행
    }
}

