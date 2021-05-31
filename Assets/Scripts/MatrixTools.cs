using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixTools : MonoBehaviour
{
    public static float[,] TransposeMatrix(float [,] matrix)
    {
        int width = matrix.GetLength(0);
        int height = matrix.GetLength(1);
        float[,] result = new float[height, width];

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                result[j, i] = matrix[i, j];
            }
        }
        return result;
    }

    public static float[,] PopulateMatrix(float[,] matrix, Vector3[] pos)
    {
        //only supports 3x3 matrix?
        for (int i = 0; i < pos.Length; i++)
        {
            matrix[0, i] = pos[i].x;
            matrix[1, i] = pos[i].y;
            matrix[2, i] = pos[i].z;

            
            /*matrix[3, i] = pos[i].x;
            matrix[4, i] = pos[i].y;
            matrix[5, i] = pos[i].z;

            /*matrix[0, i] = pos[i].x;
            matrix[1, i] = pos[i].y;
            matrix[2, i] = pos[i].z;*/

        }

        return matrix;
    }

    public static float[,] MultiplyMatrix(float[,] A, float[,] B)
    {
        int rA = A.GetLength(0);
        int cA = A.GetLength(1);
        int rB = B.GetLength(0);
        int cB = B.GetLength(1);
        float[,] C = new float[cA, cB];

        Debug.Log("rA: " + A.GetLength(0));
        Debug.Log("cA: " + A.GetLength(1));
        Debug.Log("rB: " + B.GetLength(0));
        Debug.Log("cB: " + B.GetLength(1));

        Debug.Log("C ARRAY: " + C.GetLength(0) + " " + C.GetLength(1));
        
        if (cA == rB)
        {
            Debug.Log("EIIIIIIIIIIIIIII");
            for (int i = 0; i < rA; i++)
            {
                for (int j = 0; j < cB; j++)
                {
                    Debug.Log("HERE: " + i);
                    Debug.Log("HERE2: " + j);
                    Debug.Log("J: " + j);
                    Debug.Log("cB: " + cB);
                    C[i, j] = 0;
                    for (int k = 0; k < cA; k++) // OR k<b.GetLength(0)
                        C[i, j] += A[i, k] * B[k, j];
                }
            }
        }

        return C;
    }
}
