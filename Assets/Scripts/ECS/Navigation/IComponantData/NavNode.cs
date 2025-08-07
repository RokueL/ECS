using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct NavNode : IComponentData
{
    public Entity Entity;
    public int2 GridPos;
    public float3 WorldPos;
    public bool Walkable;
    public Entity exNode;

    public NavNode(NavNode nod)
    {
        this.Entity = nod.Entity;
        this.GridPos = nod.GridPos;
        this.WorldPos = nod.WorldPos;
        this.Walkable = nod.Walkable;
        this.exNode = nod.exNode;
    }
}

public struct NavNodeInnerEntity : IBufferElementData
{
    public Entity Entity;
}

public struct NodeGroup : IBufferElementData
{
    public Entity Entity;
    public NavNode Node;
}

public struct NodeGroupTag : IComponentData{}
