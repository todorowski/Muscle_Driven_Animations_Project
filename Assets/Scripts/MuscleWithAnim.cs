using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuscleWithAnim : MonoBehaviour
{
    //Affected bodies
    public Rigidbody rb1;
    public Rigidbody rb2;

    //Muscle attachment points
    public Transform a1;
    public Transform a2;

    public float strength;
    public string name;

    //PD controller gains
    public float Kp;
    public float Kd;
    public float Ki;

    float activationValue;
    Muscle2 muscle;
    MuscleBasedPD pd;

    [System.NonSerialized]
    public float targetLength;

    void Start()
    {
        pd = new MuscleBasedPD(Kp, Kd, Ki);
    }

    void FixedUpdate()
    {
        activationValue = pd.UpdateMuscleBasedPD(targetLength, (a2.transform.position - a1.transform.position).magnitude, Time.fixedDeltaTime);
    }

    public void Activate()
    {
        if (activationValue > 0)
        {
            DeactivateMuscle();
        }
        else
        {
            activationValue = Mathf.Abs(activationValue);
            ActivateMuscle();
        }
    }

    private void ActivateMuscle()
    {
        //Muscle force cannot be lower than 0.01f
        if (activationValue < 0.01f)
        {
            activationValue = 0.01f;
        }
        muscle = new Muscle2(activationValue);
        double force = muscle.step(activationValue, Time.fixedDeltaTime);

        Vector3 forceOnRb2 = (a1.transform.position - a2.transform.position) * (float)force * strength;
        Vector3 forceOnRb1 = (a2.transform.position - a1.transform.position) * (float)force * strength;
        rb2.AddForceAtPosition(forceOnRb2, a2.transform.position);
        rb1.AddForceAtPosition(forceOnRb1, a1.transform.position);

        Debug.Log("activate force at rb1: " + forceOnRb1);
        Debug.Log("activate force at rb2: " + forceOnRb2);
    }

    private void DeactivateMuscle()
    {
        //When deactivating, use lowest possible muscle force of 0.01f and do not multiply with strength
        muscle = new Muscle2(0.01f);
        double force = muscle.step(0.01f, Time.fixedDeltaTime);

        Vector3 forceOnRb2 = (a1.transform.position - a2.transform.position) * (float)force;
        Vector3 forceOnRb1 = (a2.transform.position - a1.transform.position) * (float)force;
        rb2.AddForceAtPosition(forceOnRb2, a2.transform.position);
        rb1.AddForceAtPosition(forceOnRb1, a1.transform.position);


        Debug.Log("deactive force at rb1: " + forceOnRb1);
        Debug.Log("deactivate force at rb2: " + forceOnRb2);
    }

    private void LateUpdate()
    {
        DrawMuscleSegment(a1, a2);
    }

    //Drawing the muscle in the scene for visualization
    private void DrawMuscleSegment(Transform point1, Transform point2)
    {
        Debug.DrawLine(point1.transform.position, point2.transform.position, Color.red);
    }
}
