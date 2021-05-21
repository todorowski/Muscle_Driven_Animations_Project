using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IK_Test_Muscle : MonoBehaviour
{
    public GameObject[] joints;
    private GameObject[] jointRots;
    public GameObject target;
    //Vector3 target;
    public int countMax = 1000;

    public float EPS = 0.01f;
    public float step = 0.015f;

    private float[] angles;
    private int count = 0;
    private bool startJT_Method_Flag = false;

    List<Rigidbody> rigidbodyList;
    public Transform ragdoll;

    //List of muscles
    List<MuscleWithAnim> ragdollMuscles = new List<MuscleWithAnim>();

    float currentLength1 = 0.0f;
    float currentLength2 = 0.0f;
    float currentLength3 = 0.0f;
    float currentLength4 = 0.0f;
    float currentLength5 = 0.0f;
    float currentLength6 = 0.0f;

    //Support polygon things
    SupportPolygonGenerator supportPolyGenObj;
    public GameObject supportPolyGen;
    Collider supportPolyCol;
    public Transform[] rightEdges;
    public Transform[] leftEdges;

    public MuscleWithAnim[] leftMuscles;
    public MuscleWithAnim[] rightMuscles;
    float[] leftCurrentLengths;
    float[] rightCurrentLengths;

    private Vector3 CoMHit = Vector3.zero;
    private Vector3 CoM = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        ragdollMuscles = ragdoll.GetComponentsInChildren<MuscleWithAnim>().ToList();
        rigidbodyList = ragdoll.GetComponentsInChildren<Rigidbody>().ToList();

        foreach (Rigidbody rb in rigidbodyList)
        {
            rb.centerOfMass = new Vector3(0, 0, 0);
            rb.inertiaTensor = new Vector3(1, 1, 1);
            if (rb.GetComponent<Collider>() != null)
            {
                //ignore collision on player layer
                Physics.IgnoreLayerCollision(6, 6);
            }
        }

        //Get CoM
        CoM = CalculateCenterOfMass();
        Debug.Log("COM POS: " + CoM.x + CoM.y + CoM.z);

        supportPolyGenObj = supportPolyGen.GetComponent<SupportPolygonGenerator>();
        supportPolyGenObj.GenerateNewPolygon();
        supportPolyCol = supportPolyGenObj.GetComponent<Collider>();

        jointRots = new GameObject[joints.Length - 1];

        for (int i = 0; i < jointRots.Length; i++)
        {
            GameObject tmp = new GameObject(joints[i + 1].name + "_Rot");
            tmp.transform.position = joints[i + 1].transform.position;
            tmp.transform.parent = joints[i].transform;
            jointRots[i] = tmp;
        }

        //fill current lengths array with all of the current lengths
        leftCurrentLengths = new float[leftMuscles.Length];
        rightCurrentLengths = new float[rightMuscles.Length];

        for(int i = 0; i < leftCurrentLengths.Length; i++)
        {
            leftCurrentLengths[i] = (leftMuscles[i].a1.position - leftMuscles[i].a2.position).magnitude;
        }

        for (int i = 0; i < rightCurrentLengths.Length; i++)
        {
            rightCurrentLengths[i] = (rightMuscles[i].a1.position - rightMuscles[i].a2.position).magnitude;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startJT_Method_Flag)
        {
            iterate_IK();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            start_IK();
        }

        //CoM - Support poly things
        CoM = CalculateCenterOfMass();
        CoMHit = GetCoMHit();
        //target = GetCenterOfColldier(supportPolyCol);
    }

    private void start_IK()
    {
        count = 0;
        startJT_Method_Flag = true;

        float angleA = calculateAngle(Vector3.up, joints[1].transform.position, joints[0].transform.position);
        float angleB = calculateAngle(Vector3.up, joints[2].transform.position, joints[1].transform.position);
        float angleC = calculateAngle(Vector3.up, joints[3].transform.position, joints[2].transform.position);
        angles = new float[] { angleA, angleB, angleC };
    }

    private void iterate_IK()
    {
        if (Mathf.Abs(Vector3.Distance(joints[joints.Length - 1].transform.position, target.transform.position)) > EPS)
        {
            JacobianIK();
        }

        /*if (Mathf.Abs(Vector3.Distance(new Vector3(GetCoMHit().x, 0.0f, GetCoMHit().z), new Vector3(target.x, 0.0f, target.z))) > EPS)
        {
            JacobianIK();
        }*/
        else
        {
            Debug.Log("Cycle Count: " + count.ToString());
            startJT_Method_Flag = false;
        }
        if (count >= countMax)
        {
            Debug.Log("Hit Cycle Count: " + count.ToString());
            startJT_Method_Flag = false;
        }
    }

    private void JacobianIK()
    {
        float[] dO = new float[angles.Length];
        float[] angleDiff = new float[angles.Length];

        dO = GetDeltaOrientation();
        for (int i = 0; i < dO.Length; i++)
        {
            Debug.Log("dO: " + i + ":" + dO[i]);
            angles[i] += dO[i] * step;
            angleDiff[i] = dO[i] * step;
            Debug.Log("anglediff: " + i + ":" + angleDiff[i]);
        }

        Debug.Log("ANGLEDIFF: " + angleDiff[0]);
        // update angles
        rotateLinks2(angleDiff);
        //rotateLinks(angleDiff);

        count++;
    }

    private float[] GetDeltaOrientation()
    {
        float[,] Jt = GetJacobianTranspose();

        Vector3 V = (target.transform.position - joints[joints.Length - 1].transform.position);
        //Vector3 V = new Vector3(GetCoMHit().x, 0.0f, GetCoMHit().z) - new Vector3(target.x, 0.0f, target.z);


        //dO = Jt * V;
        float[,] dO = MatrixTools.MultiplyMatrix(Jt, new float[,] { { V.x }, { V.y }, { V.z } });
        return new float[] { dO[0, 0], dO[1,0], dO[2,0] };
    }

    private float[,] GetJacobianTranspose()
    {

        Vector3 J_A = Vector3.Cross(joints[0].transform.forward, (joints[joints.Length - 1].transform.position - joints[0].transform.position));
        Vector3 J_B = Vector3.Cross(joints[1].transform.forward, (joints[joints.Length - 1].transform.position - joints[1].transform.position));
        Vector3 J_C = Vector3.Cross(joints[2].transform.forward, (joints[joints.Length - 1].transform.position - joints[2].transform.position));

        float[,] matrix = new float[3, 3];

        matrix = MatrixTools.PopulateMatrix(matrix, new Vector3[] { J_A, J_B, J_C });

        return MatrixTools.TransposeMatrix(matrix);
    }

    private float calculateAngle(Vector3 axis, Vector3 pos1, Vector3 pos2)
    {
        float value = 0f;

        value = Vector3.Angle(axis, (pos1 - pos2).normalized);
        Vector3 cross = Vector3.Cross(axis, (pos1 - pos2).normalized);
        if (cross.z < 0)
            value = -value;

        return value;
    }

    private void rotateLinks(float[] angleDiff)
    {
        float[] displayAngles = new float[angleDiff.Length];

        for (int i = 0; i < joints.Length - 1; i++)
        {
            Vector3 upDir = joints[i].transform.right;

            Vector3 crossAxis = Vector3.Cross(upDir, (joints[i + 1].transform.position - joints[i].transform.position).normalized);
            float currAngle = calculateAngle(Vector3.up, joints[i + 1].transform.position, joints[i].transform.position);
            float newAngle = angleDiff[i];
            displayAngles[i] = angleDiff[i] + currAngle;

            if (newAngle != 0)
                joints[i].transform.RotateAround(joints[i].transform.position, crossAxis, newAngle);
            //if newAngle >= 0 ActivateMuscles(leftMuscles[i])
            //else ActivateMuscles(rightMuscles[i])


            if (i < joints.Length - 2)
                updateLinkPos(i, joints[i].transform.position, crossAxis, angleDiff[i]);
            if (i >= joints.Length - 2) // end effector
                joints[i + 1].transform.position = jointRots[i].transform.position;

            //Debug.Log("joint " + (i + 1).ToString() + ": New angle Value: " + angleDiff[i].ToString());
        }
    }

    private void rotateLinks2(float[] angleDiff)
    {
        float[] displayAngles = new float[angleDiff.Length];

        for (int i = 0; i < joints.Length - 1; i++)
        {
            Vector3 upDir = joints[i].transform.right;

            Vector3 crossAxis = Vector3.Cross(upDir, (joints[i + 1].transform.position - joints[i].transform.position).normalized);
            float currAngle = calculateAngle(Vector3.up, joints[i + 1].transform.position, joints[i].transform.position);
            float newAngle = angleDiff[i];
            displayAngles[i] = angleDiff[i] + currAngle;

            if (newAngle >= 0)
            {
                ActivateMuscles(leftMuscles[i], newAngle);
            }
            if(newAngle < 0)
            {
                ActivateMuscles(rightMuscles[i], newAngle);
            }

            if (i < joints.Length - 2)
                updateLinkPos(i, joints[i].transform.position, crossAxis, angleDiff[i]);
            if (i >= joints.Length - 2) // end effector
                joints[i + 1].transform.position = jointRots[i].transform.position;

            //Debug.Log("joint " + (i + 1).ToString() + ": New angle Value: " + angleDiff[i].ToString());
        }
    }

    private void ActivateMuscles(MuscleWithAnim m, float angle)
    {
        float currentLength = (m.a1.position - m.a2.position).magnitude;
        m.targetLength = currentLength + angle;
        m.Activate();
        currentLength += angle;
        //current length
        //new length = current + angle (?)
        //m.Activate(new length)
        //current ´length = new length ?

        //activate biceps
        /*ragdollMuscles[0].targetLength = currentLength1 + angleDiff[0];
        ragdollMuscles[0].Activate();
        currentLength1 += angleDiff[0];*/
        
        //activate triceps
        /*ragdollMuscles[1].targetLength = currentLength2 + angleDiff[0];
        ragdollMuscles[1].Activate();
        currentLength2 += angleDiff[0];*/
       
    }

    private void updateLinkPos(int p, Vector3 rotPos, Vector3 cross, float angle)
    {
        if (p >= joints.Length - 2)
            return;

        for (int i = p; i < jointRots.Length; i++)
            joints[i + 1].transform.position = jointRots[i].transform.position;

        return;

    }

    private void display_JointAngles(float[] angles, float[] actualAngles)
    {

    }

    private void resetJoints()
    {
        for (int i = 0; i < joints.Length - 1; i++)
        {
            joints[i].transform.rotation = new Quaternion(0f, 0f, 0f, joints[i].transform.rotation.w);

            if (i > 0)
                joints[i].transform.position = jointRots[i - 1].transform.position;
            if (i >= joints.Length - 2) // end effector
                joints[i + 1].transform.position = jointRots[i].transform.position;
        }

    }

    //----------SUPPORT POLYGON THINGS--------------//

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
        Debug.DrawRay(CoM, Vector3.down * 2f, Color.cyan);
        return CoM;
    }

    private Vector3 GetCoMHit()
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

    private Vector3 GetCenterOfColldier(Collider c)
    {
        c = supportPolyGenObj.GetComponent<Collider>();
        Vector3 c_Center = c.bounds.center;

        return c_Center;
    }
}
