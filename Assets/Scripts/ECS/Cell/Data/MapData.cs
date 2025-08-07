using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


public struct MapData : IComponentData
{
    public int Height;
    public int Width;
    public int MapChance;
    public ulong Seed;
    public Entity CellPrefab;
}

public struct CellGroupData : IBufferElementData
{
    public Entity CellEntity;
}
