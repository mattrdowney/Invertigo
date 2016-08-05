using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShapeToSVG : MonoBehaviour
{
    static List<QuadraticBezier> lines;

    static float delta = 4 * Mathf.Pow(2, -23); // 4 because it's the largest power of 2 below 2*pi; 2**-23 because it is the smallest mantissa possible in IEEE 754 (for float)
    static float threshold = 1e-5f; 

    public static void AddLine(ArcOfSphere edge, float begin, float end)
    {
        Vector2 begin_UV = SpaceConverter.SphereToUV(edge.Evaluate(begin));
        Vector2 end_UV = SpaceConverter.SphereToUV(edge.Evaluate(end));

        if (Vector2.Distance(begin_UV, end_UV) > threshold)
        {
            Vector2 delta_begin_UV = SpaceConverter.SphereToUV(edge.Evaluate(begin + delta));
            Vector2 delta_end_UV = SpaceConverter.SphereToUV(edge.Evaluate(end - delta));
            Vector2 control_point = Intersection(begin_UV, delta_begin_UV, delta_end_UV, end_UV);

            lines.Add(new QuadraticBezier(begin_UV, control_point, end_UV));
        }
    }

    public static void Append(GameObject shape)
    {
        ArcOfSphere first_edge = shape.GetComponentInChildren<Edge>();
        SVGBuilder.BeginShape();
        SVGBuilder.SetPoint(SpaceConverter.SphereToUV(first_edge.Evaluate(first_edge.Begin())));
        ArcOfSphere arc = first_edge;

        do
        {
            float begin = arc.Begin();
            float end = arc.End();
            float range_begin = arc.Begin();
            float range_end = arc.End();
            float range_mid = arc.End() / 2;

            int[,] xyz_signs_begin = new int[2, 3]; //positional, derivative; x, y, z
            int[,] xyz_signs_end   = new int[2, 3];
            int[,] xyz_signs_range_begin = new int[2, 3]; //immediately before the first detected change in sign
            int[,] xyz_signs_range_end = new int[2, 3]; //the first detected change in any sign
            int[,] xyz_signs_range_mid = new int[2, 3]; //middle of begin and range_end

            // 1) get x, y, and z sign at beginning and end of arc
            // 2) get beginning of arc's slope's sign for xyz(0, delta) and end of arc's slope's sign for xyz(end - delta, end)
            UpdateSigns(arc, ref xyz_signs_begin, arc.Begin(), delta); // ~2**-21 e.g. 4*(2**-23); 4 because it's the largest power of 2 below 2*pi; 2**-23 because it is the smallest mantissa possible in IEEE 754 (for float)
            UpdateSigns(arc, ref xyz_signs_end, arc.End(), -delta);

            // 7) continue from #3 until you begin = end for positional and slope signs
            while (!SameSigns(ref xyz_signs_begin, ref xyz_signs_end))
            {
                xyz_signs_range_begin = xyz_signs_begin;
                xyz_signs_range_end = xyz_signs_end;

                // 3) binary search and discard ranges with matching slope signs and position signs at ends; and then update the slope signs.
                while (range_begin < range_end - delta)
                {
                    range_mid = (range_begin + range_end) / 2; //guaranteed not to overflow since numbers are in range [0, 2pi]
                    UpdateSigns(arc, ref xyz_signs_range_mid, range_mid, delta);
                    if (SameSigns(ref xyz_signs_range_begin, ref xyz_signs_range_mid))
                    {
                        range_begin = range_mid;
                        //xyz_signs_begin = xyz_signs_range_mid; //not necessary, the signs are the same
                    }
                    else
                    {
                        range_end = range_mid;
                        xyz_signs_range_end = xyz_signs_range_mid;
                    }
                }
                // 4) when you find a sign that switches, log the exact position of the switch with as much precision as possible
                Subdivide(arc, begin, range_begin);
                // 6) when you find that position, you must then switch the x, y, z signs at the new beginning of the arc and the slope signs xyz at the beginning of the arc
                begin = range_end;
                xyz_signs_begin = xyz_signs_range_end;

            }
            // 4) when you find a sign that switches, log the exact position of the switch with as much precision as possible
            Subdivide(arc, begin, end);

            for (float iteration = 1; iteration <= 1000; ++iteration) //TODO: use exactly the number iterations neccessary to minimize error below threshold in svg
            {
                SVGBuilder.SetPoint(SpaceConverter.SphereToUV(arc.Evaluate(arc.End() * (iteration / 1000)))); //Assert: begin = 0 for radius of 0
            }
            arc = arc.next.next; //skip corners
        } while (arc != first_edge);
        SVGBuilder.EndShape();
    }

    private static Vector2 Intersection(Vector2 begin, Vector2 after_begin, Vector2 before_end, Vector2 end)
    {
        float numerator_x, numerator_y, denominator;

        numerator_x = ((begin.x * after_begin.y - begin.y * after_begin.x) * (before_end.x - end.x) -
                       (before_end.x * end.y - before_end.y * end.x) * (begin.x - after_begin.x);


        numerator_y = (begin.x * after_begin.y - begin.y * after_begin.x) * (before_end.y - end.y) -
                      (before_end.x * end.y - before_end.y * end.x) * (begin.y - after_begin.y);

        denominator = (begin.x - after_begin.x) * (before_end.y - end.y) -
                      (begin.y - after_begin.y) * (before_end.x - end.x);

        return new Vector2(numerator_x / denominator, numerator_y / denominator);
    }

    private static float MaxError(ArcOfSphere arc, float begin, float end)
    {
        float midpoint = (begin + end) / 2;

        Vector2 L1 = SpaceConverter.SphereToUV(arc.Evaluate(begin));
        Vector2 L2 = SpaceConverter.SphereToUV(arc.Evaluate(end));

        float error1 = MaxError(arc, L1, L2, begin, midpoint);
        float error2 = MaxError(arc, L1, L2, midpoint, end);

        Vector2 P1 = SpaceConverter.SphereToUV(arc.Evaluate(error1));
        Vector2 P2 = SpaceConverter.SphereToUV(arc.Evaluate(error2));

        if (Point_Line_Distance(L1, L2, P1) > Point_Line_Distance(L1, L2, P2))
        {
            return error1;
        }
        return error2;
    }

    private static float MaxError(ArcOfSphere arc, Vector2 L1, Vector2 L2, float begin, float end)
    {
        float error_begin = Point_Line_Distance(L1, L2, SpaceConverter.SphereToUV(arc.Evaluate(begin)));
        float error_end = Point_Line_Distance(L1, L2, SpaceConverter.SphereToUV(arc.Evaluate(end)));

        // 2) use binary / bisection search to find the point of maximal error
        while (begin < end - delta)
        {
            float midpoint = (begin + end) / 2;
            float error_mid = Point_Line_Distance(L1, L2, SpaceConverter.SphereToUV(arc.Evaluate(midpoint)));
            if(error_begin < error_end) //error begin should be replaced since it has minimum error
            {
                begin = midpoint;
                error_begin = error_mid;
            }
            else //error end should be replaced since it has minimum error
            {
                end = midpoint;
                error_end = error_mid;
            }
        }

        return (begin + end) / 2; // return LOCATION of max error
    }

    private static float Point_Line_Distance(Vector2 L1, Vector2 L2, Vector2 P)
    {
        float numerator = Mathf.Abs((L2.y - L1.y) * P.x - (L2.x - L1.x) * P.y + L2.x * L1.y - L2.y * L1.x);
        float denominator = Mathf.Sqrt(Mathf.Pow((L2.y - L1.y), 2) + Mathf.Pow((L2.x - L1.x), 2));

        return numerator / denominator;
    }

    private static bool SameSigns(ref int[,] data_A, ref int[,] data_B) // for the purpose of this function, zero is not considered a sign
    {
        for(int derivative = 0; derivative < 2; ++derivative)
        {
            for(int dimension = 0; dimension < 3; ++dimension)
            {
                if (data_A[derivative, dimension] != data_B[derivative, dimension])
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static void Subdivide(ArcOfSphere arc, float begin, float end)
    {
        float midpoint = MaxError(arc, begin, end);

        Vector2 L1 = SpaceConverter.SphereToUV(arc.Evaluate(begin));
        Vector2 L2 = SpaceConverter.SphereToUV(arc.Evaluate(end));
        Vector2 P = SpaceConverter.SphereToUV(arc.Evaluate(midpoint));

        // if the max error is greater than a threshold, recursively add the left and right halves into the list of lines
        if (Point_Line_Distance(L1, L2, P) > threshold)
        {
            Subdivide(arc, begin, midpoint);
            Subdivide(arc, midpoint, end);
        }
        else
        {
            AddLine(arc, begin, end);
        }
    }

    private static void UpdateSigns(ArcOfSphere arc, ref int[,] data, float location, float delta)
    {
        //assert data's size is [2, 3]

        for (int dimension = 0; dimension < 3; ++dimension)
        {
            data[0, dimension] = System.Math.Sign(arc.Evaluate(location)[dimension]);
        }

        for (int dimension = 0; dimension < 3; ++dimension)
        {
            data[1, dimension] = System.Math.Sign(arc.Evaluate(location + delta)[dimension] - arc.Evaluate(location)[dimension]) * System.Math.Sign(delta);
        }
    }
}