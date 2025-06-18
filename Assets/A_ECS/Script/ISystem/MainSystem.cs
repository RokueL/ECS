
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

partial struct MainSystem : ISystem
{
    private InitializeSystem initializeSystemRef;
    
    public void OnCreate(ref SystemState state)
    {


    }

    public void OnUpdate(ref SystemState state)
    {

        var world = state.WorldUnmanaged;
        var initializeSystem=   world.GetExistingUnmanagedSystem<InitializeSystem>(); 
        initializeSystemRef =   world.GetUnsafeSystemRef<InitializeSystem>(initializeSystem);
        if(!initializeSystemRef.RenderAdded)
            initializeSystem.Update(world);
        else
        {
            Debug.Log("렌더 완");
        }

       
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
