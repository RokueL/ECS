using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(NavNodeConnectionSystem))]
partial struct NavMoveSystem : ISystem
{
    private DynamicBuffer<NavDestination> Destinations;
    private DynamicBuffer<NodeGroup> NodeGroups;

    private bool hasInitialized;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //return;
        float dt = SystemAPI.Time.DeltaTime;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Query에서 가져온 Entity가 있다고 가정
        foreach (var (buffer, entity) in SystemAPI.Query<DynamicBuffer<NodeGroup>>()
                     .WithEntityAccess())
        {
            NodeGroups = buffer;
            break;
        }

        foreach (var (npcData, transform, entity) in
                 SystemAPI.Query<RefRW<NPCData>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            switch (npcData.ValueRO.E_NPCState)
            {
                case NPCState.None:
                    // if(npcData.ValueRO.CurrentPathIndex != 0)
                    //     return;
                    foreach (var buffer in SystemAPI
                                 .Query<DynamicBuffer<NavDestination>>())
                    {
                        Destinations = buffer;
                        break;
                    }

                    var time = (uint)(SystemAPI.Time.ElapsedTime * 1000);
                    var seed = math.asuint(entity.Index * 73856093 ^ entity.Version * 19349663 ^ time);
                    if (seed == 0) seed = 1; // 절대 0이 되면 안됨
                    var rng = new Random(seed);
                    int rngNum = rng.NextInt(0, Destinations.Length);
                    var clearBuffer = SystemAPI.GetBuffer<MoveNode>(entity);
                    clearBuffer.Clear();

                    int2 transformint2 = new int2((int)transform.ValueRO.Position.x, (int)transform.ValueRO.Position.z);
                    int2 endPostint2 = new int2((int)Destinations[rngNum].Node.WorldPos.x, (int)Destinations[rngNum].Node.WorldPos.z);
                    while (transformint2.x == endPostint2.x && transformint2.y == endPostint2.y)
                    {
                        // seed = math.asuint(entity.Index * 73856093 ^ entity.Version * 19349663 ^ time) + 1;
                        // rng = new Random(seed);
                        rngNum = rng.NextInt(0, Destinations.Length);
                        endPostint2 = new int2((int)Destinations[rngNum].Node.WorldPos.x, (int)Destinations[rngNum].Node.WorldPos.z);
                    }

                    ecb.AddComponent(entity, new PathRequest()
                    {
                        Start = new int2((int)transform.ValueRO.Position.x, (int)transform.ValueRO.Position.z),
                        End = new int2((int)Destinations[rngNum].Node.WorldPos.x, (int)Destinations[rngNum].Node.WorldPos.z)
                    });
                    //Debug.Log($"{new int2((int)transform.ValueRO.Position.x, (int)transform.ValueRO.Position.z)}  {new int2((int)Destinations[rngNum].Node.WorldPos.x, (int)Destinations[rngNum].Node.WorldPos.z)}");
                    NPCData data = npcData.ValueRO;
                    data.E_NPCState = NPCState.None;
                    ecb.SetComponent(entity, data);
                    break;
                case NPCState.Idle:
                    break;
                case NPCState.Move:

                    #region 이동 버퍼 체크 및 처리

                    var pathBuffer = SystemAPI.GetBuffer<MoveNode>(entity);
                    if (pathBuffer.Length == 0)
                        return;

                    #endregion

                    #region 도착 처리

                    if (npcData.ValueRO.CurrentPathIndex >= pathBuffer.Length)
                    {
                        transform.ValueRW.Position = npcData.ValueRO.NPCMoveData.TargetPosition;
                        npcData.ValueRW.E_NPCState = NPCState.None;
                        continue; // 도착 완료
                    }

                    #endregion

                    // 회피 코드
                    // 그리드 영역 주변 가져오기
                    var neighborNPC = new NativeList<NPCData>(Allocator.TempJob);
                    var neighborNPCTransform = new NativeList<LocalTransform>(Allocator.TempJob);
                    var neighborEntities = new NativeList<Entity>(Allocator.TempJob);
                    var resultVelocities = new NativeArray<float3>(1, Allocator.TempJob);

                    int neighborDistance = npcData.ValueRO.NPCMoveData.neightborDistance;
                    for (int x = -neighborDistance; x <= neighborDistance; x++)
                    {
                        for (int y = -neighborDistance; y <= neighborDistance; y++)
                        {
                            int2 myPos = new int2((int)transform.ValueRO.Position.x, (int)transform.ValueRO.Position.z);
                            int2 otherPos = myPos + new int2(x, y);
                            if (otherPos.x >= 0 && otherPos.x <= 100 && otherPos.y >= 0 && otherPos.y <= 100)
                            {
                                var navnode = NodeGroups[Index(otherPos, 100)].Entity;
                                var innerBuffer = SystemAPI.GetBuffer<NavNodeInnerEntity>(navnode);
                                foreach (var npc in innerBuffer)
                                {
                                    if(entity == npc.Entity) continue;
                                    var neighborNpcData = state.EntityManager.GetComponentData<NPCData>(npc.Entity);
                                    var neighborTr = state.EntityManager.GetComponentData<LocalTransform>(npc.Entity);
                                    neighborEntities.Add(npc.Entity);
                                    neighborNPC.Add(neighborNpcData);
                                    neighborNPCTransform.Add(neighborTr);
                                }
                            }
                        }
                    }

                    //float3 resultVelocities = 0;
                     // 회피
                     var job = new RVOJob
                     {
                         NPC = npcData.ValueRO,
                         LocalTransform =transform.ValueRO,
                         Entity = entity,
                         NPCs = neighborNPC,
                         Transforms = neighborNPCTransform,
                         Entities = neighborEntities,
                         ResultVelocity = resultVelocities,
                         DeltaTime = dt
                     };
                    
                    
                     var handle = job.Schedule();
                     handle.Complete();
                    
                    float3 nextPos = float3.zero;
                    float3 currentPos = transform.ValueRO.Position;
                    float3 targetPos = npcData.ValueRO.NPCMoveData.TargetPosition;
                    float3 targetDir = math.normalizesafe(npcData.ValueRO.NPCMoveData.TargetPosition - transform.ValueRO.Position);
                    
                    if (neighborEntities.Length > 0)
                    {
                        //Debug.Log($"<color=cyan>{neighborEntities.Length}</color>");
                        nextPos = resultVelocities[0] * dt * npcData.ValueRO.NPCMoveData.MoveSpeed;
                        //Debug.Log($"{nextPos}");
                    }
                    else
                    {
                        //Debug.Log($"<color=red>{neighborEntities.Length}</color>");
                        nextPos = targetDir * npcData.ValueRO.NPCMoveData.MoveSpeed * Time.deltaTime;
                        //Debug.Log($"<color=red>{nextPos}</color>");
                    }
                    
                    
                    if (math.distance(currentPos+nextPos, targetPos) < 0.05f)
                    {
                        //Debug.Log("이동");
                        npcData.ValueRW.CurrentPathIndex++;
                        if(npcData.ValueRO.CurrentPathIndex < pathBuffer.Length)
                            npcData.ValueRW.NPCMoveData.TargetPosition = pathBuffer[npcData.ValueRO.CurrentPathIndex].WorldPos;
                    }
                    
                    
                    //Debug.Log($"{resultVelocities[0]}");
                    // float3 currentPos = transform.ValueRO.Position;
                    // float speed = npcData.ValueRO.NPCMoveData.MoveSpeed;
                    // float3 nextPos = 0;
                    // nextPos = currentPos + resultVelocities[0] * speed * dt;
                    // if (neighborEntities.Length == 0)
                    // {
                    //     float3 targetPos = pathBuffer[npcData.ValueRO.CurrentPathIndex].WorldPos;
                    //     //float3 targetPos = npcData.ValueRO.NPCMoveData.TargetPosition;
                    //
                    //     float3 dir = targetPos - currentPos;
                    //
                    //     // 이동 코드
                    //     float lenSq = math.lengthsq(dir);
                    //     float3 direction = lenSq > 0.0001f ? math.normalize(dir) : float3.zero;
                    //     
                    //     //Debug.Log($"{resultVelocities}");
                    //     nextPos = currentPos + direction * speed * dt;
                    //     //Debug.Log($"{nextPos}");
                    //     if (math.distance(nextPos, targetPos) < 0.1f)
                    //     {
                    //         //Debug.Log("이동");
                    //         nextPos = targetPos;
                    //         npcData.ValueRW.CurrentPathIndex++;
                    //         if(npcData.ValueRO.CurrentPathIndex < pathBuffer.Length)
                    //             npcData.ValueRW.NPCMoveData.TargetPosition = pathBuffer[npcData.ValueRO.CurrentPathIndex].WorldPos;
                    //     }
                    //     
                    //     npcData.ValueRW.NPCMoveData.MoveVelocity = direction;
                    // }
                    // if(npcData.ValueRO.NPCMoveData.MoveWeight == 0)
                    //     Debug.Log($"<color=red>{nextPos}</color>");
                    // if(npcData.ValueRO.NPCMoveData.MoveWeight == 1)
                    //     Debug.Log($"<color=cyan>{nextPos}</color>");

                    #region 셀 이동 처리

                    var myindex = Index(new int2((int)transform.ValueRO.Position.x, (int)transform.ValueRO.Position.z), 100);
                    transform.ValueRW.Position += nextPos;
                    var newPosIndex = Index(new int2((int)transform.ValueRO.Position.x, (int)transform.ValueRO.Position.z), 100);
                    if (myindex != newPosIndex)
                    {
                        var exnode = NodeGroups[myindex].Entity;
                        var newnode = NodeGroups[newPosIndex].Entity;
                        var exInnerBuffer = SystemAPI.GetBuffer<NavNodeInnerEntity>(exnode);
                        var newInnerBuffer = SystemAPI.GetBuffer<NavNodeInnerEntity>(newnode);

                        for (int i = 0; i < exInnerBuffer.Length; i++)
                        {
                            if (exInnerBuffer[i].Entity.Equals(entity))
                            {
                                exInnerBuffer.RemoveAt(i);
                                break;
                            }
                        }

                        newInnerBuffer.Add(new NavNodeInnerEntity() { Entity = entity });
                    }

                    #endregion
                    Debug.DrawLine(currentPos, currentPos + resultVelocities[0] * 0.5f, Color.red, 0.1f);
                    neighborNPC.Dispose();
                    neighborNPCTransform.Dispose();
                    neighborEntities.Dispose();
                    resultVelocities.Dispose();
                    //NodeGroups[Index(new int2((int)transform.ValueRO.Position.x, (int)transform.ValueRO.Position.z), 100)].NpcData.Add(npcData.ValueRO);
                    break;
                case NPCState.Talk:
                    break;
                case NPCState.Drink:
                    break;
                case NPCState.Smoke:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        //state.Enabled = false;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    public static int Index(int2 pos, int2 gridSize) => pos.x * gridSize.x + pos.y;

    //TODO : [이거 단일객체가 다중객체 검사하도록 수정 필요] @장세현   2025년 6월 17일 화요일 

    //[BurstCompile]
    public struct RVOJob : IJob
    {
        [ReadOnly] public NPCData NPC;
        [ReadOnly] public LocalTransform LocalTransform;
        [ReadOnly] public Entity Entity;
        
        [ReadOnly] public NativeArray<NPCData> NPCs;
        [ReadOnly] public NativeArray<LocalTransform> Transforms;
        [ReadOnly] public NativeArray<Entity> Entities;
        public NativeArray<float3> ResultVelocity;
        public float DeltaTime;

        public void Execute()
        {
            // 내 위치
            float3 myPos = LocalTransform.Position;
            // 내 타겟 방향
            float3 targetDir = math.normalizesafe(NPC.NPCMoveData.TargetPosition - myPos);
            // 이상적인 속도
            float3 preferredVelocity = targetDir * NPC.NPCMoveData.MoveSpeed;
            // 같은 방향 회피 적용
            preferredVelocity = AvoidCrowdingSameDirection(
                 myPos, preferredVelocity, NPC, NPCs, Transforms
            );
            float3 avoidance = float3.zero;
            int nearbyCount = 0;
            bool hasNeighbor = false;

            for (int i = 0; i < NPCs.Length; i++)
            {
                float3 otherPos = Transforms[i].Position;
                float dist = math.distance(myPos, otherPos);
                // ✅ 주변에 이웃이 있는지 검사
                if (dist < NPC.NPCMoveData.neightborDistance)
                {
                    hasNeighbor = true;

                    float3 toOther = otherPos - myPos;
                    float3 otherGoalDir = math.normalizesafe(NPCs[i].NPCMoveData.TargetPosition - otherPos);
                    float alignment = math.dot(targetDir, otherGoalDir);

                    if (alignment > 0.6f && dist < 1f)
                    {
                        avoidance -= math.normalizesafe(toOther) / (dist + 0.01f);
                        nearbyCount++;
                    }
                }
            }
            // 넘 가까우면 회피 추가
            if (nearbyCount > 0)
            {
                float3 offsetDir = math.normalizesafe(targetDir + math.normalize(avoidance) * 0.5f);
                preferredVelocity = offsetDir * NPC.NPCMoveData.MoveSpeed * 0.8f;
            }
            // 최고 속력 조절
            float3 bestVelocity = preferredVelocity;
            // ✅ 이웃이 있으면 RVO 계산, 없으면 그냥 목표 방향
            if (hasNeighbor)
            {
                bestVelocity = ComputeRVOVelocity(myPos, preferredVelocity, NPC, NPCs, Transforms);
            }

            ResultVelocity[0] = math.normalizesafe(bestVelocity);
            //Debug.Log($"{ResultVelocity[0]}");
        }

        float3 ComputeRVOVelocity(
            float3 myPos, float3 preferredVelocity, NPCData myUnit,
            NativeArray<NPCData> units, NativeArray<LocalTransform> transforms
        )
        {
            float3 bestVelocity = preferredVelocity;
            float minPenalty = float.MaxValue;
            int samples = 36;

            // 이거 내 기준 원형으로 경로 겹치는 지 체크
            for (int s = 0; s < samples; s++)
            {
                float angle = (360f / samples) * s;
                float rad = math.radians(angle);
                float3 dir = new float3(math.sin(rad), 0, math.cos(rad));
                float3 sampleVel = dir * myUnit.NPCMoveData.MoveSpeed;

                // 목표 방향에서 많이 어긋날수록 페널티가 큼
                float penalty = math.lengthsq(sampleVel - preferredVelocity);

                for (int j = 0; j < units.Length; j++)
                {
                    // 다른 유닛 이동 방향과 내 이동방향의 시간 차에 따라 닿는 지 안 닿는 지 확인
                    float3 relPos = transforms[j].Position - myPos;
                    float3 vel = math.normalize(units[j].NPCMoveData.TargetPosition - transforms[j].Position) * units[j].NPCMoveData.MoveSpeed;
                    float3 relVel = sampleVel - vel;
                    // 두 유닛이 충돌한다고 간주하는 거리 기준
                    float combinedRadius = units[j].NPCMoveData.Radius * 2f;
                    // 두 유닛이 현재 속도로 갈 경우 충돌까지 걸리는 시간 (없으면 무한대 반환)
                    float ttc = ComputeTimeToCollision(relPos, relVel, combinedRadius);

                    float myWeight = myUnit.NPCMoveData.MoveWeight;
                    float otherWeight = units[j].NPCMoveData.MoveWeight;
                    float totalWeight = myWeight + otherWeight + 0.0001f; // 0 나눗셈 방지
                    float reciprocalWeight = otherWeight / totalWeight; // 내 시점에서 상대가 더 무거우면 내가 더 피해줌
                    //n초 안에 충돌할 가능성이 있는 경우, 가까울수록 큰 페널티 부여
                    if (ttc > 0 && ttc < 3f)
                    {
                        float ttcPenalty = 1.0f / ttc;
                        penalty += ttcPenalty * 10f * reciprocalWeight;
                    }
                }

                if (penalty < minPenalty)
                {
                    minPenalty = penalty;
                    bestVelocity = sampleVel;
                }
            }

            return bestVelocity;
        }

        float3 AvoidCrowdingSameDirection(
            float3 myPos,
            float3 preferredVelocity,
            NPCData myUnit,
            NativeArray<NPCData> units,
            NativeArray<LocalTransform> transforms
        )
        {
            float3 myDir = math.normalizesafe(preferredVelocity);
            float mySpeed = math.length(preferredVelocity);

            int nearbyCount = 0;
            float3 avoidance = float3.zero;

            for (int i = 0; i < units.Length; i++)
            {
                // 거리 비교
                float3 otherPos = transforms[i].Position;
                float3 toOther = otherPos - myPos;
                float dist = math.length(toOther);

                // 인식 범위 바깥이면 스킵
                if (dist > myUnit.NPCMoveData.neightborDistance) continue;

                //다른 녀석의 방향 값
                float3 otherGoalDir = math.normalizesafe(units[i].NPCMoveData.TargetPosition - otherPos);
                // 👇 같은 방향 유사도 비교 (코사인 유사도 사용)
                float alignment = math.dot(myDir, otherGoalDir);
                // 1 = 같은 방향
                // 👉 가까우면 회피 대상
                if (alignment > 0.8f && dist < 1f)
                {
                    avoidance -= math.normalizesafe(toOther) / dist;
                    nearbyCount++;
                }
            }

            // 근처에 유닛 있으면 회피 기동 추가
            if (nearbyCount > 0)
            {
                float3 offsetDir = math.normalizesafe(myDir + math.normalizesafe(avoidance) * 0.5f);
                float reducedSpeed = mySpeed * 0.8f;
                return offsetDir * reducedSpeed;
            }

            return preferredVelocity;
        }

        float ComputeTimeToCollision(float3 relPos, float3 relVel, float combinedRadius)
        {
            // 상대 속도가 0이면 움직이지 않으므로 충돌 없음 → 무한대 반환
            float a = math.dot(relVel, relVel);
            if (a == 0) return float.MaxValue;

            //상대 위치와 상대 속도의 내적
            float b = math.dot(relPos, relVel);
            // 현재 거리의 제곱에서 반지름 합 제곱을 뺀 것 → 충돌 여부에 영향을 줌
            float c = math.dot(relPos, relPos) - combinedRadius * combinedRadius;
            // 이 값이 음수이면 해 없음 → 충돌 안 함
            float discr = b * b - a * c;

            //음수 → 루트 안이 음수 → 현실 해 없음 → 충돌 없음
            if (discr < 0) return float.MaxValue;
            float t = (b - math.sqrt(discr)) / a;
            return t < 0 ? float.MaxValue : t;
        }
    }
}