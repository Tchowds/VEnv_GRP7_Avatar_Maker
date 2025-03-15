using UnityEngine;
using Ubiq.Messaging;
using System.Collections;
using System.Collections.Generic;

public class TailorNPC : MonoBehaviour
{
    public List<Transform> defaultLookTargets; 
    public int lookDefaultMinInterval = 5;
    public int lookDefaultMaxInterval = 10;

    public float lookDefaultLookSpeed = 2f; 

    public float playerDetectedLookSpeed = 5f;

    private float lookSpeed = 5f;
    public float detectionRadius = 5f; 
    public LayerMask playerLayer; // Ensure it only detects players

    private Transform currentTarget;

    private Coroutine randomLookCoroutine;

    public GameObject playerXRig; 
    private Transform playerHead;
    void Start()
    {

        if (playerXRig != null)
        {
            playerHead = playerXRig.transform.Find("Camera Offset/Main Camera");
        }

        StartCoroutine(UpdateLookTarget());
        randomLookCoroutine = StartCoroutine(RandomLookRoutine());
    }

    IEnumerator UpdateLookTarget()
    {
        while (true)
        {
            LookIfClose();
            yield return new WaitForSeconds(0.5f); 
        }
    }

    IEnumerator RandomLookRoutine()
    {
        while (true)
        {
            currentTarget = defaultLookTargets[Random.Range(0, defaultLookTargets.Count)];
            yield return new WaitForSeconds(Random.Range(lookDefaultMinInterval, lookDefaultMaxInterval));
        }
    }


    void LookIfClose()
    {
        float distance = Vector3.Distance(playerXRig.transform.position, transform.position);
        if (distance < detectionRadius)
        {
            currentTarget = playerHead;
            lookSpeed = playerDetectedLookSpeed;

            if (randomLookCoroutine != null)
            {
                StopCoroutine(randomLookCoroutine);
                randomLookCoroutine = null;
            }
        }
        else
        {
            if (randomLookCoroutine == null)
            {
                randomLookCoroutine = StartCoroutine(RandomLookRoutine());
                lookSpeed = lookDefaultLookSpeed;
            }
        }
    }

    void Update()
    {
        Quaternion targetRotation = Quaternion.LookRotation(currentTarget.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }

}