using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

//[DisableAutoCreation]
partial struct UnitSpawn : ISystem
{
    private RVOData RvoData;
    private MaterialType E_MaterialType;
    private Entity data;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
       
    }


    public void OnUpdate(ref SystemState state)
    {
        
        Debug.Log("스폰 시작");
        var em = state.EntityManager;
        
        // 메쉬, 머테리얼 가져오기
        var query = SystemAPI.QueryBuilder().WithAll<GraphicData>().Build();
        var graphicSetting = query.GetSingleton<GraphicData>();
        // 생성 설정
        foreach (var (buffer, entity) in SystemAPI.Query<RefRO<RVOData>>()
                     .WithEntityAccess())
        {
            RvoData = buffer.ValueRO;
            data = entity;
            break;
        }
        var spawnBuffer = state.EntityManager.GetBuffer<SpawnPositionData>(data);
        var spawnPositionArray = spawnBuffer.ToNativeArray(Allocator.Temp);
        // 렌더 매쉬 지정
        RenderMeshArray renderMeshArrayy = new RenderMeshArray(    graphicSetting.material.ToArray(),
            graphicSetting.mesh.ToArray());
        
        // 스폰 카운트 만큼 생성
        var instances = new NativeArray<Entity>(RvoData.SpawnCount, Allocator.Temp);
        var normalEntity = em.CreateEntity();
        em.Instantiate(normalEntity, instances);
        float3 spawnPoint;
        int weight = 0;
        
        foreach (var entity in instances)
        {
            var time = (uint)(SystemAPI.Time.ElapsedTime * 1000);
            var seed = math.asuint(entity.Index * 73856093 ^ entity.Version * 19349663 ^ time);
            var rng = new Random(seed);
            spawnPoint = spawnPositionArray[rng.NextInt(0, spawnPositionArray.Length)].SpawnPosition;
            SpawnEntity(em,entity,spawnPoint,renderMeshArrayy,graphicSetting,
                MaterialType.E_Normal,MeshType.E_Capsule,weight);
            weight++;
        }
        
        
        state.Enabled = false;
    }

    public void SpawnEntity(EntityManager em, Entity entity, float3 pos, 
        RenderMeshArray renderMeshArray, GraphicData graphicSetting,MaterialType materialType, MeshType meshType, int weight)
    {
        em.AddComponentData(entity, new NPCData()
        {
            E_NPCState = NPCState.None,

            NPCMoveData = new MoveData()
            {
                MoveSpeed = 3f,
                RotateSpeed = 1f,
                MoveWeight = weight,
                neightborDistance = 2,
                Radius = 0.5f
            }
        });
        em.AddBuffer<MoveNode>(entity);
        // 스폰
        em.AddComponentData(entity, LocalTransform.FromPosition(pos));
        // 렌더 설정
        var renderDesc = new RenderMeshDescription(
            shadowCastingMode: ShadowCastingMode.On,
            receiveShadows: true,
            layer: 0,
            renderingLayerMask: 1 << 1 // ECS 렌더 레이어 1번에 해당
        );
        // 바운드
        var bounds = new RenderBounds {Value = graphicSetting.mesh[0].bounds.ToAABB()};
        em.AddComponentData(entity, bounds);
        // 렌더 매쉬
        RenderMeshUtility.AddComponents(
            entity,
            em,
            renderDesc,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices((int)materialType, (int)meshType)
        );
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
    

}

