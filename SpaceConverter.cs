using UnityEngine;
using System.Collections;

public class SpaceConverter
{
    private static Vector3 SphereToOctahedron(Vector3 spherical)
    {
        float sum = Mathf.Abs(spherical.x) + Mathf.Abs(spherical.y) + Mathf.Abs(spherical.z);
        return spherical / sum;
    }

    private static Vector2 OctahedronToUV(Vector3 octahedral) // since this is essentially editor code, there might not be any need to optimize this
    {
        Vector2 UV;
        Vector2 xPivot, yPivot, zPivot;

        if (System.Math.Sign(octahedral.x) == +1) //FIXME: optimize this hardcoded stuff, Barycentric coordinate conversions are certainly capable of being elegant
        {
            if(System.Math.Sign(octahedral.z) == +1)
            {
                yPivot = new Vector2(1.0f, 1.0f);
                zPivot = new Vector2(0.5f, 1.0f);
            }
            else
            {
                yPivot = new Vector2(1.0f, 0.0f);
                zPivot = new Vector2(0.5f, 0.0f);
            }
            xPivot = new Vector2(1.0f, 0.5f);
        }
        else
        {
            if (System.Math.Sign(octahedral.z) == +1)
            {
                yPivot = new Vector2(0.0f, 1.0f);
                zPivot = new Vector2(0.5f, 1.0f);
            }
            else
            {
                yPivot = new Vector2(0.0f, 0.0f);
                zPivot = new Vector2(0.5f, 0.0f);
            }
            xPivot = new Vector2(0.0f, 0.5f);
        }
        if(System.Math.Sign(octahedral.y) == +1)
        {
            yPivot = new Vector2(0.5f, 0.5f);
        }
        UV = xPivot*Mathf.Abs(octahedral.x) + yPivot* Mathf.Abs(octahedral.y) + zPivot* Mathf.Abs(octahedral.z);
        return UV;
    }

    public static Vector2 SphereToUV(Vector3 spherical)
    {
        return OctahedronToUV(SphereToOctahedron(spherical));
    }
}
