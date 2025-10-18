using UnityEngine;
using System.Collections.Generic;

public class SeagullSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject seagullPrefab;

    [SerializeField]
    Transform spawnLocation;
    
    public bool isSpawned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("CreateStartCreatures", Random.Range(10f, 30f));
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateStartCreatures()
    {
        //int rand = Random.Range(0, seagullPrefab.Count);
        Vector3 startPos = new Vector3(spawnLocation.position.x, spawnLocation.position.y);
        GameObject newCreature = Instantiate(seagullPrefab, startPos, Quaternion.identity);
        isSpawned = true;

        Invoke("CreateStartCreatures", Random.Range(130f, 140f));
    }
}