using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Flags]
public enum PlayerFSMFlags : byte
{
    None = 0,
    Idle = 1 << 0,
    Move = 1 << 1,
    Attack = 1 << 2,
    Die = 1 << 3
}

public struct PlayerFSM : IComponentData
{
    public PlayerFSMFlags PlayerFsmFlags;
}
