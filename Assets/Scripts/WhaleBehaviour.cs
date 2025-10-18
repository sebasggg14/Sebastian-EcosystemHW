using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class WhaleBehaviour : MonoBehaviour
{
    //public UnityEngine.AI.NavMeshAgent agent;

    private Animator animator;

    [SerializeField]
    Transform[] possibleTargets; //array of spots the spider can idle between

    List<GameObject> activeIsland = new List<GameObject>(); // list of active islands in the scene 

    [SerializeField]
    float lerpTimeMax; //appr. how long we want a lerp to last

    [SerializeField]
    AnimationCurve idleWalkCurve; //anim curve to add easing to the lerp

    [SerializeField]
    float hungerStep; //max time we want to wait to deincrement hunger

    Transform target = null; //current spot we're moving towards
    Vector3 startPos = Vector3.zero; //current spot we're moving from

    //count of current lerp progress
    float lerpTime;

    //enum is like a custom variable type
    //we're using it to make states for our spider's behavior
    public enum WhaleStates
    {
        eating,
        playing,
        dying,
        idling,
        pause
    }

    //current state
    WhaleStates state = WhaleStates.idling;

    //timer that'll count down for hunger
    float hungerTime;
    //hunger stat
    float hungerVal = 10;

    //list for food currently in the scene
    List<GameObject> allFood = new List<GameObject>();

    //holds which game object the spider has touched
    GameObject touchingObj;

    //could use to display organism stats for debugging. should NOT be in the final game
    // [SerializeField]
    // TMP_Text hungerText;
    // variables for playing state

    // stuff for rotation /playing state
    float rotationSpeed = 1f;
    float radius = 5f;
    float angle = 0f;
    public float spinCount = 0f;
    public bool finishedPlay = false;
    private bool movingToOrbit = false;
    private Vector3 orbitStartPos;
    private float orbitLerpTime = 0f; // lerp progress for approach
    private GameObject currentIsland = null;
    private bool isPlayLocked = false; // so the play cannot be interrupted by hungry state

    [SerializeField] 
    float orbitApproachDuration = 1.0f; // time to move to orbit start

    [SerializeField] 
    float playCooldown = 5f; // how long before whale can play again

    private float playCooldownTimer = 0f;      // internal countdown
    public bool startBobbing = false;



    void Start()
    {
        //agent.updateRotation = false;
        //agent.updateUpAxis = false;
        FindAllFood(); //find all food objs in the scene
        hungerTime = hungerStep; //reset our hunger timer
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //hungerText.text = hungerVal.ToString();
        //switch statement for our statement
        //cleaner way of checking the same condition

        if (playCooldownTimer > 0f)
        {
            playCooldownTimer -= Time.deltaTime;
        }

        switch (state)
        {
            case WhaleStates.pause: // ended up not using this 
                RunPause(); // 
                break;
            case WhaleStates.idling: //if we're in the idle state
                RunIdle(); //run idle code
                break;
            case WhaleStates.eating: //if we're in the eating state
                RunEat(); //run eating code
                break;
            case WhaleStates.playing:
                RunPlay();
                break;
            case WhaleStates.dying:
                break;
            default:
                break;
        }

        GameObject [] islands = GameObject.FindGameObjectsWithTag("Island");

        if (islands.Length > 0)
        {
            foreach (GameObject island in islands)
            {
                activeIsland.Clear();
                activeIsland.Add(island);
            }
        }
    }

    void RunPause()
    {
        StartCoroutine(WaitCoroutine());
        state = WhaleStates.idling;
    }

    void RunIdle()
    {
        if (isPlayLocked) // whalee cannot idle if play is happening
        {
            return;
        }
        
        StepNeeds();
        if (hungerVal <= 0)
        {
            target = null;
            state = WhaleStates.eating;
            Debug.Log("hungryyyy switching to eating state");
            return;
        }

        Transform previousTarget = target;

        if (target == null)
        { //if we do not have a target to move to
            Transform newTarget = null;

            do
            {
                newTarget = possibleTargets[Random.Range(0, possibleTargets.Length)];
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
    }

    void RunPlay()
    {
        if (!isPlayLocked)
        {
            isPlayLocked = true;
        }
        
        if (target == null)
        {
            Debug.LogWarning("RunPlay called but no target assigned");
            state = WhaleStates.idling;
            return;
        }

        //  approach orbit start position ---
        if (movingToOrbit)
        {
            orbitLerpTime += Time.deltaTime;
            float t = Mathf.Clamp01(orbitLerpTime / orbitApproachDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, orbitStartPos, eased);

            // tilt
            Vector3 dirToCenter = (target.position - transform.position);
            dirToCenter.y = 0f; // remove rotate vertical component
            if (dirToCenter.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dirToCenter.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 2f);
            }

            // switch to orbit
            if (t >= 1f)
            {
                movingToOrbit = false;
                orbitLerpTime = 0f;
            }

            return; // wait until approach finished
        }

        // now orbit around island
        // compute smooth orbit target

        startBobbing = true;

        float x = target.position.x + Mathf.Cos(angle) * radius;
        float y = target.position.y - 3f;
        float z = target.position.z + Mathf.Sin(angle) * radius;
        Vector3 orbitPos = new Vector3(x, y, z);

        // move smoothly toward the orbit position
        transform.position = Vector3.Lerp(transform.position, orbitPos, Time.deltaTime * 3f);

        Vector3 direction = (target.position - transform.position);
        direction.y = 0f; // remove vertical component (again)
        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);
        }

        // advance angle and count full rotations, 2pi = 360 degrees
        angle += rotationSpeed * Time.deltaTime;
        if (angle >= Mathf.PI * 2f)
        {
            angle -= Mathf.PI * 2f;
            spinCount++;
            Debug.Log("Completed orbit #" + spinCount);
        }

        // finish playing after 2 full rotations
        if (spinCount >= 2)
        {
            startBobbing = false;
            spinCount = 0;
            finishedPlay = true;
            target = null; // clear target (or keep depending on behavior)
            state = WhaleStates.idling;
            isPlayLocked = false;
            playCooldownTimer = playCooldown;
        }
    }

    //IEnumerator RotateAroundIsland() // original code, used tutorial
    //{
    //    float x = target.position.x + Mathf.Cos(angle) * radius;
    //    float y = target.position.y;
    //    float z = target.position.z + Mathf.Sin(angle) * radius;

    //    transform.position = new Vector3(x, y, z);

    //    angle += rotationSpeed * Time.deltaTime;

    //    yield return null;
    //}

    void RunEat()
    {
        if (isPlayLocked)
        {
            return;
        }
        
        FindAllFood();
        animator.SetBool("isEating", true);
        
        if (target == null)
        { //if we do not have a target to move to
            target = FindNearest(allFood); //find the closest food obj and set our target to it
            startPos = transform.position; //set our starting pos to our current pos
            lerpTime = 0; //reset our lerp progress
        }
        else
        {
            transform.position = Move(); //move to food
            
            if (Vector3.Distance(transform.position, target.position) < 2f)
            {

                Debug.Log("ateee!!");
                hungerVal = 10; //reset our hunger
                StartCoroutine(DestroyFood(target.gameObject)); //destroy that food
                target = null; //empty our movement target
                Debug.Log("switch to idling");
                animator.SetBool("isEating", false);
                state = WhaleStates.idling; //switch the state to idle
                
            }
        }
    }

    void StepNeeds()
    {
        hungerTime -= Time.deltaTime; //deincrement the hunger timer
        if (hungerTime <= 0)
        { //if the hunger timer gets to 0
            hungerVal--; //decrease our hunger stat
            hungerTime = hungerStep; //reset the hunger timer
        }
    }

    void FindAllFood()
    {
        allFood.Clear(); // clear old references
        allFood.AddRange(GameObject.FindGameObjectsWithTag("food")); //find all objs tagged food and put them in a list
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

        objsToFind.RemoveAll(item => item == null);

        for (int i = 0; i < objsToFind.Count; i++)
        { //loop through the objects we're checking
            if (objsToFind[i] == null)
            {
                continue;
            }
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

    void OnTriggerEnter(Collider collider)
    {
        if (playCooldownTimer > 0f)
        {
            Debug.Log("skipping island play");
            return;
        }
        if (!collider.CompareTag("Island"))
        {
            return;
        }
        if ((state == WhaleStates.playing) || (state == WhaleStates.eating) || (state == WhaleStates.dying))
        {
            Debug.Log("Estoy occupado");
            return;
        }
        if (collider.CompareTag("Island") && playCooldownTimer <= 0f)
        {
            
            if (currentIsland != null && currentIsland != collider.gameObject)
            {
                Debug.Log("already playing with an island");
                return;
            }
            
            Debug.Log("island detected");
            currentIsland = collider.gameObject;

            // set the island transform as the target
            target = collider.transform;

            // set a random starting angle so orbit doesn't always start at same point
            angle = Random.Range(0f, Mathf.PI * 2f);

            // compute the orbit start position from that angle
            orbitStartPos = new Vector3(
                target.position.x + Mathf.Cos(angle) * radius,
                target.position.y - 3f,
                target.position.z + Mathf.Sin(angle) * radius
            );

            // begin approach phase
            movingToOrbit = true;
            orbitLerpTime = 0f;

            // record current position as approach start
            startPos = transform.position;

            state = WhaleStates.playing;
        }
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null) touchingObj = col.gameObject; //if we touch something, set the var to whatever that thing is
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col != null)
        { //if we stop touching something 
            if (col.gameObject == touchingObj) touchingObj = null; //AND that thing is being tracked, clear the touching tracking var
        }
    }

    IEnumerator WaitCoroutine()
    {
        Debug.Log("start coroutine");
        yield return new WaitForSeconds(1f);
        Debug.Log("pause complete");
        yield break;
    }

    IEnumerator DestroyFood(GameObject fish)
    {
        ParticleSystem ps = fish.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop();
            yield return new WaitForSeconds(ps.main.startLifetime.constantMax);
        }
        Destroy(fish);
    }
}
