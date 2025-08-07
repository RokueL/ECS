using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(MapBuildSystemGroup))]

//[UpdateAfter(typeof(InputSystem))]
partial struct MapBuildSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapBuildData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state )//, Entity entity
    {
        // var entityManager = state.EntityManager;
        // if (entityManager.Exists(entity))
        // {
        //     var component = entityManager.GetComponentData<MapBuildData>(entity);
        //     
        //
        //     var cellCreationSystem = state.World.GetExistingSystemManaged<CellBufferManagerBase>();
        //
        // }
        // 1) GridSize 컴포넌트 있는 엔티티 쿼리

    }

    //
    // private void MakeMap(Entity entity, EntityManager entityManager)
    // {
    //     MapRandomFill(entity,entityManager);
    //     //cellBufferManager.SwapBuffer();
    //
    //     for (int i = 0; i < entity.SmoothNum; i++)
    //     {
    //         SmoothMap(entity,entityManager);
    //        // cellBufferManager.SwapBuffer();
    //     }
    //     
    //     // cellBufferManager.MakeAllDirty();
    //     // OnDrawTile();
    //     // currentStepId++;
    // }
    //
    //
    // /// <summary>
    // /// 비율에 따라 맵의 셀을 채웁니다. 
    // /// </summary>
    // private void MapRandomFill(Entity entity, EntityManager entityManager)
    // {
    //     var mapBuildData = entityManager.GetComponentData<MapBuildData>(entity);
    //     if (mapBuildData.UseRandomSeed)
    //     {
    //         mapBuildData.Seed =  (ulong)DateTime.Now.Ticks; //시드
    //         entityManager.SetComponentData(entity, mapBuildData);   
    //     }
    //
    //     System.Random pseudoRandom = new System.Random(mapBuildData.GetHashCode()); //시드로 부터 의사 난수 생성
    //
    //     for (int x = 0; x < mapBuildData.Width; x++) {
    //         for (int y = 0; y < mapBuildData.Height; y++)
    //         {
    //             ref Cell writeCell = ref cellBufferManager.GetRefWriteCell(x,y);
    //             writeCell.Pos = new int2(x, y);
    //
    //             // 외각 체크 벽으로 할거임 
    //             if (IsOutskirts(x,y))
    //             {
    //                 writeCell.SetElementType(ElementType.Stone);
    //                 writeCell.Amount = 1;
    //             }
    //             else
    //             {
    //                 // 랜덤하게 벽인지 또는 빈공간인지 설정 
    //                 //비율에 따라 벽 혹은 빈 공간 생성
    //                 if (pseudoRandom.Next(0, 100) < randomFillPercent)
    //                 {
    //                     writeCell.SetElementType(ElementType.Stone);
    //                     writeCell.Amount = 1;
    //                 }
    //             }
    //             cellBufferManager.MarkDirtyForMainThread(writeCell.Pos);
    //         }
    //     }
    // }
    //
    // /// <summary>
    // /// 헤딩 값이 외곽인지
    // /// </summary>
    // /// <param name="x"> x 좌표 </param>
    // /// <param name="y"> y 좌표 </param>
    // /// <returns></returns>
    // private bool IsOutskirts(int x, int y)
    // {
    //     if (cellBufferManager.CheckOutOfIndex(x,y))
    //     {
    //         throw new Exception($"CellBuffer.IsOutskirts에서 인덱스를 벗어남\n" +
    //                             $"인자 X : {x} / 넓이 {width} \n" +
    //                             $"인자 Y : {y} / 높이 {height}"
    //         );
    //     }
    //     else
    //     {
    //         return (x == 0 || x == width - 1 || y == 0 || y == height - 1);
    //     }
    // }
    //
    // /// <summary>
    // /// 셀룰러 오토마타의 방식으로 맵을 부드럽게 만듭니다.
    // /// </summary>
    // private void SmoothMap(MapBuildData entity, EntityManager entityManager)
    // {
    //     for (int x = 0; x < width; x++) {
    //         for (int y = 0; y < height; y++) {
    //             int neighbourWallTiles = GetSurroundElementCount(x, y,ElementType.Stone);
    //             
    //             ref Cell cell = ref cellBufferManager.GetRefWriteCell(x, y);
    //             
    //             if (IsOutskirts(x, y))
    //                 continue;
    //             
    //             //주변 칸 중 벽이 4칸을 초과할 경우 현재 타일을 벽으로 바꿈
    //             if (neighbourWallTiles > 4)
    //             {
    //                 cell.SetElementType(ElementType.Stone);
    //                 cell.Amount = 1;
    //             }
    //             //주변 칸 중 벽이 4칸 미만일 경우 현재 타일을 빈 공간으로 바꿈
    //             else if (neighbourWallTiles < 4)
    //             {
    //                 cell.SetElementType( ElementType.Empty);
    //                 cell.Amount = 0;
    //             }
    //             cellBufferManager.MarkDirtyForMainThread(cell.Pos);
    //         }
    //     }
    // }

}
