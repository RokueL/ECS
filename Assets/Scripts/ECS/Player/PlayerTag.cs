using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct MoveSpeed : IComponentData
{
    public float Value;
}

public struct RotateSpeed : IComponentData
{
    public float Value;
}

public struct JumpData : IComponentData
{
    public float JumpForce;
    public bool IsGrounded;
    public float GroundIgnoreTime; // 점프 직후 바닥 무시용 타이머
    public PhysicsVelocity PhysicsVelocity;
}

public struct Gravity : IComponentData
{
    public float Value;
}

public struct Velocity : IComponentData
{
    public float3 Value;
}

public struct PlayerTag : IComponentData
{
}


public struct PlayerFootTag : IComponentData {}
