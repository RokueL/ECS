using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Collections.Generic;

public class SimpleBRGExample : MonoBehaviour
{
    public Mesh mesh;
    public Material material;

    private BatchRendererGroup brg;
    private GraphicsBuffer matrixBuffer;

    void Start()
    {
    }

    void OnDestroy()
    {
        brg.Dispose();
        matrixBuffer.Dispose();
    }
}
