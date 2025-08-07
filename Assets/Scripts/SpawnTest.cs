using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnTest : MonoBehaviour
{
    public GameObject prefab;
    
    public Mesh mesh;
    public Material material;
    
    const int INSTANCE_COUNT = 40000;
    const int BATCH_SIZE = 1023;
    Matrix4x4[][] allMatrices;

    // Start is called before the first frame update
    void Start()
    {
        for (int x = 0; x < 200; x++)
        {
            for (int y = 0; y <= 200; y++)
            {
                var obj = Instantiate(prefab);
                obj.transform.position = new Vector3(x, y, 0);
            }
        }
        
        // // 카메라 위치 세팅
        // Camera.main.transform.position = new Vector3(100, 150, -200);
        // Camera.main.transform.LookAt(new Vector3(100, 0, 100));
        // Camera.main.farClipPlane = 1000f;
        //
        // // 미리 행렬 생성
        // int totalBatches = Mathf.CeilToInt((float)INSTANCE_COUNT / BATCH_SIZE);
        // allMatrices = new Matrix4x4[totalBatches][];
        //
        // for (int i = 0; i < totalBatches; i++)
        // {
        //     int count = Mathf.Min(BATCH_SIZE, INSTANCE_COUNT - i * BATCH_SIZE);
        //     Matrix4x4[] matrices = new Matrix4x4[count];
        //
        //     for (int j = 0; j < count; j++)
        //     {
        //         int index = i * BATCH_SIZE + j;
        //         float x = (index % 200) * 1.1f;
        //         float z = (index / 200) * 1.1f;
        //         Vector3 pos = new Vector3(x, 0, z);
        //         matrices[j] = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
        //     }
        //
        //     allMatrices[i] = matrices;
        // }
    }

    void Update()
    {
        // if (mesh == null || material == null || allMatrices == null)
        //     return;
        //
        // // 매 프레임 그리기
        // foreach (var matrices in allMatrices)
        // {
        //     Graphics.DrawMeshInstanced(mesh, 0, material, matrices);
        // }
    }
}
