using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Muscle : MonoBehaviour
{
    public Rigidbody rb1;
    public Rigidbody rb2;

    public Transform a1;
    public Transform a2;
    public MuscleWithAnim m;

    void FixedUpdate()
    {
        m.targetLength = 1;
        if (Input.GetKey(KeyCode.Space))
        {
            
            m.Activate();
        }
    }
}
