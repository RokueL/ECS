using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 15f;
    public float jumpForce = 8f;
    public float gravity = -9.81f;
    public PlayerFSMFlags state;
}

public class PlayerBaker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new MoveSpeed { Value = authoring.moveSpeed });
        AddComponent(entity, new RotateSpeed() { Value = authoring.rotateSpeed });
        AddComponent(entity, new JumpData { JumpForce = authoring.jumpForce, IsGrounded = false });
        AddComponent(entity, new Gravity { Value = authoring.gravity });
        AddComponent(entity, new PlayerFSM() { PlayerFsmFlags = authoring.state });
        AddComponent<Velocity>(entity); // 초기 속도 0
        AddComponent<PlayerTag>(entity);
    }
}