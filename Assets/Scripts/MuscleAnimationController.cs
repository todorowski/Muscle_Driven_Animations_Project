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
    public GameObject supportPolyGen;

    public Transform[] rightEdges;
    public Transform[] leftEdges;

    public Transform ragdoll;
    //public Transform forcePos1;
    //public Transform forcePos2;
    //public Transform footPos1;
    //public Transform footPos2;
    //public float forceMultiplier;
    public float compensationMultiplier;
    public float footSupportForce;

    //public Transform pos;
    //public Transform[] footSupportPositions;
    //public Rigidbody leftFoot;
    //public Rigidbody rightFoot;

    //Start of the animation timeline
    public float animationHead = 0.0f;

    //Initial center of mass
    Vector3 CoM = Vector3.zero;

    float leftFootTargetDist = 0.0f;
    float rightFootTargetDist = 0.0f;

    List<MuscleWithAnim> ragdollMuscles = new List<MuscleWithAnim>();
    List<Rigidbody> rigidbodyList;
    bool r_grounded, l_grounded;

    //public Rigidbody addForce;

    bool balancingLeft;
    bool balancingRight;

    //test
    public GameObject test;

    //SupportPolygonGenerator supportPolyGenObj;

    Vector3 CoMStartPos;

    //public Rigidbody hips;
    
    void Start()
    {
        rigidbodyList = ragdoll.GetComponentsInChildren<Rigidbody>().ToList();
        ragdollMuscles = ragdoll.GetComponentsInChildren<MuscleWithAnim>().ToList();
        //CalculateCenterOfMass();

        //supportPolyGenObj = supportPolyGen.GetComponent<SupportPolygonGenerator>();
        //supportPolyGenObj.GenerateNewPolygon();

        //CoMStartPos = GetHitPos();
        //GameObject testObj = Instantiate(test, CoMStartPos, transform.rotation);
        //Debug.Log(CoMStartPos);
    }

    private void Update()
    {
        //CalculateCenterOfMass();
        //AddGravityCompensation();
        //supportPolyGenObj.GenerateNewPolygon();

        //CheckIfCoMIsBalanced();

    }

    void FixedUpdate()
    {
        //Set target length according to anim curve and activate muscles
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (MuscleAnimationStruct mStruct in animationObject.muscles)
            {
                foreach (MuscleWithAnim m in ragdollMuscles)
                {
                    if (m.name == mStruct.muscleName)
                    {
                        m.targetLength = mStruct.animCurve.Evaluate(animationHead);
                        m.Activate();
                    }
                }
            }
            
        }
        animationHead += Time.fixedDeltaTime;
    }

    //---------------------BALANCE---------------------//

    /*public Vector3 CalculateCenterOfMass()
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
    }*/

    /*void AddGravityCompensation()
    {
        Vector3 forcePos = CalculateCenterOfMass();
        
        addForce.AddForceAtPosition(Vector3.up * compensationMultiplier, pos.position);
        Debug.DrawRay(forcePos, Vector3.up * compensationMultiplier, Color.cyan);

        //add force for keeping feet grounded
        leftFoot.AddForceAtPosition(Vector3.down * footSupportForce, footSupportPositions[0].position);
        rightFoot.AddForceAtPosition(Vector3.down * footSupportForce, footSupportPositions[1].position);
    }*/

    /*bool CheckIfCoMIsBalanced()
    {
        RaycastHit hit;
        bool balanced;
        
        if(Physics.Raycast(CoM, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("SupportPolygon")))
        {
            //called in update so this value will change, save value from start
            Vector3 hitPointInCol = hit.point;
            Debug.Log("hit in collider: " + hit.point);

            //Get the direction CoM is moving in from start pos
            Vector3 CoMMovementDir = hitPointInCol - CoMStartPos;
            Debug.Log("COM movement: " + CoMMovementDir);

            //how to make this value signed?
            float CoMMovementDirMag = Mathf.Abs(CoMMovementDir.magnitude);
            if(CoMMovementDirMag > 0.01f)
            {
                Vector3 compensationMovement = -(CoMMovementDir);
                Vector3 compForce = new Vector3(compensationMovement.x, 0.0f, compensationMovement.z);
                //hips.AddForce(compForce * 40f, ForceMode.Impulse);
                Debug.DrawRay(CoM, CoMMovementDir * 10f, Color.magenta);
                Debug.DrawRay(CoM, compForce * 10f, Color.blue);
            }
            balanced = true;
        }
        else
        {
            balanced = false;
        }
        return balanced;   
    }*/

    /*Vector3 GetHitPos()
    {
        RaycastHit hit;
        Vector3 position;
        if (Physics.Raycast(CoM, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("SupportPolygon")))
        {
            position = hit.point;
            Debug.Log("THE HIT: " + hit.point);
            return position;
        }
        else
        {
            position = Vector3.zero;
        }

        return position;
    }*/
}

