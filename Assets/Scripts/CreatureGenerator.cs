using UnityEngine;
using System.Collections.Generic;

public class CreatureGenerator : MonoBehaviour
{
    [SerializeField]
    List<GameObject> creaturePrefabs;

    [SerializeField]
    Transform topleftLimit, bottomrightLimit;

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
        int rand = Random.Range(0, creaturePrefabs.Count);
        Vector3 startPos = new Vector3(Random.Range(topleftLimit.position.x, bottomrightLimit.position.x),
                                       Random.Range(bottomrightLimit.position.y, bottomrightLimit.position.y));
        GameObject newCreature = Instantiate(creaturePrefabs[rand], startPos, Quaternion.identity);
    }
}
