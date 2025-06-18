using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Unity.Entities.Graphics;
using Unity.Mathematics;

public class CellAuthoring : MonoBehaviour
{
    public CellVisualType CellType = CellVisualType.Empty;
    
     class CellBaker : Baker<CellAuthoring>
    {
        public override void Bake(CellAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            
            var data = new CellDataJSH()
            {
                CellVisualType = CellVisualType.Empty,
                Amount = 0,
            };
            
            AddComponent(entity, new ColorOverrid()
            {
                Value = new float4(0,0,0,1)
            });

            AddComponent<URPMaterialPropertyBaseColor>(entity); // 인스턴싱 색상 속성
            
            AddComponent(entity, data);
            
        }
    }
}
