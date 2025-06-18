 using System;
using System.Collections;
using SerializableDictionaryGroup;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

enum SimulateThreadMode
{
    MainThread,
    JobThread,
}

enum SimulateStep
{
    SimulateMovement // 이동 스텝 
}


public class CaveGeneratorByCellularAutomata : MonoBehaviour
{
    public GameObject gg;
    
    /// <summary> 시물레이션 돌릴 쓰레드 모드 </summary>
    [SerializeField] [Header(" 시물레이션 돌릴 쓰레드 모드 ")]
    private SimulateThreadMode simulateThreadMode;

    /// <summary> 잡 배칭 사이즈 </summary>
    [SerializeField] [Header(" 잡 배칭 사이즈 ")]
    private int jobBatchingSize = 64;
    
    /// <summary> 더티 셀 카운트 </summary>
    [SerializeField] [Header(" 더티 셀  ")]
    private int currentDirtyCount = 0;
    int forDebugX, forDebugY;
    
    /// <summary> 원소별 타일  </summary>
    [SerializeField] [Header("원소별 타일 딕셔너리")]
    private ElementTileDictionary elementTileDictionary = new();
    /// <summary> 맵 넓이 </summary>
    [SerializeField]  [Header(" 맵 넓이 ")]
    private int width;
    /// <summary> 맵 높이 </summary>
    [SerializeField] [Header(" 맵 높이 ")]
    private int height;
    
    /// <summary>셀 버퍼매니저</summary>
    private CellBufferManager cellBufferManager;
    
    /// <summary> 맵 생성 시드 </summary>
    [SerializeField][Header(" 맵 생성 시드 ")] 
    private string seed;
    /// <summary> 맵 생성 랜덤할당 </summary>
    [SerializeField][Header(" 맵 생성 랜덤할당 ")]  
    private bool useRandomSeed;

    /// <summary> 맵 생성시 공간 비율 </summary>
    [Range(0, 100)] [SerializeField][Header(" 맵 생성시 공간 비율 ")]  
    private int randomFillPercent;
    
    /// <summary> 맵 Smooth 로직 횟수 </summary>
    [SerializeField] [Header(" 맵 Smooth 로직 횟수 ")]  
    private int smoothNum;

    /// <summary> 타일 맵 </summary>
    [SerializeField] [Header( " 타일 맵 ")]  
    private Tilemap tilemap;

    /// <summary> 현재 스텝 id </summary>
    [SerializeField] [Header( " 현재 스텝 id ")]  
    private ulong currentStepId;
    
    /// <summary> 현재 시뮬레이트 중 </summary>
    [SerializeField] [Header( " 현재 시뮬레이트 중 ")]  
    private bool isSimulate = false;


    #region 캐싱 용 
     
    /// <summary> 맵 넓이 반 </summary>
    private float halfWidth;
    
    /// <summary> 맵 높이 반 </summary>
    private float halfHeight;
    
    /// <summary> 메인 카메라 </summary>
    private Camera mainCamera;
        
    #endregion


    #region JobSystem
    
    /// <summary> 시뮬레이트에 보낼 DirtyCell리스트 (IJobParallelFor 용 HashSet변환 ) </summary>
    private NativeList<int2> dirtyCells;

    /// <summary> 시뮬레이트에 보낼 NativeQueue 시뮬 잡에서 Dirty로 만든 애들 반환  </summary>
    private NativeParallelHashMap<int2, FlowInfo> tempFlowInfoMap;
    
    /// <summary> MoveStep 시뮬 잡 </summary>
    private JobHandle simulateMoveStepJobHandle;
    /// <summary> 시뮬 끝나고 결과 적용 잡 </summary>
    private JobHandle lastStepJobHandle;
  //  public NativeList<Cell> debuge;
    #endregion

    private void Start()
    {
        Initialize();
       // Application.targetFrameRate = 60;
        MakeMap(smoothNum);

      
    }
    
    /// <summary>
    /// 버퍼 생성 
    /// </summary>
    private void Initialize()
    {
        cellBufferManager = new CellBufferManager(width,height,1,Allocator.Persistent);
        halfWidth = width / 2f;
        halfHeight = height / 2f;
        tempFlowInfoMap  =    new NativeParallelHashMap<int2, FlowInfo>(width * height,Allocator.Persistent);
      //  debuge = new NativeList<Cell>(width * height, Allocator.Persistent);
        if(mainCamera == null)
            mainCamera = Camera.main;
    }
    
