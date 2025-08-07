using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class MapExporter : MonoBehaviour
{
    [System.Serializable]
    public class MapEntityData
    {
        public string prefabName;
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
    }

    [System.Serializable]
    public class MapData
    {
        public List<MapEntityData> entities = new();
    }

    [MenuItem("Tools/Export Map To JSON")]
    public static void Export()
    {
        var allObjects = GameObject.FindGameObjectsWithTag("MapEntity"); // 또는 Layer 등 사용
        var mapData = new MapData();

        foreach (var obj in allObjects)
        {
            var data = new MapEntityData
            {
                prefabName = obj.name.Replace("(Clone)", "").Trim(),
                position = obj.transform.position,
                scale = obj.transform.localScale,
                rotation = obj.transform.rotation
            };
            mapData.entities.Add(data);
        }

        string json = JsonUtility.ToJson(mapData, true);
        File.WriteAllText(Application.dataPath + "/map_export.json", json);
        Debug.Log("Map exported to JSON");
    }
}