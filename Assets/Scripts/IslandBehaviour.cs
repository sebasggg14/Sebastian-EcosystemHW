using UnityEngine;
using System.Collections.Generic;

public class IslandBehaviour : MonoBehaviour
{
    private Animator animator;

    bool isBeingPlayedWith = false;

    //public UnityEngine.AI.NavMeshAgent agent;
    private Transform islandTarget;

    WhaleBehaviour whaleScript;

    [SerializeField]
    float lerpTimeMax; //appr. how long we want a lerp to last

    [SerializeField]
    AnimationCurve movementCurve; //anim curve to add easing to the lerp

    Transform target = null; //current spot we're moving towards
    Vector3 startPos = Vector3.zero; //current spot we're moving from

    //count of current lerp progress
    float lerpTime;

    //enum is like a custom variable type
    //we're using it to make states for our spider's behavior
    public enum IslandStates
    {
        moving,
        playing,
        destroy
    }

    //current state
    IslandStates state = IslandStates.moving;

    //holds which game object the spider has touched
    GameObject touchingObj;

    //could use to display organism stats for debugging. should NOT be in the final game
    // [SerializeField]
    // TMP_Text hungerText;

    void Start()
    {
        //agent.updateRotation = false;
        //agent.updateUpAxis = false;
        animator = GetComponent<Animator>();
        GameObject whaleCreature = GameObject.Find("Whale_Creature 1");
        if (whaleCreature != null)
        {
            whaleScript = whaleCreature.GetComponent<WhaleBehaviour>();
        }
    }

    void Update()
    {
        //hungerText.text = hungerVal.ToString();
        //switch statement for our statement
        //cleaner way of checking the same condition
        switch (state)
        {
            case IslandStates.moving: //if we're in the idle state
                RunMove(); //run idle code
                break;
            case IslandStates.playing:
                RunPlay();
                break; 
            case IslandStates.destroy:
                DestroyIsland(); // run destroy code 
                break;
            default:
                break;
        }
    }

    void RunMove()
    {
        Transform previousTarget = target;

        if (target == null)
        { //if we do not have a target to move to
            Transform newTarget = null;
            newTarget = islandTarget;
            target = newTarget;
            startPos = transform.position;
            lerpTime = 0;
            //agent.SetDestination(target.position);
        }
        else
        {
            transform.position = Move(); //move to that position

            // check if the two objects collide 
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < 0.05f)
            {
                target = null;
                state = IslandStates.destroy;
                Debug.Log("target reached, look for new target now");
            }
        }
    }

    void RunPlay()
    {
        if (!isBeingPlayedWith)
        {
            return;
        }
        
        if (whaleScript.startBobbing)
        {
            animator.SetBool("isPlaying", true);
        }
        if (whaleScript.finishedPlay)
        {
            isBeingPlayedWith = false;
            state = IslandStates.moving;
            animator.SetBool("isPlaying", false);
            whaleScript.finishedPlay = false;
        }
    }

    void ResetTarget()
    {
        target = null;
        Debug.Log("target has been reset");
    }

    public void SelectTarget(Transform target)
    {
        islandTarget = target;
    }

    Transform FindNearest(List<GameObject> objsToFind)
    {
        float minDist = Mathf.Infinity; //setting the min dist to a big number
        Transform nearest = null; //tracks the obj closest to us
        for (int i = 0; i < objsToFind.Count; i++)
        { //loop through the objects we're checking
            float dist = Vector3.Distance(transform.position, objsToFind[i].transform.position); //check the dist b/t the spider and the current obj
            if (dist < minDist)
            { //if the dist is less than our currently tracked min dist
                minDist = dist; //set the min dist to the new dist
                nearest = objsToFind[i].transform; //set the nearest obj var to this obj
            }
        }
        return nearest; //return the closest obj
    }

    Vector3 Move()
    {
        lerpTime += Time.deltaTime; //increase progress by delta time (time b/t frames)
        float percent = movementCurve.Evaluate(lerpTime / lerpTimeMax); //from progress on curve
        Vector3 newPos = Vector3.LerpUnclamped(startPos, target.position, percent); //find current lerped position
        return newPos; //return the new position
    }

    void DestroyIsland()
    {
        Debug.Log("island destroyed");
        Destroy(gameObject, 1f);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Whale"))
        {
            Debug.Log("Whale detected");
            isBeingPlayedWith = true;
            state = IslandStates.playing;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Whale"))
        {
            isBeingPlayedWith = false;
            state = IslandStates.moving;
            animator.SetBool("isPlaying", false);
        }
    }
}
