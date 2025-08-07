using System;
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


public class EnviormentAuthoring : MonoBehaviour
{
    public EnviormentType EnviormentType;
    class Baker : Baker<EnviormentAuthoring>
    {
        public override void Bake(EnviormentAuthoring src)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            EnviormentTag data = new EnviormentTag()
            {
                EnviormentType = EnviormentType.None
            };
            switch (src.EnviormentType)
            {
                case EnviormentType.None:
                    break;
                case EnviormentType.Ground:
                    data.EnviormentType = EnviormentType.Ground;
                    break;
                case EnviormentType.Stairs:
                    break;
            }
            AddComponent(entity, data);
        }
    }
}