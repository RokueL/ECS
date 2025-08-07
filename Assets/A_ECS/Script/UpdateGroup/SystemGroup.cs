using Unity.Burst;
using Unity.Entities;
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class  MapBuildSystemGroup : ComponentSystemGroup
{
}


[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class  InitializeSystemGroup : ComponentSystemGroup
{
}
