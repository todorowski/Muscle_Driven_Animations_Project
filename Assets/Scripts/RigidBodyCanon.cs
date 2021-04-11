using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyCanon : MonoBehaviour
{
    public Rigidbody bullet;
    public float force;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Rigidbody bulletInstance = Instantiate(bullet);
            bulletInstance.transform.position = this.transform.position;
            bulletInstance.velocity = Camera.main.transform.forward * force;

        }
    }
}
