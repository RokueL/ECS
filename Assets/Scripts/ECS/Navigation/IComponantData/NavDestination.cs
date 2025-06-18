using Unity.Entities;
using Unity.Mathematics;

public struct NavDestination : IBufferElementData
{
    public Entity Entity;
    public NavNode Node;
}