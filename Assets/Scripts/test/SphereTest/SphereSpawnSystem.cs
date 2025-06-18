using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine; // Input 사용

[BurstCompile]
public partial struct SpawnerSystem : ISystem
{
    private bool wasKeyDown;

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        bool keyDown = Input.GetKeyDown(KeyCode.Space);

        // SpawnerEntity 전체 순회
        foreach (var (spawner, entity) in SystemAPI.Query<RefRW<Spawner>>().WithEntityAccess())
        {
            if (keyDown && !wasKeyDown)
            {
                spawner.ValueRW.IsSpawning = !spawner.ValueRO.IsSpawning;
                Debug.Log("Spawner toggled: " + spawner.ValueRO.IsSpawning);
            }

            if (spawner.ValueRO.IsSpawning)
            {
                spawner.ValueRW.Timer += deltaTime;
                if (spawner.ValueRW.Timer >= spawner.ValueRW.SpawnInterval)
                {
                    spawner.ValueRW.Timer = 0f;

                    Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);

                    float3 position = float3.zero;

                    if (SystemAPI.HasComponent<LocalToWorld>(entity))
                    {
                        position = SystemAPI.GetComponent<LocalToWorld>(entity).Position;
                    }

                    state.EntityManager.SetComponentData(newEntity, new LocalTransform
                    {
                        Position = position,
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });
                }
            }
        }

        // 이전 프레임 키 상태 저장
        wasKeyDown = keyDown;
    }
}