using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class FishSwarmGenerator : MonoBehaviour
{
    [SerializeField]
    List<GameObject> fishPrefabs;

    [SerializeField]
    Transform topleftLimit, bottomrightLimit;

    [SerializeField]
    int startCreatureNum;

    [SerializeField]
    int activeCreatureMin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateStartCreatures();
    }

    // Update is called once per frame
    void Update()
    {
        int currentFishCount = GameObject.FindGameObjectsWithTag("food").Length;

        if (activeCreatureMin > currentFishCount)
        {
            Debug.Log("respawning fish");
            CreateStartCreatures();
        }
    }

    void CreateStartCreatures()
    {
        for (int i = 0; i < startCreatureNum; i++)
        {
            int rand = Random.Range(0, fishPrefabs.Count);
            Vector3 startPos = new Vector3(Random.Range(topleftLimit.position.x, bottomrightLimit.position.x),
                                            Random.Range(bottomrightLimit.position.y, topleftLimit.position.y));
            GameObject newCreature = Instantiate(fishPrefabs[rand], startPos, Quaternion.identity);
        }

        return;
    }
}
