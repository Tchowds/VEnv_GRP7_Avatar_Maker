using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed;
    private bool isSpinning = false;

    void Update()
    {
        if (isSpinning)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    public void SetSpinning(bool shouldSpin)
    {
        isSpinning = shouldSpin;
    }
}

