using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        // 카메라 방향 가져오기
        if (!SystemAPI.HasSingleton<CameraDirection>())
            return;

        var camDir = SystemAPI.GetSingleton<CameraDirection>();

        float3 camForward = camDir.Forward;
        float3 camRight = camDir.Right;
        camForward.y = 0;
        camRight.y = 0;
        camForward = math.normalize(camForward);
        camRight = math.normalize(camRight);

        float3 input = float3.zero;
        if (Input.GetKey(KeyCode.W)) input += camForward;
        if (Input.GetKey(KeyCode.S)) input -= camForward;
        if (Input.GetKey(KeyCode.A)) input -= camRight;
        if (Input.GetKey(KeyCode.D)) input += camRight;

        if (!input.Equals(float3.zero))
            input = math.normalize(input);

        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        foreach (var (speed, rotateSpeed, jump, gravity, velocity, transform, fsm) in
                 SystemAPI.Query<RefRO<MoveSpeed>, RefRO<RotateSpeed>, RefRW<JumpData>, RefRO<Gravity>, RefRW<Velocity>, RefRW<LocalTransform>, RefRO<PlayerFSM>>()
                     .WithAll<PlayerTag>())
        {
            switch (fsm.ValueRO.PlayerFsmFlags)
            {
                case PlayerFSMFlags.None:
                case PlayerFSMFlags.Idle:
                case PlayerFSMFlags.Attack:
                case PlayerFSMFlags.Die:
                    continue;
            }

            float3 vel = velocity.ValueRW.Value;
            float rotateS = rotateSpeed.ValueRO.Value;

            // 수평 이동
            float3 move = input * speed.ValueRO.Value;
            vel.x = move.x;
            vel.z = move.z;

            // 중력 적용
            vel.y += gravity.ValueRO.Value * deltaTime;

            // 점프
            if (jump.ValueRW.IsGrounded && jumpPressed)
            {
                vel.y = jump.ValueRW.JumpForce;
                jump.ValueRW.IsGrounded = false;
                jump.ValueRW.GroundIgnoreTime = 0.15f; // 150ms 정도는 바닥 무시
            }

            if (jump.ValueRW.GroundIgnoreTime > 0)
            {
                jump.ValueRW.GroundIgnoreTime -= deltaTime;
            }

            // Ray 설정
            float3 position = transform.ValueRW.Position;
            RaycastInput rayInput = new RaycastInput
            {
                Start = position + new float3(0, -transform.ValueRO.Scale/2f, 0), // 살짝 위에서 시작
                End = position + new float3(0, -transform.ValueRO.Scale/2f - 0.1f, 0), // 아래로 1.5m 쏨
                Filter = CollisionFilter.Default
            };
            Debug.DrawLine(rayInput.Start, rayInput.End, Color.red, 0.1f);
    
            // Ground check
            if (physicsWorld.CastRay(rayInput, out Unity.Physics.RaycastHit hit) && jump.ValueRO.GroundIgnoreTime <= 0 && !jump.ValueRO.IsGrounded)
            {
                Entity hitEntity = hit.Entity;

                if (SystemAPI.HasComponent<EnviormentTag>(hitEntity))
                {
                    var env = SystemAPI.GetComponent<EnviormentTag>(hitEntity);
                    switch (env.EnviormentType)
                    {
                        case EnviormentType.None:
                        case EnviormentType.Stairs:
                            break;
                        case EnviormentType.Ground:
                            Debug.Log(rayInput);
                            Debug.Log(hitEntity);
                            //Debug.Log(rayInput.Start);
                            //Debug.Log(rayInput.End);
                            float3 hitPoint = hit.Position;
                            Debug.Log(hitPoint);

                            // 현재 위치를 x,z는 유지하고 y만 스냅
                            transform.ValueRW.Position = new float3(position.x, hitPoint.y + transform.ValueRO.Scale / 2, position.z);

                            jump.ValueRW.IsGrounded = true;
                            break;
                        default:
                            jump.ValueRW.IsGrounded = false;
                            break;
                    }
                }
            }

            // 이동 적용
            if (jump.ValueRO.IsGrounded)
                vel.y = 0f;
            transform.ValueRW.Position += vel * deltaTime;
            velocity.ValueRW.Value = vel;


            // 회전 적용 (카메라 기준 이동 방향 바라보게)
            if (!input.Equals(float3.zero))
            {
                quaternion currentRot = transform.ValueRW.Rotation;
                quaternion targetRot = quaternion.LookRotationSafe(input, math.up());
                transform.ValueRW.Rotation = math.slerp(currentRot, targetRot, deltaTime * rotateS);
            }
        }
    }
}