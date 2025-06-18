using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public partial struct WalkingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        
    }
    
    public void OnUpdate(ref SystemState state)
    {
        //  델타 타임
        var dt = SystemAPI.Time.DeltaTime;
        
        // 해당 struct를 가진 엔티티를 찾아 작업 쿼리에 넣어준다.
        foreach (var (walker, xform) in
                 SystemAPI.Query<RefRO<MoveStruct>,
                     RefRW<LocalTransform>>())
        {
            if(Input.GetKeyDown(KeyCode.W))
                xform.ValueRW.Position.z += walker.ValueRO.MoveSpeedValue * dt;
            if(Input.GetKeyDown(KeyCode.S))
                xform.ValueRW.Position.z -= walker.ValueRO.MoveSpeedValue * dt;
            if(Input.GetKeyDown(KeyCode.A))
                xform.ValueRW.Position.x += walker.ValueRO.MoveSpeedValue * dt;
            if(Input.GetKeyDown(KeyCode.D))
                xform.ValueRW.Position.x += walker.ValueRO.MoveSpeedValue * dt;
        }
    }
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
