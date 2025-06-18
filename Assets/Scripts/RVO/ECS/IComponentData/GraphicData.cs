using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public enum MeshType : byte
{
    E_Capsule = 0,
    E_Cube = 1 << 0,
    E_Circle = 1 << 1,
    
}

public enum MaterialType : byte
{
    E_Normal = 0,
    E_Normal2 = 1 << 0,
    E_Normal3 = 1 << 1,
    
}

public class GraphicData : IComponentData
{
    public List<Material> material;
    public List<Mesh> mesh;
}

public struct GraphicTag : IComponentData{}