using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEditor;
using UnityEngine;


[Flags]
public enum CellVisualType : byte
{
    Empty = 0,
    Wall = 1 << 0,
    Water = 1 << 1,
}

public struct CellDataJSH : IComponentData
{
    public CellVisualType CellVisualType;
    public int2 Postion;

    public int Amount;
}

[MaterialProperty("TestColor")]
public struct ColorOverrid : IComponentData
{
    public float4 Value;
}

public struct WaterTag : IComponentData{}
