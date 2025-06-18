using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshMaterialLibrary
{
    private Dictionary<string, int> _meshKeyToID = new();
    private Dictionary<string, int> _matKeyToID = new();

    private Dictionary<int, Mesh> _idToMesh = new();
    private Dictionary<int, Material> _idToMat = new();

    private BatchRendererGroup _brg;
    private int _nextID = 1;

    public MeshMaterialLibrary(BatchRendererGroup brg)
    {
        _brg = brg;
    }

    public int RegisterMesh(string key, Mesh mesh)
    {
        if (_meshKeyToID.TryGetValue(key, out var id))
            return id;

        int newID = _nextID++;
        var meshID = _brg.RegisterMesh(mesh);
        _meshKeyToID[key] = newID;
        _idToMesh[newID] = mesh;
        return newID;
    }

    public int RegisterMaterial(string key, Material mat)
    {
        if (_matKeyToID.TryGetValue(key, out var id))
            return id;

        int newID = _nextID++;
        var matID = _brg.RegisterMaterial(mat);
        _matKeyToID[key] = newID;
        _idToMat[newID] = mat;
        return newID;
    }

    public Mesh GetMesh(int id) => _idToMesh[id];
    public Material GetMaterial(int id) => _idToMat[id];
}