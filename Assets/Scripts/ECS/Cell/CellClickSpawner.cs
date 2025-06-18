using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Ray = UnityEngine.Ray;

public class CellClickSpawner : MonoBehaviour
{
    private Camera MainCam;
    EntityManager em;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MainCam = Camera.main;
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    // Update is called once per frame
    void Update()
    {
        ///return;
        if (Input.GetMouseButton(0))
        {
            var singleton = em.CreateEntityQuery(typeof(PhysicsWorldSingleton)).GetSingleton<PhysicsWorldSingleton>();
            var world = singleton.PhysicsWorld;
            Entity mapEntity = em.CreateEntityQuery(typeof(MapData)).GetSingletonEntity();
            DynamicBuffer<CellGroupData> CellGroupDatas = em.GetBuffer<CellGroupData>(mapEntity);


            var cellPosition = ClickPosition(MainCam, Input.mousePosition);
            var celldata = CellGroupDatas[CellIndexFinder.GetIndex((int)cellPosition.x, (int)cellPosition.y, 200)];
            var cell = em.GetComponentData<CellDataJSH>(celldata.CellEntity);
            //Debug.Log($"맞은 엔티티 좌표 = X : {cell.Postion.x} Y : {cell.Postion.y}\n엔티티 상태 : {cell.CellVisualType}\n엔티티 벨류 : {cell.Amount}");
            if (cell.CellVisualType == CellVisualType.Empty)
            {
                SpawnWater(celldata.CellEntity);
            }
        }
    }

    void SpawnWater(Entity entity)
    {
        var cell = em.GetComponentData<CellDataJSH>(entity);
        cell.Amount = 10000;
        cell.CellVisualType = CellVisualType.Water;
        em.SetComponentData(entity, cell);
        em.SetComponentData(entity, new ColorOverrid()
        {
            Value = new float4(0, 1, 0, 1)
        });
        em.AddComponentData(entity, new WaterTag());
    }

    public static Vector2 ClickPosition(Camera MainCam, Vector3 clickPosition)
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 카메라가 Orthographic 이어야 함
        float worldScreenHeight = MainCam.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * screenWidth / screenHeight;

        // 월드 좌표 좌하단 계산 (카메라 위치 기준)
        Vector3 bottomLeftWorld = new Vector3(
            MainCam.transform.position.x - worldScreenWidth / 2f,
            MainCam.transform.position.y - worldScreenHeight / 2f,
            MainCam.transform.position.z);

        // 마우스 픽셀 좌표 정규화
        float normalizedX = clickPosition.x / screenWidth;
        float normalizedY = clickPosition.y / screenHeight;

        // 좌하단 기준 월드 좌표
        float worldX = bottomLeftWorld.x + normalizedX * worldScreenWidth;
        float worldY = bottomLeftWorld.y + normalizedY * worldScreenHeight;

        int cellX = Mathf.RoundToInt(worldX);
        int cellY = Mathf.RoundToInt(worldY);

        Vector2 postion = new Vector2(cellX, cellY);

        Debug.Log($"클릭한 셀 좌표: {cellX}, {cellY}");
        return postion;
    }
}