using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;

    private void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if (existingObjects.Length == 0)
        {
            //If there is a grid, then spawn the Player at its centre
            var spawnPos = new Vector3(0, 0, 0); //Change this in future to have a set Spawn point for the Player instead of the centre

            var grid = FindObjectOfType<Grid>();
            if (grid != null)
            {
                spawnPos = grid.transform.position;
            }

            Instantiate(essentialObjectsPrefab, spawnPos, Quaternion.identity);

        }
    }
}
