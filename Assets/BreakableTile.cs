using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableTile : MonoBehaviour
{
    Tilemap tilemap;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void DeleteTile(Vector3 pos)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(pos);

        tilemap.SetTile(cellPosition, null);
    }
}
