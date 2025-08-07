using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
public partial struct MapInitSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapInitTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("맵 생성");
        Entity settingsEntity = SystemAPI.GetSingletonEntity<MapData>();
        var mapData = SystemAPI.GetSingleton<MapData>();
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var rng = new System.Random(mapData.Seed.GetHashCode());
        
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                Entity cell = entityManager.Instantiate(mapData.CellPrefab);
                
                
                bool isWall = rng.Next(0,100) < mapData.MapChance;

                if (x == 0 || x == mapData.Width - 1 || y == 0 || y == mapData.Height - 1)
                {
                    entityManager.SetComponentData(cell, new CellDataJSH() { CellVisualType = CellVisualType.Wall, Postion = new int2(x,y)});
                    isWall = true;
                }
                else
                {
                    if(isWall)
                        entityManager.SetComponentData(cell, new CellDataJSH() { CellVisualType = CellVisualType.Wall, Postion = new int2(x, y) });
                    else
                        entityManager.SetComponentData(cell, new CellDataJSH() { CellVisualType = CellVisualType.Empty, Postion = new int2(x, y) });
                }



                float3 position = new float3(x, y, 0); // y축이 아닌 z축을 사용해 그리드 생성
                entityManager.SetComponentData(cell, new LocalTransform
                {
                    Position = position,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                
                float4 color = isWall
                    ? new float4(0f, 0f, 0f, 1f)  // 벽 - 검정
                    : new float4(1f, 1f, 1f, 0f); // 빈칸 - 투명

    
                entityManager.SetComponentData(cell, new ColorOverrid() { Value = color });
                var buffer = state.EntityManager.GetBuffer<CellGroupData>(settingsEntity);
                buffer.Add(new CellGroupData() { CellEntity = cell });
            }
        }
        
        entityManager.AddComponent<MapSmoothingTag>(settingsEntity);
        state.Enabled = false;
    }
}