using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct SequenceData : IComponentData
{
}

public struct MapInitTag : IComponentData{}
public struct MapSmoothingTag : IComponentData{}
public struct MapDoneTag : IComponentData{}
