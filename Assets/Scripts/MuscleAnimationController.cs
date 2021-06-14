using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MuscleAnimationController : MonoBehaviour
{
    public MuscleAnimation animationObject;
    //public GameObject supportPolyGen;

    public Transform[] rightEdges;
    public Transform[] leftEdges;

    public Transform ragdoll;

    public float compensationMultiplier;
    public float footSupportForce;
    public float hipsCompensationForce;

    public Transform pos;
    public Transform[] footSupportPositions;
    public Rigidbody leftFoot;
    public Rigidbody rightFoot;
    public Rigidbody hips;
    public Rigidbody addForce;

    //Start of the animation timeline
    public float animationHead = 0.0f;

    //Initial center of mass
    Vector3 CoM = Vector3.zero;

    List<MuscleWithAnim> ragdollMuscles = new List<MuscleWithAnim>();
    List<Rigidbody> rigidbodyList;

    Vector3 CoMStartPos;
    

    void Start()
    {
        //Keep track of all muscles and rigidbodies in ragdoll
        rigidbodyList = ragdoll.GetComponentsInChildren<Rigidbody>().ToList();
        ragdollMuscles = ragdoll.GetComponentsInChildren<MuscleWithAnim>().ToList();

        //Don't let Unity calculate these, prevents weird behavior
        foreach(Rigidbody rb in rigidbodyList)
        {
            rb.centerOfMass = new Vector3(0, 0, 0);
            rb.inertiaTensor = new Vector3(1, 1, 1);
        }
        
        //CalculateCenterOfMass();

        //supportPolyGenObj = supportPolyGen.GetComponent<SupportPolygonGenerator>();
        //supportPolyGenObj.GenerateNewPolygon();

        //CoMStartPos = GetHitPos();
        Debug.Log("COM start pos: " + CoMStartPos);
    }

    private void Update()
    {
        //CalculateCenterOfMass();
        //AddGravityCompensation();
        //supportPolyGenObj.GenerateNewPolygon();

        //RebalanceCoM();
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

    public Vector3 CalculateCenterOfMass()
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
        
        addForce.AddForceAtPosition(Vector3.up * compensationMultiplier, pos.position);
        Debug.DrawRay(forcePos, Vector3.up * compensationMultiplier, Color.cyan);

        //add force for keeping feet grounded
        leftFoot.AddForceAtPosition(Vector3.down * footSupportForce, footSupportPositions[0].position);
        rightFoot.AddForceAtPosition(Vector3.down * footSupportForce, footSupportPositions[1].position);
    }

    void RebalanceCoM()
    {
        RaycastHit hit;
        
        if(Physics.Raycast(CoM, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("SupportPolygon")))
        {
            Vector3 hitPointInCol = hit.point;
            Debug.Log("hit in collider: " + hit.point);

            //Get the direction CoM is moving in from start pos
            Vector3 CoMMovementDir = hitPointInCol - CoMStartPos;
            Debug.Log("COM movement: " + CoMMovementDir);

            float CoMMovementDirMag = Mathf.Abs(CoMMovementDir.magnitude);
            if(CoMMovementDirMag > 0.001f)
            {
                Vector3 compensationMovement = -(CoMMovementDir);
                Vector3 compForce = new Vector3(compensationMovement.x, 0.0f, compensationMovement.z);
                hips.AddForce(compForce * hipsCompensationForce, ForceMode.Impulse);
                Debug.DrawRay(CoM, CoMMovementDir * 10f, Color.magenta);
                Debug.DrawRay(CoM, compForce * 10f, Color.blue);
            }
        } 
    }

    private Vector3 GetHitPos()
    {
        RaycastHit hit;

        if (Physics.Raycast(CoM, Vector3.down, out hit, Mathf.Infinity))
        {
            if (hit.collider != null)
            {
                return hit.point;
            }
        }
        return Vector3.zero;
    }


}

