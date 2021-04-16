using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MuscleAnimationController : MonoBehaviour
{
    public MuscleAnimation animationObject;
    public GameObject endEffector_r;
    public GameObject endEffector_l;

    public Transform[] rightEdges;
    public Transform[] leftEdges;
    public Transform origin;

    public Transform ragdoll;
    public Transform forcePos1;
    public Transform forcePos2;
    public Transform footPos1;
    public Transform footPos2;

    //Start of the animation timeline
    public float animationHead = 0.0f;

    //Initial center of mass
    Vector3 CoM = Vector3.zero;

    List<MuscleWithAnim> ragdollMuscles = new List<MuscleWithAnim>();
    List<Rigidbody> rigidbodyList;
    bool isBalanced;

    //For tests
    GameObject test; 
    Rigidbody tb;
    
    void Start()
    {
        rigidbodyList = ragdoll.GetComponentsInChildren<Rigidbody>().ToList();
        ragdollMuscles = ragdoll.GetComponentsInChildren<MuscleWithAnim>().ToList();
        test = GameObject.Find("Body");
        tb = test.GetComponent<Rigidbody>();
    }

    void Update()
    {
        //tb.AddForceAtPosition(Vector3.up * (150f), origin.transform.position);
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

        CalculateCenterOfMass();

        if (CheckIfCoMIsBalanced() == false)
        {
            Rebalance(footPos1, footPos2, CoM);
        }


    }

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

    bool CheckIfCoMIsBalanced()
    {
        RaycastHit hit;
        bool balanced;
        if(Physics.Raycast(CoM, -origin.up, out hit, Mathf.Infinity, LayerMask.GetMask("SupportPolygon")))
        {
            Debug.Log("balanced");
            balanced = true;
        }
        else
        {
            Debug.Log("Not balanced");
            balanced = false;
        }
        Debug.DrawRay(CoM, -origin.up, Color.cyan);
        return balanced;
        
    }

    //Get the point which the CoM is the closest to 
    Vector3 GetDirectionToMove(Transform[] edges)
    {
        Vector3 dirToMove = new Vector3(0,0,0);
        if((edges[0].position - CoM).magnitude < (edges[1].position - CoM).magnitude)
        {
            dirToMove = edges[1].position - edges[0].position;
        }
        else
        {
            dirToMove = edges[0].position - edges[1].position;
        }

        return dirToMove;
    }

    void Rebalance(Transform pos1, Transform pos2, Vector3 CoM)
    {
        GameObject g;
        Transform[] edges;
        Transform forcepos;

        //Check which foot is furthest from CoM
        if ((CoM - endEffector_r.transform.position).magnitude < (CoM - endEffector_l.transform.position).magnitude)
        {
            g = endEffector_l;
            edges = leftEdges;
            forcepos = forcePos1;
        }
        else
        {
            g = endEffector_r;
            edges = rightEdges;
            forcepos = forcePos2;
        }

        Vector3 dirToMove = GetDirectionToMove(edges);
        Debug.DrawRay(g.transform.position, dirToMove * 10f, Color.blue);
        Rigidbody rb = g.GetComponent<Rigidbody>();
        rb.AddForceAtPosition(dirToMove * 67.5f, forcepos.position, ForceMode.Impulse);

    }
}

