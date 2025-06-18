using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class MapAuthoring : MonoBehaviour
{
    public int Height = 100;
    public int Width = 100;
    public int MapChance = 40;
    public bool IsRandom = false;
    public ulong Seed;
    public GameObject CellPrefab;
    
    
    class MapBaker : Baker<MapAuthoring>
    {
        public override void Bake(MapAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            if (authoring.IsRandom)
                authoring.Seed = ulong.Parse(DateTime.Now.Ticks.ToString());
            else
                authoring.Seed = authoring.Seed;
            var data = new MapData()
            {
                Height = authoring.Height,
                Width = authoring.Width,
                MapChance = authoring.MapChance,
                Seed = authoring.Seed,
                CellPrefab = GetEntity(authoring.CellPrefab, TransformUsageFlags.None)
            };
            
            
            AddComponent(entity, data);
            AddBuffer<CellGroupData>(entity);
            AddComponent<MapInitTag>(entity);
        }
    }
}

public static class CellIndexFinder
{
    public static int GetIndex(int x, int y, int width)
    {
        return x * width + y;
    }
}
