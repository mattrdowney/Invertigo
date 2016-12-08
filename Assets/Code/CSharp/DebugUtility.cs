#define DEBUG

using UnityEngine;
using System;
using System.Diagnostics;
using System.Text;

public class DebugUtility
{
    [Conditional("DEBUG")]
    public static void Assert(bool condition, params object[] parameters)
    {
        if (!condition)
        {
            Log("<color=red>", parameters, "</color>");
            throw new Exception();
        }
    }

    [Conditional("DEBUG")]
    public static void Log(params object[] parameters) // http://www.alanzucconi.com/2015/08/26/console-debugging-in-unity-made-easy/
    {
        StringBuilder builder = new StringBuilder();
        for (int index = 0; index < parameters.Length; ++index)
        {
            builder.Append(parameters[index].ToString());
            builder.Append(" ");
        }
        UnityEngine.Debug.Log(builder.ToString());
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
            Log("Vectors not orthogonal!");
            Log(v1, ".", v2, "=", Vector3.Dot(v1, v2));
            Log(v1, ".", v3, "=", Vector3.Dot(v1, v3));
            Log(v2, ".", v3, "=", Vector3.Dot(v2, v3));
            throw new Exception();
        }
    }
}