    private void OnDestroy()
    {
        simulateMoveStepJobHandle.Complete();
        lastStepJobHandle.Complete();
        cellBufferManager.Dispose();

        if(tempFlowInfoMap.IsCreated)
            tempFlowInfoMap.Dispose();

        // if(debuge.IsCreated)
        //     debuge.Dispose();
    }
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // 맵 생성 로직 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
  
    #region 맵 만들기 1번

    /// <summary>
    /// 기본 맵 생성은 Smooth를 횟수만큼 반복
    /// </summary>
    private void MakeMap( int smoothNum)
    {
        MapRandomFill();
        cellBufferManager.SwapBuffer();

        for (int i = 0; i < smoothNum; i++)
        {
            SmoothMap();
            cellBufferManager.SwapBuffer();
        }
        
        cellBufferManager.MakeAllDirty();
        OnDrawTile();
        currentStepId++;
    }

    /// <summary>
    /// 비율에 따라 맵의 셀을 채웁니다. 
    /// </summary>
    private void MapRandomFill() 
    {
        if (useRandomSeed) 
            seed =  DateTime.Now.Ticks.ToString(); //시드
    
        System.Random pseudoRandom = new System.Random(seed.GetHashCode()); //시드로 부터 의사 난수 생성
    
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++)
            {
                ref Cell writeCell = ref cellBufferManager.GetRefWriteCell(x,y);
                writeCell.Pos = new int2(x, y);

                // 외각 체크 벽으로 할거임 
                if (IsOutskirts(x,y))
                {
                    writeCell.SetElementType(ElementType.Stone);
                    writeCell.Amount = 1;
                }
                else
                {
                    // 랜덤하게 벽인지 또는 빈공간인지 설정 
                    //비율에 따라 벽 혹은 빈 공간 생성
                    if (pseudoRandom.Next(0, 100) < randomFillPercent)
                    {
                        writeCell.SetElementType(ElementType.Stone);
                        writeCell.Amount = 1;
                    }
                }
                cellBufferManager.MarkDirtyForMainThread(writeCell.Pos);
            }
        }
    }
    
    /// <summary>
    /// 헤딩 값이 외곽인지
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <returns></returns>
    private bool IsOutskirts(int x, int y)
    {
        if (cellBufferManager.CheckOutOfIndex(x,y))
        {
            throw new Exception($"CellBuffer.IsOutskirts에서 인덱스를 벗어남\n" +
                                $"인자 X : {x} / 넓이 {width} \n" +
                                $"인자 Y : {y} / 높이 {height}"
            );
        }
        else
        {
            return (x == 0 || x == width - 1 || y == 0 || y == height - 1);
        }
    }
    
    /// <summary>
    /// 셀룰러 오토마타의 방식으로 맵을 부드럽게 만듭니다.
    /// </summary>
    private void SmoothMap()
    {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int neighbourWallTiles = GetSurroundElementCount(x, y,ElementType.Stone);
                
                ref Cell cell = ref cellBufferManager.GetRefWriteCell(x, y);
                
                if (IsOutskirts(x, y))
                    continue;
                
                //주변 칸 중 벽이 4칸을 초과할 경우 현재 타일을 벽으로 바꿈
                if (neighbourWallTiles > 4)
                {
                    cell.SetElementType(ElementType.Stone);
                    cell.Amount = 1;
                }
                //주변 칸 중 벽이 4칸 미만일 경우 현재 타일을 빈 공간으로 바꿈
                else if (neighbourWallTiles < 4)
                {
                    cell.SetElementType( ElementType.Empty);
                    cell.Amount = 0;
                }
                cellBufferManager.MarkDirtyForMainThread(cell.Pos);
            }
        }
    }

    #endregion
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // 메인 로직 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    private void Update()
    {
        
        // if(Time.frameCount%4 != 0)
        //     return;
        StartSimulate();
        
        // 입력 액션은 항상 맨뒤에서 해야함 버퍼에 강제로 넣는거여서 꼬임 
        CellInputLogic();
    }
    void LateUpdate()
    {
        FinishSimulate();
    }


    private void StartSimulate()
    {
        if (isSimulate) return; 
        currentDirtyCount = cellBufferManager.CurrentDirtyCount;
        
        //수정된 Cell이 없으면 시뮬 X 
        if(currentDirtyCount == 0)
            return;

        
        switch (simulateThreadMode)
        {
            case SimulateThreadMode.MainThread:
                StartSimulateByMainThread();
                //StartSimulateByMainThread();
                break;
            case SimulateThreadMode.JobThread:
                StartSimulateByJobThread();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void FinishSimulate()
    {
        switch (simulateThreadMode)
        {
            case SimulateThreadMode.MainThread:
                FinishSimulateByMainThread();
                break;
            case SimulateThreadMode.JobThread:
                FinishSimulateByJobThread();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void SimulateChemicalReaction( )
    {
        
    }

    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // MainThread 용 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================

    #region MainThread

    /// <summary>
    /// 시물레이터 스타트 ( MainThread 용 )
    /// </summary>
    private void StartSimulateByMainThread()
    {
        isSimulate = true;
        SimulateMoveStep();
    }
    
    /// <summary>
    /// 시물레이터 스타트 ( MainThread 용 )
    /// </summary>
    private void FinishSimulateByMainThread()
    {
        if (!isSimulate)
            return;
        isSimulate = false;

        cellBufferManager.SwapBuffer();
        currentDirtyCount = cellBufferManager.CurrentDirtyCount;
                
        //그래픽 업데이팅 
        OnDrawTile();
        currentStepId++;
        StartSimulate();
    }
    
    
    /// <summary>
    /// 셀 이동 시뮬레이션 
    /// </summary>
    /// <param name="cell"></param>
    private void SimulateMoveStep()
    {
        foreach (int2 dirtyCell in cellBufferManager.GetDirtyCells())
        {
            ref readonly Cell cell = ref cellBufferManager.GetRefReadCell(dirtyCell.x, dirtyCell.y);
            switch (cell.ElementType)
            {
                case ElementType.Water:
                    //MoveStep.SimulateWaterFlow(in cell,cellBufferManager,width,height,currentStepId);

                    break;
            }
            
        }
    
    }

    #endregion

    
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // Burst + Job 스레드 로직 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    
    #region JobSystem
    
    /// <summary>
    /// 잡으로 시뮬레이션 돌리기 
    /// </summary>
    private void StartSimulateByJobThread()
    {
        isSimulate = true;
       //debuge.Clear();
        dirtyCells = cellBufferManager.GetDirtyCellsList();
        //tempWriteCellBuffer.Clear();
        //tempWriteCellBuffer = new NativeArray<Cell>(currentDirtyCount * (int)NeighborIndex.Max, Allocator.Persistent);
        var moveStepJob = new SimulateMoveStepJob
        {
            dirtyCells = dirtyCells,
            readCellBuffer = cellBufferManager.ReadCellBuffer.Buffer,
            flowInfoMap = tempFlowInfoMap.AsParallelWriter(),
            width = width,
            height = height,
            currentStepId = currentStepId
        };
        
        // dirtyCells.Length, batchSize 를 실험적으로 조정
        //simulationHandles = simulateJob.ScheduleParallel(jobCount, jobBatchingSize);
        simulateMoveStepJobHandle = moveStepJob.Schedule(currentDirtyCount, jobBatchingSize);
        
        // 
        var flowApplyStepJob = new SimulateFlowApplyStep
        {
            dirtyCells = dirtyCells,
            readCellBuffer = cellBufferManager.ReadCellBuffer.Buffer,
            writeCellBuffer = cellBufferManager.WriteCellBuffer.Buffer,
            flowInfoMap = tempFlowInfoMap,
            width = width,
            currentStepId = currentStepId,
            nextDirtySet = cellBufferManager.GetNextDirtyWriter(),
            // debuge  = debuge.AsParallelWriter(),
        };
        //combineHandle = combineJob.Schedule(tempWriteCellBuffer.Length, jobBatchingSize, simulationHandles);
        lastStepJobHandle = flowApplyStepJob.Schedule(currentDirtyCount,jobBatchingSize,simulateMoveStepJobHandle);
    }
    
    /// <summary>
    /// 잡 끝내기 
    /// </summary>
    private void FinishSimulateByJobThread()
    {
        if (!isSimulate || !lastStepJobHandle.IsCompleted) 
            return;
        isSimulate = false;
        simulateMoveStepJobHandle.Complete();
        lastStepJobHandle.Complete();
        tempFlowInfoMap.Clear();
        //if (debuge.Length > 0)
        // {
        //     foreach (Cell cell in debuge)
        //     {
        //         Debug.Log($"{cell.Pos}   {cell.Amount} \n originRemain :{cell.Amount3}\n" +
        //                   $"myOutAmount {cell.Amount2}\n" +
        //                   $"myInAmount {cell.Amount1}");
        //     }
        //
        //     Debug.Break();
        // }
        dirtyCells.Clear();
        cellBufferManager.SwapBuffer();
        OnDrawTile();
        currentStepId++;
        //tartSimulate();
    }
    //
    // [BurstCompile] 
    // struct SimulateMoveStepBatchJob : IJobParallelForBatch
    // {
    //     [ReadOnly] public NativeList<int2> dirtyCells;
    //     [ReadOnly] public NativeArray<Cell> readCellBuffer;
    //     [ReadOnly] public int width;
    //     [ReadOnly] public int height;
    //     [ReadOnly] public ulong currentStepId;
    //     
    //     [NativeDisableParallelForRestriction] 
    //     [WriteOnly]
    //     public  NativeQueue<Cell>.ParallelWriter writeCellBuffer;
    //
    //     [NativeDisableParallelForRestriction] 
    //     [WriteOnly]
    //     public  NativeQueue<Cell>.ParallelWriter tempDirtyCellBuffer;
    //
    //     
    //     public void Execute(int startIndex, int count)
    //     {
    //         for (int i = startIndex; i < startIndex + count; i++)
    //         {
    //            ref readonly Cell cell = ref  CellBuffer.GetRef_ReadOnly(dirtyCells[i], width, readCellBuffer);
    //
    //             switch (cell.ElementType)
    //             {
    //                 case ElementType.Water:
    //                     MoveStepSimulation.SimulateWaterCell(i, in cell, in readCellBuffer, ref writeCellBuffer, width, height, currentStepId);
    //                     break;
    //             }
    //         }
    //     }
    // }

    [BurstCompile]
     struct SimulateMoveStepJob : IJobParallelFor
    {
        [ReadOnly] public NativeList<int2> dirtyCells;
        [ReadOnly] public NativeArray<Cell> readCellBuffer;

        [ReadOnly] public int width;
        [ReadOnly] public int height;
        [ReadOnly] public ulong currentStepId;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeParallelHashMap<int2, FlowInfo>.ParallelWriter flowInfoMap;
        
        public void Execute(int index)
        {
            ref readonly  Cell readCell = ref CellBuffer.GetRef_ReadOnly(dirtyCells[index], width, readCellBuffer);
            
            switch (readCell.ElementType)
            {
                case ElementType.Water:
                    MoveStep.SimulateWaterFlow(index, in readCell, in readCellBuffer, ref flowInfoMap,   width,  height,  currentStepId);
                    break;
            }
        }
    }

    [BurstCompile]
    struct SimulateFlowApplyStep : IJobParallelFor
    {
        [ReadOnly] public NativeList<int2> dirtyCells;
        [ReadOnly] public NativeArray<Cell> readCellBuffer;
    
        [ReadOnly]public NativeArray<Cell> writeCellBuffer;
        
        [ReadOnly]public NativeParallelHashMap<int2, FlowInfo> flowInfoMap;
        [ReadOnly] public int width;
        [ReadOnly] public ulong currentStepId;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly]public NativeParallelHashSet<int2>.ParallelWriter nextDirtySet;

        //[WriteOnly] public NativeList<Cell>.ParallelWriter debuge;
        public void Execute(int index)
        {
            ref readonly  Cell readCell = ref CellBuffer.GetRef_ReadOnly(dirtyCells[index], width, readCellBuffer);
            ref  Cell writeCell = ref CellBuffer.GetRef(dirtyCells[index], width, writeCellBuffer);
           // FlowApplyStep.ApplyFlow( index , in readCell,ref writeCell,ref debuge,ref nextDirtySet,in flowInfoMap, currentStepId );
           FlowApplyStep.ApplyFlow( index , in readCell,ref writeCell,ref nextDirtySet,in flowInfoMap, currentStepId );

        }
    } 
    
    #endregion
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // 입력 액션들 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================

  
    /// <summary>
    /// Cell 입력 로직들 
    /// </summary>
    private void CellInputLogic()
    {
        if (Input.GetMouseButton(0))
        {
            int2 cellPos = GetCellPositionByMousePosition();
            if (!cellBufferManager.CheckOutOfIndex(cellPos.x, cellPos.y))
            {
                ref Cell cell = ref cellBufferManager.GetRefWriteCell(cellPos.x, cellPos.y);
               
                //StartSimulate();     
                if (cell.ElementType == ElementType.Empty)
                {
                    cell.SetElementTypeOn(ElementType.Water);
                    cell.Amount = 10000;
                    cellBufferManager.MarkNeighborsDirtyForMainThread(cellPos);
                    if (isSimulate)
                        return;
                    //현재 더티셀로한후 그래픽 업데이트 ( 항상 swap ( next를 current로 옮기후 그래픽 업데이트해야함 )
                    cellBufferManager.SwapBuffer();
                    OnDrawTile();

                }
            }
        }
                    
        if (Input.GetMouseButtonDown(1))
        {
            int2 cellPos = GetCellPositionByMousePosition();
            if (!cellBufferManager.CheckOutOfIndex(cellPos))
            {
                ref readonly Cell cell = ref cellBufferManager.GetRefReadCell(cellPos);
                Debug.Log(cell.ToString());
                //forDebugX =cellX ; forDebugY = cellY;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            int2 cellPos = GetCellPositionByMousePosition();
            if (!cellBufferManager.CheckOutOfIndex(cellPos))
            {
                cellBufferManager.MarkDirtyForMainThread(cellPos);
                cellBufferManager.SwapBuffer();
                OnDrawTile();
            }
        }

        
        
    }

    /// <summary>
    /// 마우스 포지션에 해당하는 Cell Position 반환 
    /// </summary>
    /// <returns></returns>
    private int2 GetCellPositionByMousePosition()
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        int cellX = Mathf.FloorToInt(worldPosition.x + halfWidth);
        int cellY = Mathf.FloorToInt(worldPosition.y + halfHeight);
        return new int2(cellX, cellY);
    }

    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // 그래픽 업데이트 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================

    /// <summary>
    /// 타일 그리는 함수 
    /// </summary>
    private void OnDrawTile()
    {
        foreach (int2 dirtyCell in cellBufferManager.GetDirtyCells())
        {

            ref readonly Cell cell = ref cellBufferManager.GetRefReadCell(dirtyCell.x, dirtyCell.y);

            Vector3Int pos = new Vector3Int(-(int)halfWidth + cell.Pos.x, -(int)halfHeight + cell.Pos.y, 0);

            Instantiate(gg, (Vector3)pos,Quaternion.identity);
            // // Debug.Log(pos);
            // Tile tile;
            // ElementTile tileType  = (ElementTile)((int)cell.ElementType);
            // if (elementTileDictionary.TryGetValue(tileType, out tile))
            // {
            //     tilemap.SetTile(pos,tile);
            // }
        }
    }
    
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================
    // 헬퍼 함수 
    //==========================================================================================
    //==========================================================================================
    //==========================================================================================

    
    /// <summary>
    /// [x,y]의 주위 8칸에 해당 Element의 개수
    /// </summary>
    /// <param name="x"> x 좌표 </param>
    /// <param name="y"> y 좌표 </param>
    /// <param name="elementType"> 검색할 타입 </param>
    /// <returns></returns>
    private int GetSurroundElementCount(int x, int y, ElementType elementType)
    {
        int count = 0;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) 
                    continue; // 자기 자신 제외
                int nx = x + dx;
                int ny = y + dy;
                
                // 범위 벗어나는지 체크 
                if (cellBufferManager.CheckOutOfIndex(nx,ny))
                    continue;

                if (cellBufferManager.GetRefReadCell(nx, ny).ElementType == elementType)
                    count++;
            }
        }
        return count ;
    }



    
}
