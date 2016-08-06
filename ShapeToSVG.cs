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

            lines.Add(new QuadraticBezier(edge, begin_UV, control_point, end_UV, begin, end));
        }
    }

    public static void Append(GameObject shape)
    {
        ArcOfSphere first_edge = shape.GetComponentInChildren<Edge>();
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

            // get signs for beginning and end
            UpdateSigns(arc, ref xyz_signs_begin, arc.Begin(), delta); // ~2**-21 e.g. 4*(2**-23); 4 because it's the largest power of 2 below 2*pi; 2**-23 because it is the smallest mantissa possible in IEEE 754 (for float)
            UpdateSigns(arc, ref xyz_signs_end, arc.End(), -delta);

            // process new lines until the signs match
            while (!SameSigns(ref xyz_signs_begin, ref xyz_signs_end))
            {
                xyz_signs_range_begin = xyz_signs_begin;
                xyz_signs_range_end = xyz_signs_end;

                // binary search and discard ranges with matching slope signs and position signs at ends; and then update the slope signs.
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
                // when you find a sign that switches, log the exact position of the switch with as much precision as possible
                Subdivide(arc, begin, range_begin);
                // when you find that position, you must then switch the x, y, z signs at the new beginning of the arc and the slope signs xyz at the beginning of the arc
                begin = range_end;
                xyz_signs_begin = xyz_signs_range_end;

            }
            // draw the last line
            Subdivide(arc, begin, end);

            arc = arc.next.next; //skip corners //HACK: FIXME: make this elegant
        } while (arc != first_edge);

        ClampToEdges();
        OverlapSharedEdges();
        BuildShape();
    }

    private static void AugmentDictionary(Dictionary<QuadraticBezier, QuadraticBezier> edge_pattern)
    {
        //add edges that weren't added by BuildDictionary (along  the square (0,0) -> (1,0) -> (1,1) -> (0,1) ) using arc and EvaluateNormal information
        //at the discontinuity use Mathf.Sign(EvaluateNormal().y) to find the direction the edge isn't going and add it to one of 8 lists (2 directions * 4 corners)
        //other info you need Sign(UV.x) and Sign(UV.z) which gives you which of the four corners the point is located at

        //after you have created the four lists, you can then start drawing edges between discontinuities (and the points (0,0), (1,0), (1,1), (0,1) ) no other points should be introduced
        //these points will have a control point at (UV1 + UV2) / 2 and should be added both ways

        //possible implementation issues: since this maps a QB to a QB, it should not work using the current naive approach.
        //this is because QB objects are not the same even if they have the same data (because they are compared by pointers)
        //a good solution is to make it Dictionary<int, int> where "int" is the index into List<QuadraticBezier>, this does mean that the created lines need to be added to the "lines" variable
    }

    private static void BuildDictionary(Dictionary<QuadraticBezier, QuadraticBezier> edge_pattern)
    {
        //for each QuadraticBezier and the next; (including last then first !)
        //if QB1.end == QB2.begin (difference's magnitude is less than threshold)
        //add edge from QB1 to QB2 and from QB2 to QB1
    }

    private static void BuildShape()
    {
        //each "shape" may contain more than one shape (e.g. a zig-zag on the southern hemisphere between two octahedral faces will have #zig + #zag + 1 shapes)
        //SVGBuilder.BeginShape();
        //SVGBuilder.SetPoint(SpaceConverter.SphereToUV(first_edge.Evaluate(first_edge.Begin())));
        //SVGBuilder.EndShape();

        Dictionary<QuadraticBezier, QuadraticBezier> edge_pattern = new Dictionary<QuadraticBezier, QuadraticBezier>();
        BuildDictionary(edge_pattern);
        AugmentDictionary(edge_pattern); // add edges that are along the square (0,0) -> (1,0) -> (1,1) -> (0,1)

        //after this point, simply remove edges from edge_pattern until you have none left to draw. (in a double while loop, one for making sure there are none left, the other for drawing the shapes one at a time)
        //since shapes go in both directions, you do need to create a list of visited edges
    }

    private static void ClampToEdges()
    {
        //for each QuadraticBezier and the next; (including last then first !)
        //see if Vector3.Dot(QB1.end - QB1.begin, QB2.begin - QB1.begin) < 0
        //if it is, then there is a discontinuity, since QB1.end should be equal to QB2.begin (approximately)
        //all you need to do is project QB1.end and QB2.begin onto the square (0,0) -> (1,0) -> (1,1) -> (0,1) based on their control point curvature
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

    private static void OverlapSharedEdges()
    {
        //for each QuadraticBezier and the next; (including last then first !)
        //if QB1.end ~= QB2.begin (difference's magnitude is less than threshold)
        //set QB1.end = QB2.begin = (QB1.end + QB2.begin) / 2;
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