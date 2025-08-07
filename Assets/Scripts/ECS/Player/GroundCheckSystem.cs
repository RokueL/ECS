using Unity.Entities;
using Unity.Physics.Stateful;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;

public partial struct GroundTriggerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (triggerBuffer, entity) in SystemAPI
                     .Query<DynamicBuffer<StatefulTriggerEvent>>()
                     .WithAll<PlayerFootTag>()
                     .WithEntityAccess())
        {
            bool isGrounded = false;
            foreach (var triggerEvent in triggerBuffer)
            {
                if ((triggerEvent.State == StatefulEventState.Stay || triggerEvent.State == StatefulEventState.Enter) &&
                     SystemAPI.HasComponent<EnviormentTag>(triggerEvent.GetOtherEntity(entity)))
                {
                    Debug.Log($"{triggerEvent.GetOtherEntity(entity).ToString()}");
                    // 바닥이라고 판단할 수 있는 조건
                    if (triggerEvent.GetOtherEntity(entity) != entity)
                    {
                        Debug.Log("<color=red> true </color>");
                        isGrounded = true;
                        break;
                    }
                }
            }

            // 부모(플레이어)에 JumpData 수정
            var parent = SystemAPI.GetComponent<Parent>(entity).Value;
            if (SystemAPI.HasComponent<JumpData>(parent))
            {
                var jumpData = SystemAPI.GetComponentRW<JumpData>(parent);
                if(jumpData.ValueRO.GroundIgnoreTime <= 0)
                    jumpData.ValueRW.IsGrounded = isGrounded;
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}