using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuScript : Editor
{
   [MenuItem("Tools/Assign Tile Material")]
   public static void AssignMaterial()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        Material material_ground = Resources.Load<Material>("tile_ground");
        Material material_tile = Resources.Load<Material>("tile");
        Material material_water = Resources.Load<Material>("TileWater");
        foreach (GameObject t in tiles)
        {
            Tile myTile = t.GetComponent<Tile>();
            if (myTile.getType() == "Water")
            {
                t.GetComponent<Renderer>().material = material_water;
            }
            else if(myTile.getType() == "Iceberg")
            {
                t.GetComponent<Renderer>().material = material_ground;
            }
            else if (myTile.getType() == "Ice")
            {
                t.GetComponent<Renderer>().material = material_tile;
            }

        }
    }

    [MenuItem("Tools/Assign Tile Script")]
    public static void AssignTileScript()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject t in tiles)
        {
            t.AddComponent<Tile>();
        }
    }
}
