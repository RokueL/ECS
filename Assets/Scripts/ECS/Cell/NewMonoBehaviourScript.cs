// using System;
// using System.Runtime.CompilerServices;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Collections.LowLevel.Unsafe;
// using Unity.Mathematics;
// using UnityEngine;
//
//
// [BurstCompile]
// public class MoveStep
// {
//     #region 물 흐름
//
//     const int MAX_FLOW = 40000;
//     const int MIN_FLOW = 500;
//
//     const int MAX_AMOUNT = 10000;
//     const int MAX_COMPRESS = 5000;
//
//     const float LEFT_FLOW = 0.25f;
//     const float RIGHT_FLOW = 0.333334f;
//     const float FLOW_SPEED = 0.5f;
//
//     #endregion
//
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     [BurstCompile]
//     static float GetFlowAmount(float from, float to, float rate)
//     {
//         // return Mathf.Max(0f, (from - to) * 0.25f); // 좌우는 느리게 흘러야 자연스러워
//         return (from - to) * rate;
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     [BurstCompile]
//     static int GetFlowAmount(int amount, float rate)
//     {
//         // return Mathf.Max(0f, (from - to) * 0.25f); // 좌우는 느리게 흘러야 자연스러워
//         return (int)((amount) * rate);
//     }
//
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     [BurstCompile]
//     static int GetStableFlowAmount(int total_mass)
//     {
//         if (total_mass <= MAX_AMOUNT)
//         {
//             return MAX_AMOUNT;
//         }
//         else if (total_mass < 2 * MAX_AMOUNT + MAX_COMPRESS)
//         {
//             return (int)(((float)MAX_AMOUNT * MAX_AMOUNT + total_mass * MAX_COMPRESS) / (MAX_AMOUNT + MAX_COMPRESS));
//         }
//         else
//         {
//             return (int)((total_mass + MAX_COMPRESS) * 0.5f);
//         }
//     }
//
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     [BurstCompile]
//     static bool CheckOutOfIndex(in int2 pos, int width, int height)
//         => pos.x < 0 || pos.y < 0 || pos.x >= width || pos.y >= height;
//
//
//     //     
//     [BurstCompile]
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static void SimulateWaterFlow(int index, in Cell readCell,
//         //ref Cell writeCell,
//         in NativeArray<Cell> readCellBuffer,
//         // ref NativeParallelHashSet<int2>.ParallelWriter nextDirtyCell,
//         ref NativeParallelHashMap<int2, FlowInfo>.ParallelWriter flowInfoMap,
//         int width,
//         int height, //int forDebugX,int forDebugY,
//         ulong stepId)
//     {
//         bool isDirty = false;
//         int remainAmount = readCell.Amount;
//         FlowInfo flowInfo = new FlowInfo();
//
//
//         for (int dirIndex = 0; dirIndex < FixedInt2Array4.Count; dirIndex++)
//         {
//             Direction dir = (Direction)dirIndex;
//             int2 neighborPos = readCell.Pos + Directions.Cardinal[dirIndex];
//
//             if (CheckOutOfIndex(neighborPos, width, height))
//                 continue;
//
//             Cell neighbor = CellBuffer.GetRef_ReadOnly(in neighborPos, width, in readCellBuffer);
//             if (neighbor.ElementType == ElementType.Stone)
//                 continue;
//             int flow = 0;
//             switch (dir)
//             {
//                 case Direction.Down:
//                     flow = GetStableFlowAmount(remainAmount + neighbor.Amount) - neighbor.Amount;
//                     break;
//                 case Direction.Right:
//                     int rightDiff = readCell.Amount - neighbor.Amount;
//                     if (rightDiff >= MIN_FLOW)
//                         flow = GetFlowAmount(rightDiff, RIGHT_FLOW);
//
//                     break;
//                 case Direction.Left:
//                     int leftDiff = readCell.Amount - neighbor.Amount;
//                     if (leftDiff >= MIN_FLOW)
//                         flow = GetFlowAmount(leftDiff, LEFT_FLOW);
//                     break;
//                 case Direction.Up:
//                     flow = remainAmount - GetStableFlowAmount(remainAmount + neighbor.Amount);
//
//                     break;
//                 // case Direction.UpRight:
//                 //     break;
//                 // case Direction.UpLeft:
//                 //     break;
//                 // case Direction.DownRight:
//                 //     break;
//                 // case Direction.DownLeft:
//                 //     break;
//                 default:
//                     throw new ArgumentOutOfRangeException();
//             }
//
//             if (flow <= 0)
//                 continue;
//
//             if (flow > MIN_FLOW && dir != Direction.Up)
//             {
//                 flow = (int)(flow * FLOW_SPEED);
//             }
//
//             flow = Math.Clamp(flow, 0, Math.Min(MAX_FLOW, remainAmount));
//
//
//             switch (dir)
//             {
//                 case Direction.Right:
//                     flowInfo.FlowAmount[(int)Direction.Right] = flow;
//                     break;
//                 case Direction.Down:
//                     flowInfo.FlowAmount[(int)Direction.Down] = flow;
//                     break;
//                 case Direction.Left:
//                     flowInfo.FlowAmount[(int)Direction.Left] = flow;
//                     break;
//                 case Direction.Up:
//                     flowInfo.FlowAmount[(int)Direction.Up] = flow;
//                     break;
//             }
//
//             remainAmount -= flow;
//             isDirty = true;
//
//             if (remainAmount <= 0f)
//                 break;
//         }
//         //
//         // //  아래로 흐름
//         // int2 neighborCellPos = readCell.Pos +Directions.Cardinal[(int)Direction.Down] ;
//         // // 내 아래 셀 
//         // ref readonly Cell readNeighborCell = ref CellBuffer.GetRef_ReadOnly(in neighborCellPos,  width, in readCellBuffer);
//         // if (!CheckOutOfIndex(in readNeighborCell.Pos, width,  height) && readNeighborCell.ElementType != ElementType.Stone)
//         // {
//         //     int flow = GetStableFlowAmount( remainAmount +  readNeighborCell.Amount) - readNeighborCell.Amount;
//         //     ; //GetFlowAmount(remainingWater, readDownCell.Amount);
//         //
//         //     if (flow> MIN_FLOW)
//         //     {
//         //         // 최소흐름 보다 빠르면 부드럽게 작업 
//         //         flow = (int)(flow* FLOW_SPEED);
//         //         // 남은 물 보다 적지만 최소 흐름보다는 크게 
//         //     }
//         //
//         //  
//         //     flow = Math.Clamp(flow, 0, Math.Min(MAX_FLOW, remainAmount));
//         //     if (flow != 0)
//         //     {
//         //         isDirty = true;
//         //         remainAmount -= flow;
//         //         flowInfo.FlowAmount[(int)Direction.Down] = flow;
//         //     }
//         // }
//         //
//         // neighborCellPos = readCell.Pos + Directions.Cardinal[(int)Direction.Left] ;
//         // readNeighborCell = ref CellBuffer.GetRef_ReadOnly(in neighborCellPos,  width, in readCellBuffer);
//         // //
//         //
//         // if (!CheckOutOfIndex(in readNeighborCell.Pos, width, height) && readNeighborCell.ElementType != ElementType.Stone)
//         // {
//         //     int diff = readCell.Amount - readNeighborCell.Amount;
//         //     if (diff >= MIN_FLOW)
//         //     {
//         //         int flow = GetFlowAmount( diff,  LEFT_FLOW);
//         //
//         //         if (flow > MIN_FLOW)
//         //         {
//         //             flow = (int)(flow * FLOW_SPEED);
//         //         }
//         //       
//         //         flow = Math.Clamp(flow, 0, Math.Min(MAX_FLOW,remainAmount));
//         //         if (flow != 0)
//         //         {
//         //             isDirty = true;
//         //             remainAmount -= flow;
//         //             flowInfo.FlowAmount[(int)Direction.Left] = flow;
//         //
//         //             // SetNeighborDirty(readNeighborCell.Pos,ref dirtyCell);
//         //         }
//         //     }
//         // }
//         //
//         // neighborCellPos = readCell.Pos  + Directions.Cardinal[(int)Direction.Right] ;
//         // readNeighborCell = ref CellBuffer.GetRef_ReadOnly(in neighborCellPos,  width,in readCellBuffer);
//         //
//         // if (!CheckOutOfIndex(in readNeighborCell.Pos,  width, height) && readNeighborCell.ElementType != ElementType.Stone)
//         // {
//         //     int diff = readCell.Amount - readNeighborCell.Amount;
//         //     if (diff >= MIN_FLOW)
//         //     {
//         //         int flow = GetFlowAmount( diff,  RIGHT_FLOW);
//         //
//         //         if (flow > MIN_FLOW)
//         //         {
//         //             flow = (int)(flow * FLOW_SPEED);
//         //         }
//         //       
//         //         flow = Math.Clamp(flow, 0, Math.Min(MAX_FLOW,remainAmount));
//         //         if (flow != 0)
//         //         {
//         //             isDirty = true;
//         //             remainAmount -= flow;
//         //             flowInfo.FlowAmount[(int)Direction.Right] = flow;
//         //
//         //             // SetNeighborDirty(readNeighborCell.Pos,ref dirtyCell);
//         //         }
//         //     }
//         //   
//         // }
//         // //
//         // //위쪽 
//         // neighborCellPos = readCell.Pos + Directions.Cardinal[(int)Direction.Up] ;
//         // readNeighborCell = ref CellBuffer.GetRef_ReadOnly(in neighborCellPos,  width, in readCellBuffer);
//         //
//         // if (!CheckOutOfIndex(in readNeighborCell.Pos,  width,  height) && readNeighborCell.ElementType != ElementType.Stone)
//         // {
//         //     int flow = remainAmount - GetStableFlowAmount(remainAmount + readNeighborCell.Amount);
//         //
//         //     if (flow >= MIN_FLOW)
//         //     {
//         //         flow = (int)(flow * FLOW_SPEED);
//         //     }
//         //
//         //     flow = Math.Clamp(flow, 0, Math.Min(MAX_FLOW, remainAmount));
//         //     if (flow != 0)
//         //     {
//         //         isDirty = true;
//         //         remainAmount -= flow;
//         //         flowInfo.FlowAmount[(int)Direction.Up] = flow;
//         //     }
//         // }
//         //
//         //
//
//         if (isDirty)
//         {
//             flowInfo.ElementType = readCell.ElementType;
//             flowInfoMap.TryAdd(readCell.Pos, flowInfo);
//         }
//     }
//
//
//     //     
//     //     //     
//     //     [BurstCompile]
//     //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     //     public static void SimulateWaterCell(int index, in Cell readCell, in NativeArray<Cell> readCellBuffer,
//     //     ref NativeQueue<Cell>.ParallelWriter writeCellBuffer,
//     //     ref NativeParallelHashSet<int2>.ParallelWriter dirtyCell,
//     //     NativeList<Cell>.ParallelWriter ass,
//     //     int width,
//     //     int height, //int forDebugX,int forDebugY,
//     //     ulong stepId)
//     // {
//     //     float remainingWater = readCell.Amount;
//     //     Cell writeCell = new Cell
//     //     {
//     //         Pos = readCell.Pos
//     //     };
//     //
//     //     // if (remainingWater < MIN_AMOUNT)
//     //     // {
//     //     //     writeCell.Amount -= remainingWater;
//     //     //     writeCell.SetElementTypeOff(ElementType.Water);
//     //     //     writeCellBuffer.Enqueue(writeCell);
//     //     //     return;
//     //     // }
//     //
//     //
//     //     //  아래로 흐름
//     //     int2 neighborCellPos = readCell.Pos + Down;
//     //     // 내 아래 셀 
//     //     ref readonly Cell readNeighborCell = ref CellBuffer.GetRef_ReadOnly(neighborCellPos, width, readCellBuffer);
//     //
//     //     
//     //     
//     //     
//     //     if (!CheckOutOfIndex(readNeighborCell.Pos, width, height) && readNeighborCell.ElementType != ElementType.Stone)
//     //     {
//     //         float flow =  GetStableFlowAmount(remainingWater + readNeighborCell.Amount) - readNeighborCell.Amount;
//     //         ; //GetFlowAmount(remainingWater, readDownCell.Amount);
//     //
//     //         if (flow > MIN_FLOW)
//     //         {
//     //             // 최소흐름 보다 빠르면 부드럽게 작업 
//     //             flow *= FLOW_SPEED;
//     //             // 남은 물 보다 적지만 최소 흐름보다는 크게 
//     //         }
//     //
//     //         flow = Math.Clamp(flow, 0f, Math.Min(MAX_FLOW, remainingWater));
//     //         if (flow != 0)
//     //         {
//     //             Cell writeNeighborCell = new Cell();
//     //             writeNeighborCell.IsDirty = true;
//     //             writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //             writeNeighborCell.Amount += flow;
//     //             writeNeighborCell.Pos = readNeighborCell.Pos;
//     //             writeCellBuffer.Enqueue(writeNeighborCell);
//     //
//     //             writeCell.IsDirty = true;
//     //             writeCell.Amount -= flow;
//     //             remainingWater -= flow;
//     //             SetNeighborDirty(readNeighborCell.Pos,ref dirtyCell);
//     //
//     //         }
//     //     }
//     //
//     //     // if (remainingWater < MIN_AMOUNT)
//     //     // {
//     //     //     writeCell.Amount -= remainingWater;
//     //     //     writeCell.IsDirty = true;
//     //     //     writeCell.SetElementTypeOff(ElementType.Water);
//     //     //     writeCellBuffer.Enqueue(writeCell);
//     //     //     return;
//     //     // }
//     //
//     //     // if ( readNeighborCell is { ElementType: ElementType.Empty, Amount: < 1 })
//     //     // {
//     //     //     
//     //     //     if (writeCell.IsDirty)
//     //     //     {
//     //     //         writeCellBuffer.Enqueue(writeCell);
//     //     //     }
//     //     //     return;
//     //     // }
//     //
//     //     neighborCellPos = readCell.Pos + Left;
//     //     readNeighborCell = ref CellBuffer.GetRef_ReadOnly(neighborCellPos, width, readCellBuffer);
//     //
//     //
//     //     if (!CheckOutOfIndex(readNeighborCell.Pos, width, height) && readNeighborCell.ElementType != ElementType.Stone)
//     //     {
//     //         float diff =readCell.Amount - readNeighborCell.Amount;
//     //         if (diff >= MIN_FLOW)
//     //         {
//     //             float flow = GetFlowAmount(diff, 0.25f);
//     //
//     //             if (flow > MIN_FLOW)
//     //             {
//     //                 flow *= FLOW_SPEED;
//     //             }
//     //
//     //             flow = Math.Clamp(flow, 0f, Mathf.Min(MAX_FLOW, remainingWater));
//     //             if (flow != 0)
//     //             {
//     //                 writeCell.IsDirty = true;
//     //                 writeCell.Amount -= flow;
//     //                 var writeNeighborCell = new Cell();
//     //                 writeNeighborCell.Amount += flow;
//     //                 writeNeighborCell.IsDirty = true;
//     //                 writeNeighborCell.Pos = readNeighborCell.Pos;
//     //                 writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //                 writeCellBuffer.Enqueue(writeNeighborCell);
//     //                 remainingWater -= flow;
//     //                 SetNeighborDirty(readNeighborCell.Pos,ref dirtyCell);
//     //             }
//     //         }
//     //     }
//     //
//     //     // if (remainingWater < MIN_AMOUNT)
//     //     // {
//     //     //     writeCell.Amount -= remainingWater;
//     //     //     writeCell.SetElementTypeOff(ElementType.Water);
//     //     //     writeCellBuffer.Enqueue(writeCell);
//     //     //     return;
//     //     // }
//     //
//     //     neighborCellPos = readCell.Pos + Right;
//     //     readNeighborCell = ref CellBuffer.GetRef_ReadOnly(neighborCellPos, width, readCellBuffer);
//     //
//     //     if (!CheckOutOfIndex(readNeighborCell.Pos, width, height) && readNeighborCell.ElementType != ElementType.Stone)
//     //     {
//     //         float diff =readCell.Amount- readNeighborCell.Amount;
//     //         if (diff >= MIN_DIFF)
//     //         {
//     //             float flow = GetFlowAmount(diff, 0.33333334f);
//     //
//     //             if (flow > MIN_FLOW)
//     //             {
//     //                 flow *= FLOW_SPEED;
//     //             }
//     //
//     //             flow = Math.Clamp(flow, 0f, Mathf.Min(MAX_FLOW, remainingWater));
//     //             if (flow != 0)
//     //             {
//     //                 writeCell.IsDirty = true;
//     //                 writeCell.Amount -= flow;
//     //                 var writeNeighborCell = new Cell();
//     //                 writeNeighborCell.Amount += flow;
//     //                 writeNeighborCell.IsDirty = true;
//     //                 writeNeighborCell.Pos = readNeighborCell.Pos;
//     //                 writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //                 writeCellBuffer.Enqueue(writeNeighborCell);
//     //                 remainingWater -= flow;
//     //                 SetNeighborDirty(readNeighborCell.Pos,ref dirtyCell);
//     //             }
//     //         }
//     //     }
//     //
//     //
//     //     // if (remainingWater < MIN_AMOUNT)
//     //     // {
//     //     //     writeCell.Amount -= remainingWater;
//     //     //     writeCell.SetElementTypeOff(ElementType.Water);
//     //     //     writeCellBuffer.Enqueue(writeCell);
//     //     //     return;
//     //     // }
//     //
//     //
//     //     //위쪽 
//     //     neighborCellPos = readCell.Pos + Up;
//     //     readNeighborCell = ref CellBuffer.GetRef_ReadOnly(neighborCellPos, width, readCellBuffer);
//     //
//     //     if (!CheckOutOfIndex(readNeighborCell.Pos, width, height) && readNeighborCell.ElementType != ElementType.Stone)
//     //     {
//     //         float flow = remainingWater - GetStableFlowAmount(remainingWater + readNeighborCell.Amount);
//     //
//     //         if (flow > MIN_FLOW)
//     //         {
//     //             flow *= FLOW_SPEED;
//     //         }
//     //
//     //         flow = Math.Clamp(flow, 0f, Math.Min(MAX_FLOW, remainingWater));
//     //         if (flow != 0)
//     //         {
//     //             writeCell.IsDirty = true;
//     //             writeCell.Amount -= flow;
//     //
//     //             var writeNeighborCell = new Cell();
//     //             writeNeighborCell.Amount += flow;
//     //             writeNeighborCell.IsDirty = true;
//     //             writeNeighborCell.Pos = readNeighborCell.Pos;
//     //             writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //             writeCellBuffer.Enqueue(writeNeighborCell);
//     //             remainingWater -= flow;
//     //             SetNeighborDirty(readNeighborCell.Pos,ref dirtyCell);
//     //         }
//     //     }
//     //
//     //     //
//     //     // if (remainingWater < MIN_AMOUNT)
//     //     // {
//     //     //     writeCell.IsDirty = true;
//     //     //     writeCell.Amount -= remainingWater;
//     //     //     writeCell.SetElementTypeOff(ElementType.Water);
//     //     // }
//     //
//     //     if (writeCell.IsDirty)
//     //     {
//     //         writeCellBuffer.Enqueue(writeCell);
//     //     }
//     // }
//     //
//     //
//     //
//     // [BurstCompile]
//     //  public static void SimulateWaterCell(int index, in  Cell readCell, in NativeArray<Cell> readCellBuffer,
//     //      ref NativeQueue<Cell>.ParallelWriter  writeCellBuffer, int width, int height, //int forDebugX,int forDebugY,
//     //      ulong stepId)
//     //  {
//     //      int x = readCell.Pos.x;
//     //      int y = readCell.Pos.y;
//     //      float remainingWater = readCell.Amount;
//     //
//     //      Cell writeCell = new Cell();
//     //      writeCell.Pos = readCell.Pos;
//     //
//     //      //  아래로 흐름
//     //      int2 neighborCell = new int2(x, y - 1);
//     //
//     //      ref Cell readNeighborCell = ref CellBuffer.GetRef(neighborCell, width, readCellBuffer);
//     //    
//     //      if (y > 0 && readNeighborCell.ElementType != ElementType.Stone)
//     //      {
//     //          float flow = GetStableFlowAmount(remainingWater + readNeighborCell.Amount) - readNeighborCell.Amount;
//     //          ; //GetFlowAmount(remainingWater, readDownCell.Amount);
//     //
//     //          if (flow > MIN_FLOW)
//     //          {
//     //              // 최소흐름 보다 빠르면 부드럽게 작업 
//     //              flow *= 0.5f;
//     //          }
//     //
//     //          // 남은 물 보다 적지만 최소 흐름보다는 크게 
//     //          flow = Math.Clamp(flow, 0f, Math.Min(MIN_FLOW, remainingWater));
//     //          // 물이 흐르지 못하면 
//     //          if (flow == 0)
//     //          {
//     //              // 그리고 내 남아있는 물이 적으면 물 증발
//     //              if (remainingWater <= MIN_AMOUNT)
//     //              {
//     //                  writeCell.Amount -= remainingWater;
//     //                  writeCell.IsDirty = true;
//     //                  writeCell.SetElementTypeOff(ElementType.Water);
//     //              }
//     //          }
//     //          else
//     //          {
//     //              Cell writeNeighborCell = new Cell();
//     //              writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //              writeNeighborCell.Amount += flow;
//     //              writeNeighborCell.IsDirty = true;
//     //              writeNeighborCell.Pos = readNeighborCell.Pos;
//     //              writeCellBuffer.Enqueue(writeNeighborCell);
//     //
//     //              writeCell.Amount -= flow;
//     //              writeCell.IsDirty = true;
//     //              remainingWater -= flow;
//     //          }
//     //
//     //      }
//     //      //
//     //      // // → 오른쪽
//     //      // neighborCell = new int2(x + 1, y);
//     //      // readNeighborCell = ref CellBuffer.GetRef(neighborCell, width, readCellBuffer);
//     //      //
//     //      //
//     //      // if (x < width - 1 && readNeighborCell.ElementType != ElementType.Stone)
//     //      // {
//     //      //     float flow = 0;
//     //      //     if (remainingWater < MIN_FLOW)
//     //      //     {
//     //      //     }
//     //      //     else
//     //      //     {
//     //      //         flow = GetFlowAmount(readCell.Amount, readNeighborCell.Amount,0.25f);
//     //      //         if (flow > MIN_FLOW)
//     //      //         {
//     //      //             flow *= 0.5f;
//     //      //             flow = Math.Clamp(flow, 0f, remainingWater);
//     //      //             remainingWater -= flow;
//     //      //             
//     //      //             if (remainingWater <= MIN_AMOUNT)
//     //      //             {
//     //      //                 flow = remainingWater;
//     //      //                 remainingWater = 0;
//     //      //                 //
//     //      //                 writeCell.SetElementTypeOff(ElementType.Water);
//     //      //             }
//     //      //             
//     //      //             writeCell.Amount -= flow;
//     //      //             writeCell.IsDirty = true;
//     //      //
//     //      //            writeCell.Pos = readCell.Pos;
//     //      //
//     //      //             var writeNeighborCell = new Cell();
//     //      //             writeNeighborCell.Amount += flow;
//     //      //             writeNeighborCell.IsDirty = true;
//     //      //             writeNeighborCell.Pos = readNeighborCell.Pos;
//     //      //             writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //      //             writeCellBuffer.Enqueue(writeNeighborCell);
//     //      //         }
//     //      //
//     //      //     }
//     //      // }
//     //      //
//     //      //
//     //      // // 왼쪽
//     //      // neighborCell = new int2(x - 1, y);
//     //      // readNeighborCell = ref CellBuffer.GetRef(neighborCell, width, readCellBuffer);
//     //      //
//     //      // if (x > 0 && readNeighborCell.ElementType != ElementType.Stone)
//     //      // {  float flow = 0;
//     //      //     if (remainingWater < MIN_FLOW)
//     //      //     {
//     //      //     }
//     //      //     else
//     //      //     {
//     //      //         flow = GetFlowAmount(readCell.Amount, readNeighborCell.Amount,0.25f);
//     //      //         if (flow > MIN_FLOW)
//     //      //         {
//     //      //             flow *= 0.5f;
//     //      //             flow = Math.Clamp(flow, 0f, remainingWater);
//     //      //             remainingWater -= flow;
//     //      //             
//     //      //             if (remainingWater <= MIN_AMOUNT)
//     //      //             {
//     //      //                 flow = remainingWater;
//     //      //                 remainingWater = 0;
//     //      //                 
//     //      //                 writeCell.SetElementTypeOff(ElementType.Water);
//     //      //             }
//     //      //             
//     //      //             writeCell.Amount -= flow;
//     //      //             writeCell.IsDirty = true;
//     //      //             writeCell.Pos = readCell.Pos;
//     //      //
//     //      //             var writeNeighborCell = new Cell();
//     //      //             writeNeighborCell.Amount += flow;
//     //      //             writeNeighborCell.IsDirty = true;
//     //      //             writeNeighborCell.Pos = readNeighborCell.Pos;
//     //      //             writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //      //             writeCellBuffer.Enqueue(writeNeighborCell);
//     //      //         }
//     //      //
//     //      //     }
//     //      // }
//     //      //
//     //      //
//     //      //
//     //      //      
//     //      //  //위쪽 
//     //      //  neighborCell = new int2(x , y+1);
//     //      //  readNeighborCell = ref CellBuffer.GetRef(neighborCell, width, readCellBuffer);
//     //      //
//     //      //  if (y < height-1 && readNeighborCell.ElementType != ElementType.Stone)
//     //      //  {  float flow = 0;
//     //      //      if (remainingWater < MIN_FLOW)
//     //      //      {
//     //      //      }
//     //      //      else
//     //      //      {
//     //      //          flow = remainingWater - GetStableFlowAmount(remainingWater + readNeighborCell.Amount);
//     //      //          if (flow > MIN_FLOW)
//     //      //          {
//     //      //          }
//     //      //          else
//     //      //          {
//     //      //              flow = Math.Clamp(flow, 0f, Math.Max(MAX_FLOW, remainingWater));
//     //      //              remainingWater -= flow;
//     //      //              
//     //      //              if (remainingWater <= MIN_AMOUNT)
//     //      //              {
//     //      //                  flow = remainingWater;
//     //      //                  remainingWater = 0;
//     //      //                  
//     //      //                  writeCell.SetElementTypeOff(ElementType.Water);
//     //      //              }
//     //      //              
//     //      //              writeCell.Amount -= flow;
//     //      //              writeCell.IsDirty = true;
//     //      //              writeCell.Pos = readCell.Pos;
//     //      //
//     //      //              var writeNeighborCell = new Cell();
//     //      //              writeNeighborCell.Amount += flow;
//     //      //              writeNeighborCell.IsDirty = true;
//     //      //              writeNeighborCell.Pos = readNeighborCell.Pos;
//     //      //              writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //      //              writeCellBuffer.Enqueue(writeNeighborCell);
//     //      //          }
//     //      //         
//     //      //      }
//     //      // }
//     //      //
//     //      
//     //      if (remainingWater <= 0)
//     //      {
//     //          writeCell.SetElementTypeOff(ElementType.Water);
//     //          writeCell.IsDirty = true;
//     //      }
//     //
//     //      if (writeCell.IsDirty)
//     //      {
//     //          writeCellBuffer.Enqueue(writeCell);
//     //      }
//     //   
//     //      
//     //  }
//     //
//     //
//     // /// <summary>
//     // /// MainThread 용 워우터 시물 
//     // /// </summary>
//     // /// <param name="readCell"></param>
//     // /// <param name="cellBufferManager"></param>
//     // /// <param name="width"></param>
//     // /// <param name="height"></param>
//     // /// <param name="stepId"></param>
//     // public static void SimulateWaterFlow(in Cell readCell, CellBufferManager cellBufferManager, int width,
//     //     int height, //int forDebugX,int forDebugY,
//     //     ulong stepId)
//     // {
//     //     int x = readCell.Pos.x;
//     //     int y = readCell.Pos.y;
//     //
//     //     // if (forDebugX == x && forDebugY == y)
//     //     // {
//     //     //   //  Debug.Log("aa");
//     //     // }
//     //
//     //     float remainingWater = readCell.Amount;
//     //     ref var writeCell = ref cellBufferManager.GetRefWriteCell(x, y);
//     //
//     //     //  아래로 흐름
//     //     int2 neighborCell = new int2(x, y - 1);
//     //
//     //     ref readonly Cell readNeighborCell = ref cellBufferManager.GetRefReadCell(neighborCell);
//     //     ref Cell writeNeighborCell = ref cellBufferManager.GetRefWriteCell(neighborCell);
//     //
//     //     if (y > 0 && readNeighborCell.ElementType != ElementType.Stone)
//     //     {
//     //         float flow = GetStableFlowAmount(remainingWater + readNeighborCell.Amount) - readNeighborCell.Amount;
//     //         ; //GetFlowAmount(remainingWater, readDownCell.Amount);
//     //
//     //         if (flow > MIN_FLOW)
//     //         {
//     //             // 최소흐름 보다 빠르면 부드럽게 작업 
//     //             flow *= 0.5f;
//     //         }
//     //
//     //         // 남은 물 보다 적지만 최소 흐름보다는 크게 
//     //         flow = Mathf.Clamp(flow, 0f, Mathf.Min(MIN_FLOW, remainingWater));
//     //         // 물이 흐르지 못하면 
//     //         if (flow == 0)
//     //         {
//     //             // 그리고 내 남아있는 물이 적으면 물 증발
//     //             if (writeCell.Amount <= MIN_AMOUNT)
//     //             {
//     //                 writeCell.Amount = 0;
//     //                 writeCell.SetElementTypeOff(ElementType.Water);
//     //                 cellBufferManager.MarkDirtyForMainThread(x, y);
//     //             }
//     //         }
//     //         else
//     //         {
//     //             writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //             writeNeighborCell.Amount += flow;
//     //             cellBufferManager.MarkDirtyForMainThread(neighborCell);
//     //
//     //             writeCell.Amount -= flow;
//     //             cellBufferManager.MarkDirtyForMainThread(x, y);
//     //             remainingWater -= flow;
//     //         }
//     //     }
//     //
//     //     if (remainingWater <= 0)
//     //     {
//     //         writeCell.SetElementTypeOff(ElementType.Water);
//     //         return;
//     //     }
//     //
//     //     // → 오른쪽
//     //     neighborCell = new int2(x + 1, y);
//     //     readNeighborCell = ref cellBufferManager.GetRefReadCell(neighborCell);
//     //     writeNeighborCell = ref cellBufferManager.GetRefWriteCell(neighborCell);
//     //
//     //     if (x < width - 1 && readNeighborCell.ElementType != ElementType.Stone)
//     //     {
//     //         float flow = 0;
//     //         if (remainingWater < MIN_FLOW)
//     //         {
//     //         }
//     //         else
//     //         {
//     //             flow = GetFlowAmount(readCell.Amount, readNeighborCell.Amount, 0.25f);
//     //             if (flow > MIN_FLOW)
//     //             {
//     //                 flow *= 0.5f;
//     //                 flow = Mathf.Clamp(flow, 0f, remainingWater);
//     //                 writeCell.Amount -= flow;
//     //                 remainingWater -= flow;
//     //
//     //                 cellBufferManager.MarkDirtyForJobThread(x, y);
//     //
//     //                 if (writeCell.Amount <= MIN_AMOUNT)
//     //                 {
//     //                     writeCell.Amount = 0;
//     //                     writeCell.SetElementTypeOff(ElementType.Water);
//     //                     cellBufferManager.MarkDirtyForJobThread(x, y);
//     //                 }
//     //
//     //                 writeNeighborCell.Amount += flow;
//     //                 writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //                 cellBufferManager.MarkDirtyForJobThread(neighborCell);
//     //             }
//     //         }
//     //     }
//     //
//     //     if (remainingWater <= 0)
//     //     {
//     //         writeCell.SetElementTypeOff(ElementType.Water);
//     //         return;
//     //     }
//     //
//     //     // 왼쪽
//     //     neighborCell = new int2(x - 1, y);
//     //     readNeighborCell = ref cellBufferManager.GetRefReadCell(neighborCell);
//     //     writeNeighborCell = ref cellBufferManager.GetRefWriteCell(neighborCell);
//     //     if (x > 0 && readNeighborCell.ElementType != ElementType.Stone)
//     //     {
//     //         float flow = 0;
//     //         if (remainingWater < MIN_FLOW)
//     //         {
//     //         }
//     //         else
//     //         {
//     //             flow = GetFlowAmount(readCell.Amount, readNeighborCell.Amount, 0.25f);
//     //             if (flow > MIN_FLOW)
//     //             {
//     //                 flow *= 0.5f;
//     //                 flow = Mathf.Clamp(flow, 0f, remainingWater);
//     //                 writeCell.Amount -= flow;
//     //                 remainingWater -= flow;
//     //
//     //                 cellBufferManager.MarkDirtyForJobThread(x, y);
//     //
//     //                 if (writeCell.Amount <= MIN_AMOUNT)
//     //                 {
//     //                     writeCell.Amount = 0;
//     //                     writeCell.SetElementTypeOff(ElementType.Water);
//     //                     cellBufferManager.MarkDirtyForJobThread(x, y);
//     //                 }
//     //
//     //                 writeNeighborCell.Amount += flow;
//     //                 writeNeighborCell.SetElementTypeOn(ElementType.Water);
//     //                 cellBufferManager.MarkDirtyForJobThread(neighborCell);
//     //             }
//     //         }
//     //     }
//     //
//     //     if (remainingWater <= 0)
//     //     {
//     //         return;
//     //     }
//     //
//     //     //     
//     //     // // ↑ 위로 (압력에 의해 약간만)
//     //     // if (y < height - 1 && !grid[x, y + 1].isSolid)
//     //     // {
//     //     //     float flow = GetFlowAmount(grid[x, y].water, grid[x, y + 1].water) * 0.25f;
//     //     //     flow = Mathf.Min(flow, remainingWater);
//     //     //     next[x, y].water -= flow;
//     //     //     next[x, y + 1].water += flow;
//     //     // }
//     //     //     grid = next;
//     // }
// }
//
//
// [BurstCompile]
// public class FlowApplyStep
// {
//     [BurstCompile]
//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public static void ApplyFlow(int index,
//         in Cell readCell,
//         ref Cell writeCell,
//         //  ref NativeList<Cell>.ParallelWriter debug,
//         ref NativeParallelHashSet<int2>.ParallelWriter nextDirtyCell,
//         in NativeParallelHashMap<int2, FlowInfo> flowInfoMap,
//         ulong stepId)
//     {
//         bool isDirty = false;
//         int remainAmount = readCell.Amount;
//         //float myOutAmount = 0;
//         if (flowInfoMap.TryGetValue(readCell.Pos, out FlowInfo myFlow))
//         {
//             remainAmount -= myFlow.TotalFlowAmount;
//
//             if (remainAmount == 0)
//             {
//                 writeCell.SetElementTypeOff(myFlow.ElementType);
//             }
//
//             isDirty = true;
//         }
//
//         int2 neighborPos = readCell.Pos + Directions.Cardinal[(int)Direction.Up];
//         if (flowInfoMap.TryGetValue(neighborPos, out FlowInfo upFlowInfo))
//         {
//             if (upFlowInfo.FlowAmount[(int)Direction.Down] > 0)
//             {
//                 remainAmount += upFlowInfo.FlowAmount[(int)Direction.Down];
//                 writeCell.SetElementTypeOn(upFlowInfo.ElementType);
//                 isDirty = true;
//             }
//         }
//
//         //
//         neighborPos = readCell.Pos + Directions.Cardinal[(int)Direction.Down];
//         if (flowInfoMap.TryGetValue(neighborPos, out FlowInfo downFlowInfo))
//         {
//             if (downFlowInfo.FlowAmount[(int)Direction.Up] > 0)
//             {
//                 remainAmount += downFlowInfo.FlowAmount[(int)Direction.Up];
//                 writeCell.SetElementTypeOn(downFlowInfo.ElementType);
//                 isDirty = true;
//             }
//         }
//
//         //
//         neighborPos = readCell.Pos + Directions.Cardinal[(int)Direction.Right];
//         if (flowInfoMap.TryGetValue(neighborPos, out FlowInfo rightFlowInfo))
//         {
//             if (rightFlowInfo.FlowAmount[(int)Direction.Left] > 0)
//             {
//                 remainAmount += rightFlowInfo.FlowAmount[(int)Direction.Left];
//                 writeCell.SetElementTypeOn(rightFlowInfo.ElementType);
//                 isDirty = true;
//             }
//         }
//
//         neighborPos = readCell.Pos + Directions.Cardinal[(int)Direction.Left];
//         if (flowInfoMap.TryGetValue(neighborPos, out FlowInfo leftFlowInfo))
//         {
//             if (leftFlowInfo.FlowAmount[(int)Direction.Right] > 0)
//             {
//                 remainAmount += leftFlowInfo.FlowAmount[(int)Direction.Right];
//                 writeCell.SetElementTypeOn(leftFlowInfo.ElementType);
//                 isDirty = true;
//             }
//         }
//
//         if (isDirty)
//         {
//             for (int i = 0; i < FixedInt2Array4.Count; i++)
//             {
//                 // fixed 배열 구조체는 this[int]로 ref 반환함
//                 ref readonly int2 dir = ref Directions.Cardinal[i];
//
//                 nextDirtyCell.Add(readCell.Pos + dir);
//             }
//
//             // if (remainAmount < 0)
//             // {
//             //     Cell debugCell = writeCell;
//             //     debugCell.Amount1 = myInAmount;
//             //     debugCell.Amount2 = myOutAmount;
//             //     debugCell.Amount3 = readCell.Amount;
//             //     debuge.AddNoResize(debugCell);
//             // }
//
//             if (MathUtil.IsNearlyZero(remainAmount))
//             {
//                 remainAmount = 0;
//                 writeCell.SetElementTypeOn(ElementType.Empty);
//             }
//
//             writeCell.Amount = remainAmount;
//         }
//     }
// }
//
// public static class MathUtil
// {
//     public static bool IsNearlyZero(float value, float epsilon = 1e-6f)
//     {
//         return math.abs(value) < epsilon;
//     }
// }