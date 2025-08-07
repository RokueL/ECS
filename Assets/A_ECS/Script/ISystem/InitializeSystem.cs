using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

[UpdateInGroup(typeof(InitializeSystemGroup))]
internal partial struct InitializeSystem : ISystem
{
    
    public bool RenderAdded;
    public bool JobScheduled;
    public JobHandle InitializationJobHandle;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapBuildData>();
    }
    
    public void OnUpdate(ref SystemState state)
    {
 
        if (!JobScheduled)
        {
            var cellManager = state.World.GetExistingSystemManaged<CellBufferManagerBase>();

            var mapBuildDataEntity = SystemAPI.GetSingletonEntity<MapBuildData>();
            var mapBuildData = SystemAPI.GetComponent<MapBuildData>(mapBuildDataEntity);
            cellManager.Buffer = new NativeArray<Entity>( mapBuildData.Width * mapBuildData.Height, Allocator.Persistent);
            ScheduleEntityCreationJob(ref state, cellManager.Buffer, ref mapBuildData);
            
        }
        else if (JobScheduled && InitializationJobHandle.IsCompleted && !RenderAdded)
        {

            InitializationJobHandle.Complete();
            AddRenderMeshComponentsToCells(state.EntityManager);
        }
        
        // if (!initializeSystemState._jobScheduled)
        // {
        //     ScheduleEntityCreationJob(ref state);
        // }
        // else if (!_renderAdded && _initializationJobHandle.IsCompleted)
        // {
        //     _initializationJobHandle.Complete();
        //
        //     var entityManager = state.EntityManager;
        //     AddRenderMeshComponentsToCells(entityManager, _mesh, _material);
        //
        //     _renderAdded = true;
        // }
        //
      //  CreateEntity(ref state, state.EntityManager,ref mapBuildDataEntity,ref cellManager.Buffer,ref mapBuildData);
        
    }

    [BurstCompile]
    private void ScheduleEntityCreationJob(ref SystemState state,NativeArray<Entity> buffer, ref MapBuildData mapBuildData)
    {
        var entityManager = state.EntityManager;

        int width = mapBuildData.Width;
        int height =  mapBuildData.Height;
        int2 halfSize = new int2(width / 2, height / 2);

        var commandBufferSystem = state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        var ecb = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        var archetype = entityManager.CreateArchetype(
            typeof(CellData),
            typeof(LocalTransform)
        );
        var job = new CreateCellEntitiesJob
        {
            Archetype = archetype,
            Buffer = buffer,
            Width = width,
            Height = height,
            HalfSize = halfSize,
            ECB = ecb,enm = mapBuildData.Entity,
        };

        InitializationJobHandle = job.Schedule(width * height, 64, state.Dependency);
        commandBufferSystem.AddJobHandleForProducer(InitializationJobHandle);
        JobScheduled = true;
    }
    
    Mesh CreateQuad()
    {
        var mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(0,1),
            new Vector2(1,1),
        };
        mesh.triangles = new int[]
        {
            0,2,1, 2,3,1
        };
        
        mesh.RecalculateBounds();
        return mesh;
    }
    
    private void AddRenderMeshComponentsToCells(EntityManager entityManager)
    {
       EntityQuery query = entityManager.CreateEntityQuery(typeof(CellData));

        Texture2D texture = Resources.Load<Texture2D>("Groza"); // PNG 또는 SpriteAtlas texture
        var shader = Shader.Find("Shader Graphs/TileMaterial"); // Lit도 OK
        Material spriteMaterial = new Material(shader)
        {
           //mainTexture = texture
        };
        spriteMaterial.enableInstancing = true;
        Mesh quadMesh =  CreateQuad(); // 또는 Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        quadMesh.RecalculateBounds(); 
        
        var renderMeshArray = new RenderMeshArray(new[] { spriteMaterial }, new[] { quadMesh });
        var desc = new RenderMeshDescription(
            motionVectorGenerationMode: MotionVectorGenerationMode.ForceNoMotion,
            shadowCastingMode: ShadowCastingMode.Off,
            receiveShadows: false);
        using var entities = query.ToEntityArray(Allocator.Temp);
        
        foreach (var entity in entities)
        {
            
            RenderMeshUtility.AddComponents(
                entity,
                entityManager,
                desc,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        }
        RenderAdded = true;
    }

    
    [BurstCompile]
    private struct CreateCellEntitiesJob : IJobParallelFor
    {
        public EntityArchetype Archetype;
        public Entity enm;
        public int Width;
        public int Height;
        public int2 HalfSize;
        public EntityCommandBuffer.ParallelWriter ECB;
        public NativeArray<Entity> Buffer;
        public void Execute(int index)
        {
            int x = index % Width;
            int y = index / Width;

            Entity entity = ECB.CreateEntity(index,Archetype);
            Buffer[index] = entity;
            
            int2 pos = new int2(x, y);
            int2 gridPos = pos - HalfSize;
            float3 worldPos = new float3(gridPos.x, gridPos.y, 0f);

            ECB.AddComponent(index, entity, new CellData
            {
                Pos = pos,
                ElementType = ElementType.Empty,
                Amount = 0,
            });
            ECB.AddComponent(index, entity, new LocalTransform
            {
                Position = worldPos,
                Rotation = quaternion.identity,
                Scale = 1f,
            });
        }
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}
