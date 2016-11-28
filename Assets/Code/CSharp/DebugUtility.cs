#define DEBUG

using UnityEngine;
using System;
using System.Diagnostics;

public class DebugUtility
{
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            UnityEngine.Debug.Log(message);
            throw new Exception();
        }
    }

    [Conditional("DEBUG")]
    public static void Print(string message)
    {
        UnityEngine.Debug.Log(message);
    }

    [Conditional("DEBUG")]
    public static void Print(string message, int frames)
    {
        if (UnityEngine.Random.Range(0, frames) == 0) UnityEngine.Debug.Log(message);
    }

    [Conditional("DEBUG")]
    public static void OrthoNormalAssert(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        if (!Mathf.Approximately(v1.magnitude, 1) ||
                !Mathf.Approximately(v2.magnitude, 1) ||
                !Mathf.Approximately(v3.magnitude, 1))
        {
            UnityEngine.Debug.Log("Vectors not normalized!");
            UnityEngine.Debug.Log(v1 + " " + v2 + " " + v3);
            throw new Exception();
        }
        if (Mathf.Abs(Vector3.Dot(v1, v2)) > 1e-7 ||
                   Mathf.Abs(Vector3.Dot(v1, v3)) > 1e-7 ||
                   Mathf.Abs(Vector3.Dot(v2, v3)) > 1e-7)
        {
            UnityEngine.Debug.Log("Vectors not orthogonal!");
            UnityEngine.Debug.Log(v1 + "." + v2 + "=" + Vector3.Dot(v1, v2));
            UnityEngine.Debug.Log(v1 + "." + v3 + "=" + Vector3.Dot(v1, v3));
            UnityEngine.Debug.Log(v2 + "." + v3 + "=" + Vector3.Dot(v2, v3));
            throw new Exception();
        }
    }

    public static string Vector2ToString(Vector2 print_me)
    {
        return "(" + print_me.x + ", " + print_me.y + ")";
    }

    public static string Vector3ToString(Vector3 print_me)
    {
        return "(" + print_me.x + ", " + print_me.y + "," + print_me.z + ")";
    }
}