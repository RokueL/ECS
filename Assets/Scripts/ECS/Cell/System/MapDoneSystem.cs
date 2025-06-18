using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
public partial struct MapDoneSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapDoneTag>(); // InitTag가 있을 때만 작동
    }

    public void OnUpdate(ref SystemState state)
    {
        // var job = new WaveColorJob
        // {
        //     Time = (float)SystemAPI.Time.ElapsedTime,
        //     WaveSpeed = 0.8f,
        //     WaveLength = 0.01f
        // };
        //
        // job.ScheduleParallel();
    }
    
    /// <summary>
    /// HSV(H=0~1, S=1, V=1)를 RGB로 변환 (rainbow 효과용)
    /// </summary>
    private static float3 HueToRGB(float h)
    {
        float r = math.abs(h * 6f - 3f) - 1f;
        float g = 2f - math.abs(h * 6f - 2f);
        float b = 2f - math.abs(h * 6f - 4f);
        return math.clamp(new float3(r, g, b), 0f, 1f);
    }
    
    [BurstCompile]
    public partial struct WaveColorJob : IJobEntity
    {
        public float Time;
        public float WaveSpeed;
        public float WaveLength;

        public void Execute(ref ColorOverrid color, in CellDataJSH cell, in LocalTransform transform)
        {
            if ((cell.CellVisualType & CellVisualType.Wall) != 0) return;

            int2 pos = (int2)transform.Position.xy;

            float hue = math.frac(math.sin(Time * WaveSpeed + math.dot(pos, new float2(1f, 1f)) * WaveLength) * 0.5f + 0.5f);
            float3 rgb = HueToRGB(hue);
            color.Value = new float4(rgb, 1f);
        }

        /// <summary>
        /// HSV(H=0~1, S=1, V=1)를 RGB로 변환 (rainbow 효과용)
        /// </summary>
        private static float3 HueToRGB(float h)
        {
            float r = math.abs(h * 6f - 3f) - 1f;
            float g = 2f - math.abs(h * 6f - 2f);
            float b = 2f - math.abs(h * 6f - 4f);
            return math.clamp(new float3(r, g, b), 0f, 1f);
        }
    }
}

