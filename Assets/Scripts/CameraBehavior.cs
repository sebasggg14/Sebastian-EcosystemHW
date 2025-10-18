using UnityEngine;
using System.Collections.Generic;

public class CameraBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private List<Transform> instantiatedCreatures = new List<Transform>();

    [SerializeField]
    float smoothValue;

    Vector3 velocity = Vector3.zero;
    Vector3 startPos;

    [SerializeField]
    float newCameraSize = 10.0f;

    [SerializeField]
    float cameraLerpSpeed = 1.0f;

    public SeagullSpawner seagullSpawnerRef;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instantiatedCreatures.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        instantiatedCreatures.Clear();

        GameObject[] seagulls = GameObject.FindGameObjectsWithTag("Seagull");
        GameObject[] whales = GameObject.FindGameObjectsWithTag("Whale");

        foreach (GameObject obj in seagulls)
        {
            instantiatedCreatures.Add(obj.transform);   
        }
        foreach (GameObject obj in whales)
        {
            instantiatedCreatures.Add(obj.transform);
        }

        if (instantiatedCreatures.Count == 0)
        {
            return;
        }

        Vector3 targetPos = Vector3.zero;

        if (instantiatedCreatures.Count == 1)
        {
            targetPos = instantiatedCreatures[0].transform.position;
        }
        else
        {
            int validCount = 0;
            foreach (Transform creature in instantiatedCreatures)
            {
                if (creature == null) continue; 
                targetPos += creature.position;
                validCount++;
            }

            if (validCount == 0)
                return;

            targetPos /= validCount;
        }
        targetPos.z = -16f;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothValue);

        if (seagullSpawnerRef.isSpawned == true)
        {
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, newCameraSize, Time.deltaTime * cameraLerpSpeed);
        }
    }
}
