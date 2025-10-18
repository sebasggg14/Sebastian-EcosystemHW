using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CreatureSounds : MonoBehaviour
{
    [SerializeField] 
    AudioClip[] possibleSounds;

    AudioSource mySource;

    void Start()
    {
        mySource = GetComponent<AudioSource>();
        StartCoroutine(SoundPlayer());
    }

    IEnumerator SoundPlayer()
    {
        while (true)
        {
            mySource.PlayOneShot(possibleSounds[Random.Range(0, possibleSounds.Length)]);
            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }
}