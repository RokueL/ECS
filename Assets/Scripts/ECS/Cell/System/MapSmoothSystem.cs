using System;
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
public partial struct MapSmoothSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapSmoothingTag>(); // InitTag가 있을 때만 작동
    }

    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("맵 스무딩");

        var mapData = SystemAPI.GetSingleton<MapData>();
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // 현재 상태와 이전 상태를 저장할 해시맵
        var mapCopy = new NativeHashMap<int2, CellVisualType>(mapData.Width * mapData.Height, Allocator.TempJob);
        var prevMapCopy = new NativeHashMap<int2, CellVisualType>(mapData.Width * mapData.Height, Allocator.TempJob);

        // 초기 맵 상태 저장
        foreach (var (cell, transform) in SystemAPI.Query<CellDataJSH, LocalTransform>())
        {
            int2 pos = (int2)transform.Position.xy;
            mapCopy[pos] = cell.CellVisualType;
        }

        for (int i = 0; i < 25; i++)
        {
            // 이전 상태 복사
            prevMapCopy.Clear();
            var mapCopyEnumerator = mapCopy.GetEnumerator();
            
            while (mapCopyEnumerator.MoveNext())
            {
                prevMapCopy[mapCopyEnumerator.Current.Key] = mapCopyEnumerator.Current.Value;
            }

            // 셀룰러 오토마타 수행
            foreach (var (cell, transform, entity) in SystemAPI.Query<RefRW<CellDataJSH>, LocalTransform>().WithEntityAccess())
            {
                int2 pos = (int2)transform.Position.xy;
                int wallCount = 0;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int2 neighbor = pos + new int2(dx, dy);

                        // 범위 밖은 벽으로 간주
                        if (neighbor.x < 0 || neighbor.x >= mapData.Width || neighbor.y < 0 || neighbor.y >= mapData.Height)
                        {
                            wallCount++;
                            continue;
                        }

                        if (prevMapCopy.TryGetValue(neighbor, out CellVisualType CellType) && CellType == CellVisualType.Wall)
                            wallCount++;
                    }
                }

                // 상태 갱신
                if(wallCount >= 5)
                    cell.ValueRW.CellVisualType = CellVisualType.Wall;
                else
                    cell.ValueRW.CellVisualType = CellVisualType.Empty;
                
                mapCopy[pos] = cell.ValueRW.CellVisualType;

                float4 color = new float4(0, 0, 0, 0);
                switch (cell.ValueRO.CellVisualType)
                {
                    case CellVisualType.Empty:
                        color = new float4(1f, 1f, 1f, 0f);
                        break;
                    case CellVisualType.Wall:
                        color = new float4(0f, 0f, 0f, 1f);
                        break;
                }
                
                entityManager.SetComponentData(entity, new ColorOverrid() { Value = color });
            }
        }

        // 스무딩 완료 태그 추가
        Entity settingsEntity = SystemAPI.GetSingletonEntity<MapData>();
        entityManager.AddComponent<MapDoneTag>(settingsEntity);
        // 태그 제거해서 일회성 실행으로 전환
        entityManager.RemoveComponent<MapSmoothingTag>(settingsEntity);
        mapCopy.Dispose();
        prevMapCopy.Dispose();
        state.Enabled = false;

    }
}