using UnityEngine;
using System.Collections.Generic;

public class TileSpawner : MonoBehaviour
{
    public GameObject[] tilePrefabs; // Assign 9 unique prefabs here

    private readonly int rows = 6;
    private readonly int cols = 3;

    // Hardcoded bounding box
    private Vector3 boxCenter = new Vector3(34.21f, 3.22f, -4.02f);
    private Vector3 boxScale  = new Vector3(3.250326f, 1.0249f, 7.420689f);

    void Start()
    {
        RespawnTiles();
    }

    public void RespawnTiles()
    {
        ClearChildren();

        List<GameObject> tilesToSpawn = GenerateTileList();
        ShuffleList(tilesToSpawn);
        SpawnTiles(tilesToSpawn);
    }

    private List<GameObject> GenerateTileList()
    {
        List<GameObject> tiles = new List<GameObject>();
        foreach (GameObject prefab in tilePrefabs)
        {
            tiles.Add(prefab);
            tiles.Add(prefab);
        }
        return tiles;
    }

    private void ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randIndex = Random.Range(i, list.Count);
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }

    private void SpawnTiles(List<GameObject> tilesToSpawn)
    {
        if (tilesToSpawn.Count != rows * cols)
        {
            Debug.LogError("Tile count does not match grid size (should be 18)!");
            return;
        }

        // Compute grid spacing inside the bounding box
        Vector3 min = boxCenter - (boxScale * 0.5f);
        Vector3 step = new Vector3(boxScale.x / (cols - 1), 0, boxScale.z / (rows - 1));

        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 spawnPos = new Vector3(
                    min.x + step.x * c,
                    boxCenter.y, // keep Y fixed
                    min.z + step.z * r
                );

                GameObject tile = Instantiate(tilesToSpawn[index], transform);
                tile.transform.position = spawnPos;
                index++;
            }
        }
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
