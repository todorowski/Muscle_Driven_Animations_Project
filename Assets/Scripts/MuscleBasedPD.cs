using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MuscleBasedPD
{
    //Controller gains
    public float Kp;
    public float Kd;
    public float Ki;

    float integral;
    float lastError;

    float lastAngularVelError;

    public MuscleBasedPD(float Kprop, float Kder, float Kin)
    {
        Kp = Kprop;
        Kd = Kder;
        Ki = Kin;
    }

    public float UpdateMuscleBasedPD(float setpoint, float measurement, float timeFrame)
    {
        //Error signal
        float error = setpoint - measurement;
        float derivative;
        integral += error * timeFrame;
        
        derivative = (error - lastError) / timeFrame;
        lastError = error;
        return error * Kp + integral * Ki + derivative * Kd;
        
    }
}
