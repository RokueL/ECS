using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;
using Unity.Physics.Stateful;


public class PlayerGroundTriggerAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlayerGroundTriggerAuthoring>
    {
        public override void Bake(PlayerGroundTriggerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<PlayerFootTag>(entity);
            AddBuffer<StatefulTriggerEvent>(entity); // Trigger 이벤트 감지를 위해 필수
        }
    }
}