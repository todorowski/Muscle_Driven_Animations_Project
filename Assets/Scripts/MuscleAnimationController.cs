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
    
    void Start()
    {
        Debug.Log(footEdges[0].transform.position);
        rigidbodyList = ragdoll.GetComponentsInChildren<Rigidbody>().ToList();
        ragdollMuscles = ragdoll.GetComponentsInChildren<MuscleWithAnim>().ToList();

        
    }

    void LateUpdate()
    {
        CheckIfCoMIsBalanced();
    }

    void Update()
    {
        
        CalculateCenterOfMass();
        Test2();
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
            rb.AddForceAtPosition(-transform.right * 50f, forcePos.position);
        }

        GameObject g2 = GameObject.Find("LowerLeg2");
        Rigidbody rb2 = g2.GetComponent<Rigidbody>();
        if (Input.GetKey(KeyCode.R))
        {
            rb2.AddForceAtPosition(-transform.right * 70f, forcePos2.position);
        }
    }
}

