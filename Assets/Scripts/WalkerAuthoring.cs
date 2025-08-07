using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Walker : IComponentData
{
    public float ForwardSpeed;
    public float AngularSpeed;
    
    public static Walker Create(ref Unity.Mathematics.Random random)
    {
        return new Walker()
        {
            ForwardSpeed = random.NextFloat(0.1f, 0.8f),
            AngularSpeed = random.NextFloat(0.5f, 4f),
        };
    }
}

public class WalkerAuthoring : MonoBehaviour
{
    public float _forwardSpeed = 1;
    public float _angularSpeed = 1;

    class Baker : Baker<WalkerAuthoring>
    {
        public override void Bake(WalkerAuthoring src)
        {
            var data = new Walker()
            {
                ForwardSpeed = src._forwardSpeed,
                AngularSpeed = src._angularSpeed
            };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), data);
        }
    }
}
