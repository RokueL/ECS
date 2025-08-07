using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Entity = Unity.Entities.Entity;
using Random = Unity.Mathematics.Random;

[DisableAutoCreation]
public class NPCSpawner : MonoBehaviour
{
    public List<Mesh> Meshes;
    public List<Material> Materials;

    public Transform[] SpawnPosition;
    
    public int SpawnCount;
    public float SpawnDelay;
    
    public void Start()
    {
        Debug.Log("시작");
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        
        
        var spawnPositions = new NativeArray<float3>(SpawnPosition.Length, Allocator.Persistent);
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            spawnPositions[i] = SpawnPosition[i].position;
        }

        var filterSettings = RenderFilterSettings.Default;
        filterSettings.ShadowCastingMode = ShadowCastingMode.Off;
        filterSettings.ReceiveShadows = false;

        var renderMeshArray = new RenderMeshArray(Materials.ToArray(), Meshes.ToArray());
        var renderMeshDescription = new RenderMeshDescription
        {
            FilterSettings = filterSettings,
            LightProbeUsage = LightProbeUsage.Off,
        };
        
        Debug.Log("NPC 임시 만듬");
        
        var NPCPrefab = entityManager.CreateEntity();
        RenderMeshUtility.AddComponents(
            NPCPrefab,
            entityManager,
            renderMeshDescription,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        entityManager.AddComponentData(NPCPrefab, new ColorOverrid()
        {
            Value = new float4(1f,1f,1f,1f)
        });
        // 에디터 / 개발 빌드에서만 동작
#if UNITY_EDITOR
        entityManager.SetName(NPCPrefab, "NPC-Prefab");
#endif
        var bounds = new NativeArray<RenderBounds>(Meshes.Count, Allocator.TempJob);
        for (int i = 0; i < bounds.Length; ++i)
            bounds[i] = new RenderBounds { Value = Meshes[i].bounds.ToAABB() };
        
        EntityCommandBuffer ecbJob = new EntityCommandBuffer(Allocator.TempJob);
        
        Debug.Log("스폰 시작");
        var spawnJob = new SpawnJob
        {
            NpcPrefab = NPCPrefab,
            rng = new Random((uint)DateTime.Now.Ticks),
            MeshCount = Meshes.Count,
            MaterialCount = Materials.Count,
            SpawnPositions = spawnPositions,
            Ecb = ecbJob.AsParallelWriter(),
            MeshBounds = bounds,
        };
        var spawnHandle = spawnJob.Schedule(SpawnCount, 128);
        bounds.Dispose(spawnHandle);
        
        spawnHandle.Complete();
        Debug.Log("스폰 끝");
        
        ecbJob.Playback(entityManager);
        ecbJob.Dispose();
        entityManager.DestroyEntity(NPCPrefab);
    }
    
    
    [GenerateTestsForBurstCompatibility]
    public struct SpawnJob : IJobParallelFor
    {
        public Entity NpcPrefab;
        public int MeshCount;
        public int MaterialCount;
        public Random rng;
        [ReadOnly] public NativeArray<float3> SpawnPositions; 
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public NativeArray<RenderBounds> MeshBounds;

        public void Execute(int index)
        {
            var randomSpawnPoint = rng.NextInt(0, SpawnPositions.Length);
            var e = Ecb.Instantiate(index, NpcPrefab);
            // Prototype has all correct components up front, can use SetComponent
            Ecb.AddComponent(index,e, new LocalTransform());
            Ecb.SetComponent(index, e, new LocalToWorld { Value = TransformToFloat4X4(randomSpawnPoint) });
            Ecb.SetComponent(index,e,new LocalTransform()
            {
                Position = TransformToFloat3(randomSpawnPoint),
                Scale = 1,
                Rotation = Quaternion.identity
            });
            Ecb.AddComponent(index,e,new NPCData()
            {
                E_NPCState = NPCState.None,
                NPCMoveData = new MoveData()
                {
                    MoveSpeed = 3f,
                    RotateSpeed = 1f,
                    MoveWeight = index
                }
            });
            Ecb.AddBuffer<MoveNode>(index,e);
            // MeshBounds must be set according to the actual mesh for culling to work.
            // 메쉬 지정
            var meshID = rng.NextInt(0, MeshCount);
            Ecb.SetComponent(index, e, MaterialMeshInfo.FromRenderMeshArrayIndices(
                rng.NextInt(0,MaterialCount), meshID));
            Ecb.SetComponent(index, e, MeshBounds[meshID]);
        }

        public float4x4 TransformToFloat4X4(int index)
        {
            var position = SpawnPositions[index];
            float4x4 M = float4x4.TRS(
                new float3(position.x, position.y, position.z),
                quaternion.identity,
                new float3(1f));

            return M;
        }
        
        public float3 TransformToFloat3(int index)
        {
            var position = SpawnPositions[index];
            return position;
        }
    }
}
