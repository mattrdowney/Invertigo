using UnityEngine;

public class QuadraticBezier
{
    public QuadraticBezier(ArcOfSphere a, Vector2 b, Vector2 c, Vector2 e, float bt, float et)
    {
        begin_UV = b;
        control_point = c;
        end_UV = e;

        arc = a;
        begin = bt;
        end = et;
    }

    public override string ToString()
    {
        return begin_UV.ToString("F6") + "->" + control_point.ToString("F6") + "->" + end_UV.ToString("F6");
    }

    //crucial data
    public Vector2 begin_UV;
    public Vector2 control_point;
    public Vector2 end_UV;

    //supplementary data for "cutting" shapes at edges (when you need to know the "insideness")
    public ArcOfSphere arc;
    public float begin;
    public float end;
}
