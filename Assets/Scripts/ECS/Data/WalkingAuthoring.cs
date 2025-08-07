using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public struct MoveStruct : IComponentData
{
    public float MoveSpeedValue;
}


public class WalkingAuthoring : MonoBehaviour
{
    public float _MoveSPeedValue = 1f;
    class Baker : Baker<WalkingAuthoring>
    {
        public override void Bake(WalkingAuthoring src)
        {
            var data = new MoveStruct()
            {
                MoveSpeedValue = src._MoveSPeedValue
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), data);
        }
    }
}