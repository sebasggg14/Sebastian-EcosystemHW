using UnityEngine;
using System.Collections.Generic;

public class SeagullBehaviour : MonoBehaviour
{
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //[SerializeField]
    List<Transform> possibleTargets; //array of spots the Seagull can idle between

    List<GameObject> deathTarget = new List<GameObject>();

    [SerializeField]
    float lerpTimeMax; //appr. how long we want a lerp to last

    [SerializeField]
    AnimationCurve idleWalkCurve; //anim curve to add easing to the lerp

    [SerializeField]
    float hungerStep; //max wait to deincrement hunger

    float deathCounter = 100f;
    float deathTick;

    Transform target = null; //current spot we're moving towards
    Vector3 startPos = Vector3.zero; //current spot we're moving from

    //count of current lerp progress
    float lerpTime;

    //enum is like a custom variable type
    //we're using it to make states for our Seagull's behavior
    enum SeagullStates
    {
        diving,
        perching,
        dying,
        idling
    }

    //current state
    SeagullStates state = SeagullStates.idling;

    //timer that'll count down for hunger
    float hungerTime;
    //hunger stat
    float hungerVal = 18;

    //list for food (diving spots) currently in the scene
    List<GameObject> allFood = new List<GameObject>();

    //holds which game object the Seagull has touched
    GameObject touchingObj;

