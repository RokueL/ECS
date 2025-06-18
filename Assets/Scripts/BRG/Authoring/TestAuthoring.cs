using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;

public class TestAuthoring : MonoBehaviour
{
    public List<Mesh> Meshes;
    public Material Material;
    public bool CastShadow;

    public float ObjectScale = 0.1f;
    public int width;
    public int height;


    [GenerateTestsForBurstCompatibility]
    public struct SpawnJob : IJobParallelFor
    {
        public Entity Prototype;
        public int MeshID;
        public int width;
        public int height;
        public float ObjectScale;
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public NativeArray<RenderBounds> MeshBounds;

        public void Execute(int index)
        {
            var e = Ecb.Instantiate(index, Prototype);
            // Prototype has all correct components up front, can use SetComponent
            Ecb.SetComponent(index, e, new LocalToWorld { Value = ComputeTransform(index) });
            Ecb.SetComponent(index, e, new MaterialColor() { Value = ComputeColor(index) });
            // MeshBounds must be set according to the actual mesh for culling to work.
            // 메쉬 지정
            Ecb.SetComponent(index, e, MaterialMeshInfo.FromRenderMeshArrayIndices(0, MeshID));
            Ecb.SetComponent(index, e, MeshBounds[MeshID]);
        }

        public float4 ComputeColor(int index)
        {
            // float t = (float) index / (EntityCount - 1);
            // var color = Color.HSVToRGB(t, 1, 1);
            // return new float4(color.r, color.g, color.b, 1);
            return new float4(0f, 1f, 0f, 1);
        }

        public float4x4 ComputeTransform(int index)
        {
            int x = index % width;
            int y = index / width;
            
            float4x4 M = float4x4.TRS(
                new float3(x, y, 0),
                quaternion.identity,
                new float3(ObjectScale));

            return M;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;

        EntityCommandBuffer ecbJob = new EntityCommandBuffer(Allocator.TempJob);

        var filterSettings = RenderFilterSettings.Default;
        filterSettings.ShadowCastingMode = ShadowCastingMode.Off;
        filterSettings.ReceiveShadows = false;

        var renderMeshArray = new RenderMeshArray(new[] { Material }, Meshes.ToArray());
        var renderMeshDescription = new RenderMeshDescription
        {
            FilterSettings = filterSettings,
            LightProbeUsage = LightProbeUsage.Off,
        };

        var prototype = entityManager.CreateEntity();
        RenderMeshUtility.AddComponents(
            prototype,
            entityManager,
            renderMeshDescription,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        entityManager.AddComponentData(prototype, new MaterialColor());

        var bounds = new NativeArray<RenderBounds>(Meshes.Count, Allocator.TempJob);
        for (int i = 0; i < bounds.Length; ++i)
            bounds[i] = new RenderBounds { Value = Meshes[i].bounds.ToAABB() };

        // Spawn most of the entities in a Burst job by cloning a pre-created prototype entity,
        // which can be either a Prefab or an entity created at run time like in this sample.
        // This is the fastest and most efficient way to create entities at run time.
        var spawnJob = new SpawnJob
        {
            Prototype = prototype,
            Ecb = ecbJob.AsParallelWriter(),
            width = width,
            height = height,
            MeshBounds = bounds,
            ObjectScale = ObjectScale,
        };

        var spawnHandle = spawnJob.Schedule(width * height, 128);
        bounds.Dispose(spawnHandle);

        spawnHandle.Complete();

        ecbJob.Playback(entityManager);
        ecbJob.Dispose();
        entityManager.DestroyEntity(prototype);
    }

    // Update is called once per frame
    void Update()
    {
    }
}