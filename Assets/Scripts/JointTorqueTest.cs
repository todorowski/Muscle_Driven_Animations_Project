using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class JointTorqueTest : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ragdoll;
    //List<CharacterJoint> joints = new List<CharacterJoint>();
    List<Rigidbody> rigidbodyList = new List<Rigidbody>();
    void Start()
    {
        //Fill list with ragdoll joints
        //joints = ragdoll.GetComponentsInChildren<CharacterJoint>().ToList();


        //Get all of the starting joint torques in the rigidbody
        for(int i = 0; i < rigidbodyList.Count; i++)
        {
            //Get instead the rigidbodies and the joints connected to the rigidbodies
            Debug.Log(i + " " + rigidbodyList[i].transform.eulerAngles);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < rigidbodyList.Count; i++)
        {
            Debug.Log(i + " " + rigidbodyList[i].transform.eulerAngles);
        }
    }
}
