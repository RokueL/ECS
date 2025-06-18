using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

public partial struct CellSpawnSystem : ISystem
{
    Entity prefabEntity;
    private EntityManager entityManager;
    private bool prefabLoaded;

    public void OnCreate(ref SystemState state)
    {
//         
//         // 1. Sprite를 Texture로 가져오기
//         Texture2D texture = Resources.Load<Texture2D>("Groza"); // PNG 또는 SpriteAtlas texture
//         var shader = Shader.Find("Universal Render Pipeline/Unlit"); // Lit도 OK
//
// // 2. Sprite용 Material 생성
//         Material spriteMaterial = new Material(shader)
//         {
//             mainTexture = texture,
//             color = Color.white
//         };
//
// // 3. Quad Mesh 사용 (Sprite는 평면이니까)
//         Mesh quadMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx"); // 또는 Resources.GetBuiltinResource<Mesh>("Quad.fbx");
//
// // 4. RenderMeshArray 세팅
//         var renderMeshArray = new RenderMeshArray(new[] { spriteMaterial }, new[] { quadMesh });
//         var world = World.DefaultGameObjectInjectionWorld;
//         entityManager = world.EntityManager;
// // 5. Entity 생성
//         var prototype = entityManager.CreateEntity();
//
//         var desc = new RenderMeshDescription(
//             shadowCastingMode: ShadowCastingMode.Off,
//             receiveShadows: false);
//
// // 6. RenderMeshUtility로 구성
//         RenderMeshUtility.AddComponents(
//             prototype,
//             entityManager,
//             desc,
//             renderMeshArray,
//             MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
//
//         entityManager.AddComponentData(prototype, LocalTransform.FromPosition(new float3(0, 0, 0)));
//         
        // // Spawn most of the entities in a Burst job by cloning a pre-created prototype entity,
        // // which can be either a Prefab or an entity created at run time like in this sample.
        // // This is the fastest and most efficient way to create entities at run time.
        // // 기본 큐브 메쉬랑 머티리얼 가져오기
        // Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        // Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        // {
        //     color = Color.magenta
        // };
        // var world = World.DefaultGameObjectInjectionWorld;
        // entityManager = world.EntityManager;
        //
        //
        // // Create a RenderMeshDescription using the convenience constructor
        // // with named parameters.
        // var desc = new RenderMeshDescription(
        //     shadowCastingMode: ShadowCastingMode.Off,
        //     receiveShadows: false);
        //
        // // Create an array of mesh and material required for runtime rendering.
        // var renderMeshArray = new RenderMeshArray(new Material[] { material }, new Mesh[] { mesh });
        //
        // // Create empty base entity
        // var prototype = entityManager.CreateEntity();
        //
        // // Call AddComponents to populate base entity with the components required
        // // by Entities Graphics
        // RenderMeshUtility.AddComponents(
        //     prototype,
        //     entityManager,
        //     desc,
        //     renderMeshArray,
        //     MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        // entityManager.AddComponentData(prototype,  LocalTransform.FromPosition(new float3(0, 0, 0)));
        //
        //  
        // Debug.Log(this.entityManager.Debug.EntityCount);
        //  
        // Debug.Log(this.entityManager.Debug.GetEntityInfo(prototype));

    }

    public void OnUpdate(ref SystemState state)
    {
    
        
        //
        // // Spawn most of the entities in a Burst job by cloning a pre-created prototype entity,
        // // which can be either a Prefab or an entity created at run time like in this sample.
        // // This is the fastest and most efficient way to create entities at run time.
        // var spawnJob = new SpawnJob
        // {
        //     Prototype = prototype,
        //     Ecb = ecb.AsParallelWriter(),
        //     EntityCount = EntityCount,
        // };
        //
        // var spawnHandle = spawnJob.Schedule(EntityCount, 128);
        // spawnHandle.Complete();

     
        //entityManager.DestroyEntity(prototype);
        
        
        // // ▶️ Graphics Entity 생성
        // var entity = entityManager.CreateEntity();
        //
        // entityManager.AddComponentData(entity, new LocalTransform
        // {
        //     Position = new float3(0, 0, 0),
        //     Rotation = quaternion.identity,
        //     Scale = 1f
        // });
        //
        // entityManager.AddComponentData(entity, new MaterialMeshInfo
        // {
        //     Material = 0,
        //     Mesh = 0,
        // });
        //
        // entityManager.AddSharedComponentManaged(entity, new RenderMeshArray(new[] { material }, new[] { mesh }));
        //
        //
        // entityManager.SetComponentData(entity, new RenderBounds {
        //     Value = mesh.bounds.ToAABB()
        // });
        //
        // entityManager.AddSharedComponentManaged(entity, new RenderMeshArray(new[] { material }, new[] { mesh }));
    }
}