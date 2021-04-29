using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoMController : MonoBehaviour
{
    public GameObject com;
    public float force;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = com.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            rb.AddForce(transform.right * force, ForceMode.Impulse);
        }
    }


}
