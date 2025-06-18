using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Flags]
public enum EnviormentType : byte
{
    None = 0,
    Ground = 1 << 0,
    Stairs = 1 << 1,
}


public struct EnviormentTag : IComponentData
{
    public EnviormentType EnviormentType;
}
