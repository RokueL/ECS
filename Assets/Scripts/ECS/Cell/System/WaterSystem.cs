using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
partial struct WaterSystem : ISystem
{
    [Flags]
    public enum FlowDirection : byte
    {
        Down = 0,
        Right = 1 << 0,
        Left = 1 << 1,
        Up = 1 << 2,
        DownRight = 1 << 3,
        DownLeft = 1 << 4,
        UpRight = 1 << 5,
        UpLeft = 1 << 6
    }
    
    static readonly FlowDirection[] FlowDirections = new[]
    {
        FlowDirection.Down,
        FlowDirection.Right,
        FlowDirection.Left,
        FlowDirection.Up,
        FlowDirection.DownRight,
        FlowDirection.DownLeft,
        FlowDirection.UpRight,
        FlowDirection.UpLeft,
    };


    #region 물 흐름

    const int MAX_FLOW = 40000;
    const int MIN_FLOW = 500;

    const int MAX_AMOUNT = 10000;
    const int MAX_COMPRESS = 5000;
    
    const float LEFT_FLOW = 0.25f ;
    const float RIGHT_FLOW = 0.333334f;
    const float FLOW_SPEED = 0.5f;

    #endregion
    
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WaterTag>();
        state.RequireForUpdate<CellDataJSH>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entityManager = state.EntityManager;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (cell, waterTag, entity) in SystemAPI
                     .Query<RefRW<CellDataJSH>, RefRO<WaterTag>>()
                     .WithEntityAccess())
        {
            int2 cellPosition = cell.ValueRO.Postion;
            if (CheckOutOfIndex(cellPosition, 200, 200))
                continue;

            bool isFlowed = false;
            foreach (var dir in FlowDirections)
            {
                if(isFlowed)
                    break;
                var neighbor = GetNeighborCellData(entityManager, cellPosition, dir);
                var neighborCell = entityManager.GetComponentData<CellDataJSH>(neighbor);

                if (neighborCell.CellVisualType == CellVisualType.Empty || neighborCell.CellVisualType == CellVisualType.Water)
                {
                    FlowWater(ref ecb, entityManager, entity, neighbor,dir);
                    isFlowed = true;
                }
            }
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public bool CheckOutOfIndex(int2 postion, int width, int height)
        => postion.x < 0 || postion.y < 0 || postion.x >= width || postion.y >= height;
    
    [BurstCompile]
    public Entity GetNeighborCellData(EntityManager entityManager,int2 position, FlowDirection direction)
    {
        int2 neighborPos = position;
        switch (direction)
        {
            case FlowDirection.Down:
                neighborPos += new int2(0, -1);
                break;
            case FlowDirection.Right:
                neighborPos += new int2(1, 0);
                break;
            case FlowDirection.Left:
                neighborPos += new int2(-1, 0);
                break;
            case FlowDirection.Up:
                neighborPos += new int2(0, 1);
                break;
            // case FlowDirection.DownRight:
            //     neighborPos += new int2(0, -1);
            //     break;
            // case FlowDirection.DownLeft:
            //     break;
            // case FlowDirection.UpRight:
            //     break;
            // case FlowDirection.UpLeft:
            //     break;
            // default:
            //     throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
        Entity mapEntity = SystemAPI.GetSingletonEntity<MapData>();
        var mapData = SystemAPI.GetSingleton<MapData>();
        DynamicBuffer<CellGroupData> cellBuffer = entityManager.GetBuffer<CellGroupData>(mapEntity);
        
        var celldata = cellBuffer[CellIndexFinder.GetIndex(neighborPos.x, neighborPos.y, mapData.Width)];
        //var cell = entityManager.GetComponentData<CellDataJSH>(celldata.CellEntity);
        return celldata.CellEntity;
    }

    public void FlowWater(ref EntityCommandBuffer ecb, EntityManager entityManager, Entity origin, Entity neighbor, FlowDirection dir)
    {
        var originCell = entityManager.GetComponentData<CellDataJSH>(origin);
        var neighborCell = entityManager.GetComponentData<CellDataJSH>(neighbor);

        int originAmount = originCell.Amount;
        int neighborAmount = neighborCell.Amount;

        if (originAmount == 0 || neighborAmount >= MAX_AMOUNT)
            return;

        int flowAmount = 0;

        if (dir == FlowDirection.Left || dir == FlowDirection.Right)
        {
            // 수평 흐름: 양쪽 물 양을 같게 맞춤
            int total = originAmount + neighborAmount;
            int ideal = total / 2;
            flowAmount = originAmount - ideal;

            if (math.abs(flowAmount) < 200)
                return; // 너무 적게 흐르면 무시

            // Clamp
            flowAmount = math.clamp(flowAmount, -originAmount, MAX_AMOUNT - neighborAmount);
        }
        else
        {
            // 수직 흐름: 기존 단방향 로직 유지
            int originFlowAmount = (originAmount < MIN_FLOW) ? originAmount : MIN_FLOW;
            flowAmount = math.min(MAX_AMOUNT - neighborAmount, originFlowAmount);
            if (flowAmount <= 0)
                return;
        }

        // 실제 흐름 적용
        originCell.Amount -= flowAmount;
        neighborCell.Amount += flowAmount;


        // 시각 처리 및 태그 관리
        if (neighborCell.CellVisualType == CellVisualType.Empty)
        {
            neighborCell.CellVisualType = CellVisualType.Water;
            ecb.SetComponent(neighbor, new ColorOverrid { Value = new float4(0, 1, 0, 1) });
            if (!entityManager.HasComponent<WaterTag>(neighbor))
                ecb.AddComponent<WaterTag>(neighbor);
        }

        if (originCell.Amount <= 0)
        {
            originCell.Amount = 0;
            originCell.CellVisualType = CellVisualType.Empty;
            ecb.SetComponent(origin, new ColorOverrid { Value = new float4(1, 1, 1, 0) });
            ecb.RemoveComponent<WaterTag>(origin);
            originCell.CellVisualType = CellVisualType.Empty;
        }
        
        ecb.SetComponent(origin, originCell);
        ecb.SetComponent(neighbor, neighborCell);
    }
    
    
}
