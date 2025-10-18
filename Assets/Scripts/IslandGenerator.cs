using UnityEngine;
using System.Collections.Generic;

public class IslandGenerator : MonoBehaviour
{
    [SerializeField]
    List<GameObject> islandPrefabs;

    [SerializeField]
    Transform islandSpawnPosition;

    [SerializeField]
    Transform islandTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("IslandCreate", Random.Range(5, 20));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void IslandCreate()
    {
        Quaternion islandRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        int rand = Random.Range(0, islandPrefabs.Count);
        Vector3 startPos = new Vector3(islandSpawnPosition.position.x, islandSpawnPosition.position.y, islandSpawnPosition.position.z);
        GameObject newIsland = Instantiate(islandPrefabs[rand], startPos, islandRotation);

        IslandBehaviour islandScript = newIsland.GetComponent<IslandBehaviour>();
        islandScript.SelectTarget(islandTarget);


        Debug.Log("island has been created");
        Invoke("IslandCreate", Random.Range(60f, 120f));
    }
}