    bool returningToSky = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        possibleTargets = new List<Transform>(); //list of spots the Seagull can idle between
        //agent.updateRotation = false;
        //agent.updateUpAxis = false;
        FindAllFood(); //find all food/dive spot objs in the scene
        FindDeathTarget();
        hungerTime = hungerStep; //reset our hunger timer
        GameObject[] seagullTargets = GameObject.FindGameObjectsWithTag("SeagullTarget");
        foreach (GameObject obj in seagullTargets)
        {
            possibleTargets.Add(obj.transform);
        }
    }

    void Update()
    {

        switch (state)
        {
            case SeagullStates.idling: //if we're in the idle state
                RunIdle(); //run idle code
                break;
            case SeagullStates.diving: //if we're in the eating state
                RunEat(); //run eating code
                break;
            case SeagullStates.perching:
                break;
            case SeagullStates.dying:
                RunDeath();
                break;
            default:
                break;
        }
    }

    void RunIdle()
    {
        animator.SetBool("isFlying", true);
        StepNeeds();
        if (hungerVal <= 0)
        {
            target = null;
            state = SeagullStates.diving;
            Debug.Log("hungryyyy switching to eating state");
            return;
        }

        Transform previousTarget = target;

        if (target == null)
        { //if we do not have a target to move to
            Transform newTarget = null;

            do
            {
                newTarget = possibleTargets[Random.Range(0, possibleTargets.Count)];
            }
            while (newTarget == previousTarget);

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
                Debug.Log("target reached, look for new target now");
            }
        }

        if (deathCounter < deathTick)
        {
            target = null;
            Debug.Log("death to the seagull");
            state = SeagullStates.dying;
        }
    }

    void RunEat()
    {
        // Start diving animation
        animator.SetBool("isFlying", false);
        animator.SetBool("isDiving", true);

        // pcik food
        if (target == null && !returningToSky)
        {
            target = FindNearest(allFood);
            if (target == null)
            {
                Debug.Log("No food found, returning to idle");
                state = SeagullStates.idling;
                animator.SetBool("isDiving", false);
                return;
            }

            // --- Face the dive target immediately 
            FaceTarget(target.position);

            startPos = transform.position;
            lerpTime = 0;
        }

        // Diving phase — move toward food
        if (!returningToSky)
        {
            transform.position = Move(1f);

            if (touchingObj != null && touchingObj.CompareTag("SeagullDiveTarget"))
            {
                Debug.Log("Touched dive target, eating...");
                hungerVal = 18;

                // remove eaten food from list
                allFood.Remove(touchingObj);
                Destroy(touchingObj);
                touchingObj = null;

                // Prepare to return to sky
                returningToSky = true;
                target = possibleTargets[Random.Range(0, possibleTargets.Count)];

                // --- face the new upward target immediately
                FaceTarget(target.position);

                startPos = transform.position;
                lerpTime = 0;
                animator.SetBool("isDiving", false);
            }
        }

        animator.SetBool("isRising", true);
        // returning to sky phase
        if (returningToSky && target != null)
        {

            transform.position = Move(1f);

            if (Vector3.Distance(transform.position, target.position) < 0.05f)
            {
                animator.SetBool("isRising", false);
                Debug.Log("Returned to sky, switching to idle");
                returningToSky = false;
                target = null;
                state = SeagullStates.idling;
            }
        }
    }

    void RunDeath()
    {
        if (target == null)
        {
            target = FindNearest(deathTarget);
            startPos = transform.position;
            lerpTime = 0;
        }

        transform.position = Move();

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < 0.05f)
        {
            Debug.Log("Seagull destroyed");
            Destroy(gameObject, 1f);
        }
    }


    void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f; // ignore tilt

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            lookRotation *= Quaternion.Euler(0f, -90f, 0f); // adjust model's forward if needed
            transform.rotation = lookRotation;
        }
    }

    void StepNeeds()
    {
        hungerTime -= Time.deltaTime; //deincrement the hunger timer
        deathTick += Time.deltaTime; // increment death timer 
        if (hungerTime <= 0)
        { //if the hunger timer gets to 0
            hungerVal--; //decrease our hunger stat
            hungerTime = hungerStep; //reset the hunger timer
        }
    }

    void FindAllFood()
    {
        allFood.AddRange(GameObject.FindGameObjectsWithTag("SeagullDiveTarget")); //find all objs tagged food and put them in a list
    }

    void FindDeathTarget()
    {
        deathTarget.AddRange(GameObject.FindGameObjectsWithTag("SeagullDeathTarget"));
    }

    void ResetTarget()
    {
        target = null;
        Debug.Log("target has been reset");
    }

    Transform FindNearest(List<GameObject> objsToFind)
    {
        float minDist = Mathf.Infinity; //setting the min dist to a big number
        Transform nearest = null; //tracks the obj closest to us
        for (int i = 0; i < objsToFind.Count; i++)
        { //loop through the objects we're checking
            float dist = Vector3.Distance(transform.position, objsToFind[i].transform.position); //check the dist b/t the Seagull and the current obj
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
        lerpTime += Time.deltaTime; //increase progress by delta time
        float percent = idleWalkCurve.Evaluate(lerpTime / lerpTimeMax); //from progress on curve
        Vector3 newPos = Vector3.LerpUnclamped(startPos, target.position, percent); //find current lerped position

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            lookRotation *= Quaternion.Euler(0f, -90f, 0f); // -90f because the model is on default facing right 
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, percent); // Quaternion.Slerp is basically lerp for rotation (spherical lerp)
        }
        return newPos; //return the new position
    }

    Vector3 Move(float speed) //// overloaded function specifically for the seaagull dive
    {
        lerpTime += Time.deltaTime; //increase progress by delta time
        float percent = idleWalkCurve.Evaluate(lerpTime / speed); //from progress on curve
        Vector3 newPos = Vector3.LerpUnclamped(startPos, target.position, percent); //find current lerped position

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            lookRotation *= Quaternion.Euler(0f, -90f, 0f); // -90f because the model is on default facing right 
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, percent); // Quaternion.Slerp is basically lerp for rotation (spherical lerp)
        }
        return newPos; //return the new position
    }


    void OnTriggerEnter(Collider col)
    {
        if (col != null) touchingObj = col.gameObject; //if we touch something, set the var to whatever that thing is
    }

    void OnTriggerExit(Collider col)
    {
        if (col != null)
        { //if we stop touching something 
            if (col.gameObject == touchingObj) touchingObj = null; //AND that thing is being tracked, clear the touching tracking var
        }
    }
}
