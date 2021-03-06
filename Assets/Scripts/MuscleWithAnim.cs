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

    bool isActive = false;

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
        Debug.Log("ACTIVATION VALUE: " + activationValue);
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
        isActive = true;
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
    }

    private void DeactivateMuscle()
    {
        isActive = false;
        //When deactivating, use lowest possible muscle force of 0.01f and do not multiply with strength
        muscle = new Muscle2(0.01f);
        double force = muscle.step(0.01f, Time.fixedDeltaTime);

        Vector3 forceOnRb2 = (a1.transform.position - a2.transform.position) * (float)force;
        Vector3 forceOnRb1 = (a2.transform.position - a1.transform.position) * (float)force;
        rb2.AddForceAtPosition(forceOnRb2, a2.transform.position);
        rb1.AddForceAtPosition(forceOnRb1, a1.transform.position);
    }

    private void LateUpdate()
    {
        DrawMuscleSegment(a1, a2);
    }

    //Drawing the muscle in the scene for visualization
    private void DrawMuscleSegment(Transform point1, Transform point2)
    {
        //Draw the muscle
        Debug.DrawLine(point1.transform.position, point2.transform.position, isActive ? Color.red : Color.green);

        //Draw the target length
        Vector3 direction = a2.position - a1.position;
        Vector3 offset = Vector3.right * 0.01f;
        Debug.DrawLine(a1.position + offset, (a1.position + direction.normalized * targetLength) + offset,Color.cyan);

    }
}
