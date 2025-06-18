using Unity.Entities;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Editor)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(NavNodeConnectionSystem))]
public partial struct NavNodeGizmoSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // var query = SystemAPI.QueryBuilder().WithAll<NavNode>().Build();
        // var nodeLookup = SystemAPI.GetComponentLookup<NavNode>(true);
        //
        // foreach (var (node, entity) in SystemAPI.Query<RefRO<NavNode>>().WithEntityAccess())
        // {
        //     Gizmos.color = node.ValueRO.Walkable ? new Color(255f,0f,255f,120f) : Color.red;
        //     Gizmos.DrawCube(node.ValueRO.WorldPos, new Vector3(0.9f, 0.1f, 0.9f));
        // }
    }
}