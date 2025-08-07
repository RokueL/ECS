using System;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace SerializableDictionaryGroup
{
    
    [Serializable]
    public class ElementTileDictionary : SerializableDictionary<ElementTile, Tile> { }
}
