using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Rigidbody rb1;
    public Rigidbody rb2;

    public Transform a1;
    public Transform a2;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    //not use rigidbody pos to get dir, use transform pos
    // Update is called once per frame
    void FixedUpdate()
    {
        rb1.AddForceAtPosition((a2.transform.position - a1.transform.position), a1.transform.position);
        rb2.AddForceAtPosition((a1.transform.position - a2.transform.position), a2.transform.position);
        Debug.Log("Force rb1: " + 10f * (a2.transform.position - a1.transform.position));
        Debug.Log("Force rb2: " + 10f * (a1.transform.position - a2.transform.position));

        //Draw ray
        //point from one ball to th eother
        Debug.DrawRay(a2.transform.position, 10f * (a1.transform.position - a2.transform.position), Color.cyan);
        Debug.DrawRay(a1.transform.position, 10f * (a2.transform.position - a1.transform.position), Color.magenta);
    }
}
