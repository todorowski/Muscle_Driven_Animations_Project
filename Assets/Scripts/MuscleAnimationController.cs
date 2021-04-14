using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MuscleAnimationController : MonoBehaviour
{
    public MuscleAnimation animationObject;
    public Transform ragdoll;
    //public GameObject showCoM;
    public Transform[] footEdges;
    public Transform origin;

    public Transform forcePos;
    public Transform forcePos2;

    //Start of the animation timeline
    public float animationHead = 0.0f;

    Vector3 CoM = Vector3.zero;
    List<MuscleWithAnim> ragdollMuscles = new List<MuscleWithAnim>();
    List<Rigidbody> rigidbodyList;
    bool isBalanced;

    public Transform footPos1;
    public Transform footPos2;

    GameObject test; 
    Rigidbody tb;
    
    void Start()
    {
        Debug.Log(footEdges[0].transform.position);
        rigidbodyList = ragdoll.GetComponentsInChildren<Rigidbody>().ToList();
        ragdollMuscles = ragdoll.GetComponentsInChildren<MuscleWithAnim>().ToList();
        test = GameObject.Find("Body");
        tb = test.GetComponent<Rigidbody>();

        
    }

    void LateUpdate()
    {
        
    }

    void Update()
    {
        //tb.AddForceAtPosition(Vector3.up * (150f), origin.transform.position);
        CalculateCenterOfMass();
        Test2();

        CheckIfCoMIsBalanced();

        Vector3 CoMVector = CoM - footPos1.position;
        Debug.DrawLine(CoM, footPos1.position - CoM, Color.red);
    }

    void FixedUpdate()
    {
        //Set target length according to anim curve and activate muscles
        foreach (MuscleAnimationStruct mStruct in animationObject.muscles)
        {
            foreach(MuscleWithAnim m in ragdollMuscles)
            {
                if(m.name == mStruct.muscleName)
                {
                    m.targetLength = mStruct.animCurve.Evaluate(animationHead);
                    m.Activate();
                }
            }
        }
        animationHead += Time.fixedDeltaTime;

     
    }

    //Check if CoM is balanced
    //TODO: Change to SAT to support all shapes
    //Or raycast on collider

    Vector3 CalculateCenterOfMass()
    {
        float c = 0f;

        foreach (Rigidbody body in rigidbodyList)
        {
            CoM += body.worldCenterOfMass * body.mass;
            c += body.mass;
        }

        CoM /= c;
        return CoM;
    }

    void CheckIfCoMIsBalanced()
    {
        RaycastHit hit;
        if(Physics.Raycast(CoM, -origin.up, out hit, Mathf.Infinity, LayerMask.GetMask("SupportPolygon")))
        {
            Debug.Log("balanced");
        }
        else
        {
            Debug.Log("Not balanced");
        }
        Debug.DrawRay(CoM, -origin.up, Color.cyan);
    }

    void Test()
    {
        if (Input.GetKey(KeyCode.E))
        {
            foreach (MuscleAnimationStruct mStruct in animationObject.muscles)
            {
                foreach (MuscleWithAnim m in ragdollMuscles)
                {
                    if (m.name == "Ankle1")
                    {
                        Debug.Log("HEEEEEEEEJ");
                        m.targetLength = 100f;
                        m.Activate();
                    }
                }
            }
        }
    }

    void Test2()
    {
        GameObject g = GameObject.Find("LowerLeg1");
        Rigidbody rb = g.GetComponent<Rigidbody>();
        if (Input.GetKey(KeyCode.E))
        {
            rb.AddForceAtPosition(-transform.right * 10f, forcePos.position);
        }

        GameObject g2 = GameObject.Find("LowerLeg2");
        Rigidbody rb2 = g2.GetComponent<Rigidbody>();
        if (Input.GetKey(KeyCode.R))
        {
            rb2.AddForceAtPosition(-transform.right * 10f, forcePos2.position);
        }
    }

    void rebalance(Transform pos1, Transform pos2, Transform CoM)
    {
        Vector3 newPos = new Vector3(0,0,0);

        if(pos1.position.magnitude - CoM.position.magnitude > pos2.position.magnitude - CoM.position.magnitude)
        {
            
        }

  

        Vector3 targetPosition = new Vector3();
        GameObject g = GameObject.Find("LowerLeg1");
        Rigidbody rb = g.GetComponent<Rigidbody>();

        GameObject g2 = GameObject.Find("LowerLeg2");
        Rigidbody rb2 = g2.GetComponent<Rigidbody>();

        if(rb.transform.position != footPos1.position)
        {
            rb.AddForceAtPosition(-(rb.transform.position - footPos1.position) * 30f, forcePos.position);
        }
        else
        {
            Debug.Log("At pos");
        }

        if (rb2.transform.position != footPos2.position)
        {
            rb2.AddForceAtPosition(-(rb2.transform.position - footPos2.position) * 30f, forcePos2.position);
        }
        else
        {
            Debug.Log("At pos");
        }
    }
}

