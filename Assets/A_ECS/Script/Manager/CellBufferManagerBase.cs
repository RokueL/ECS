using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

partial class CellBufferManagerBase : SystemBase
{
    public NativeArray<Entity> Buffer;

    protected override void OnCreate()
    {
        base.OnCreate();
        Enabled = false;
    }

    protected override void OnUpdate()
    {
      
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Buffer.IsCreated)
            Buffer.Dispose();
    }
}
