using UnityEngine;

public class QuadraticBezier
{
    public QuadraticBezier(Vector2 b, Vector2 c, Vector2 e)
    {
        begin = b;
        control_point = c;
        end = e;
    }
    public Vector2 begin;
    public Vector2 control_point;
    public Vector2 end;
}
