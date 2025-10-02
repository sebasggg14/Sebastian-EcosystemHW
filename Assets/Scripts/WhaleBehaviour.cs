using UnityEngine;

public class WhaleBehaviour : MonoBehaviour
{
    [SerializeField]
    Transform[] possibleTargets; //array of spots the spider can idle between

    [SerializeField]
    float lerpTimeMax; //appr. how long we want a lerp to last

    [SerializeField]
    AnimationCurve idleWalkCurve; //anim curve to add easing to the lerp

    Transform target = null; //current spot we're moving towards
    Vector3 startPos = Vector3.zero; //current spot we're moving from

    //count of current lerp progress
    float lerpTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RunIdle();
        RunIdle();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector3 Move()
    {
        lerpTime += Time.deltaTime; //increase progress by delta time (time b/t frames)
        float percent = idleWalkCurve.Evaluate(lerpTime / lerpTimeMax); //from progress on curve
        Vector3 newPos = Vector3.LerpUnclamped(startPos, target.position, percent); //find current lerped position
        Debug.Log("moving works");
        return newPos; //return the new position
    }

    void RunIdle()
    {
        if (target == null)
        { //if we do not have a target to move to
            int newTarget = Random.Range(0, possibleTargets.Length); //find random position
            target = possibleTargets[newTarget]; //set target to that position
            startPos = transform.position; //set our starting pos to our current pos
            lerpTime = 0; //reset our lerp progress
            Debug.Log("target search works");
        }
        else
        {
            transform.position = Move(); //move to that position
        }
        //TO DO: find better thing to do while idle
        //Need to make all behavior be intentional
    }
}
