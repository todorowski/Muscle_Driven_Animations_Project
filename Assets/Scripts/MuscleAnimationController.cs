using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MuscleAnimationController : MonoBehaviour
{
    public MuscleAnimation animationObject;
    //public GameObject endEffector_r;
    //public GameObject endEffector_l;
    //public GameObject supportPolyGen;

    //public Transform[] rightEdges;
    //public Transform[] leftEdges;

    public Transform ragdoll;
    //public Transform forcePos1;
    //public Transform forcePos2;
    //public Transform footPos1;
    //public Transform footPos2;
    //public float forceMultiplier;
    public float compensationMultiplier;

    public Transform pos;

    //Start of the animation timeline
    public float animationHead = 0.0f;

    //Initial center of mass
    Vector3 CoM = Vector3.zero;

    float leftFootTargetDist = 0.0f;
    float rightFootTargetDist = 0.0f;

    List<MuscleWithAnim> ragdollMuscles = new List<MuscleWithAnim>();
    List<Rigidbody> rigidbodyList;
    bool r_grounded, l_grounded;

    //For tests
    GameObject test; 
    Rigidbody tb;

    bool balancingLeft;
    bool balancingRight;
    
    void Start()
    {
        rigidbodyList = ragdoll.GetComponentsInChildren<Rigidbody>().ToList();
        ragdollMuscles = ragdoll.GetComponentsInChildren<MuscleWithAnim>().ToList();
        test = GameObject.Find("Body");
        tb = test.GetComponent<Rigidbody>();

        //CheckStartConditions();
    }

    private void Update()
    {
        CalculateCenterOfMass();
        //AddGravityCompensation();
    }

    /*void CheckStartConditions()
    {
        //3 raycasts
        //the DISTANCE between them not the positions in the world/on the polygon
        //the distance between the impact points in world space
        CoM = CalculateCenterOfMass();
        Vector3 CoMStartingPos;
        RaycastHit CoMHit;
        Debug.Log(CoM);
        if (Physics.Raycast(CoM, Vector3.down, out CoMHit, Mathf.Infinity, LayerMask.GetMask("SupportPolygon")))
        {
            if(CoMHit.collider != null)
            {
                CoMStartingPos = CoMHit.point;
                Debug.Log("CoM position: " + CoMStartingPos);
                Debug.DrawRay(CoM, Vector3.down, Color.magenta);

                leftFootTargetDist = (forcePos1.position - CoM).magnitude;
                rightFootTargetDist = (forcePos1.position - CoM).magnitude;
                Debug.Log("left foot distance to CoM: " + leftFootTargetDist);
                Debug.Log("right foot distance to CoM: " + rightFootTargetDist);
            }
        }
    }*/

    void FixedUpdate()
    {
        //CalculateCenterOfMass();
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

        

        /*if (CheckIfCoMIsBalanced() == false)
        {
            if((footPos1.position - CoM).magnitude > (footPos2.position - CoM).magnitude && balancingRight == false)
            {
                //rebalance left foot
                balancingLeft = true;
                RebalanceFoot(footPos1, endEffector_l, leftEdges, forcePos1, leftFootTargetDist);
                balancingLeft = false;
            }
            else if((footPos2.position - CoM).magnitude > (footPos1.position - CoM).magnitude && balancingLeft == false)
            {
                balancingRight = true;
                //rebalance right foot
                RebalanceFoot(footPos2, endEffector_r, rightEdges, forcePos2, rightFootTargetDist);
                balancingRight = false;
            }
            
        }*/
    }

    Vector3 CalculateCenterOfMass()
    {
        CoM = Vector3.zero;
        float c = 0f;

        foreach (Rigidbody body in rigidbodyList)
        {
            CoM += body.worldCenterOfMass * body.mass;
            c += body.mass;
        }

        CoM /= c;
        Debug.DrawRay(CoM, Vector3.down, Color.cyan);
        return CoM;
    }

    void AddGravityCompensation()
    {
        Vector3 forcePos = CalculateCenterOfMass();
        
        tb.AddForceAtPosition(Vector3.up * compensationMultiplier, pos.position);
        Debug.DrawRay(forcePos, Vector3.up * compensationMultiplier, Color.cyan);

    }

    /*bool CheckIfCoMIsBalanced()
    {
        RaycastHit hit;
        bool balanced;
        if(Physics.Raycast(CoM, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("SupportPolygon")))
        {
            balanced = true;
        }
        else
        {
            balanced = false;
        }
        
        return balanced;
        
    }*/

    //Get the point which the CoM is the closest to 
    /*Vector3 GetDirectionToMove(Transform[] edges)
    {
        Vector3 dirToMove = new Vector3(0,0,0);
        //consider positive and negative results
        if((edges[0].position - CoM).magnitude < (edges[1].position - CoM).magnitude)
        {
            dirToMove = edges[1].position - edges[0].position;
        }
        else
        {
            dirToMove = edges[0].position - edges[1].position;
        }

        return dirToMove;
    }*/

    /*void Rebalance(Transform pos1, Transform pos2, Vector3 CoM)
    {
        GameObject g;
        Transform[] edges;
        Transform forcepos;
        float targetDist = 0.0f;

        //Check which foot is furthest from CoM
        if ((CoM - endEffector_r.transform.position).magnitude < (CoM - endEffector_l.transform.position).magnitude)
        {
            g = endEffector_l;
            edges = leftEdges;
            forcepos = forcePos1;
            targetDist = leftFootTargetDist;
            Debug.Log("left foot target dist: " + leftFootTargetDist);
        }
        else
        {
            g = endEffector_r;
            edges = rightEdges;
            forcepos = forcePos2;
            targetDist = rightFootTargetDist;
            Debug.Log("Right foot target dist: " + rightFootTargetDist);
        }

        Vector3 dirToMove = GetDirectionToMove(edges);
        Debug.DrawRay(g.transform.position, dirToMove.normalized * forceMultiplier, Color.blue);
        Rigidbody rb = g.GetComponent<Rigidbody>();
        if((forcepos.position - CoM).magnitude != targetDist)
        {
            Debug.Log("adding force");
            rb.AddForceAtPosition(dirToMove.normalized * forceMultiplier, forcepos.position, ForceMode.Impulse);
        }

        //supportPolyGen.GetComponent<SupportPolygonGenerator>().GenerateNewPolygon();
        //Generate new support polygon here?
        //public method in support polygon generator for generating polygons when the ragdoll is balanced
    }*/

    /*void RebalanceFoot(Transform foot, GameObject g, Transform[] edges, Transform forcepos, float targetDistance)
    {
        Vector3 dirToMove = GetDirectionToMove(edges);
        Debug.DrawRay(g.transform.position, dirToMove.normalized * forceMultiplier, Color.blue);
        Rigidbody rb = g.GetComponent<Rigidbody>();
        if ((forcepos.position - CoM).magnitude != targetDistance)
        {
            Debug.Log("adding force");
            rb.AddForceAtPosition(dirToMove.normalized * forceMultiplier, forcepos.position, ForceMode.Impulse);
        }
        else
        {
            Debug.Log("EIIII");
            if(foot == footPos1)
            {
                balancingLeft = false;
            }
            else
            {
                balancingRight = false;
            }
        }
        
        
    }*/
}

