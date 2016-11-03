using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShapeToSVG : MonoBehaviour
{
    static List<QuadraticBezier> lines;

    static float delta = 4 * Mathf.Pow(2, -23); // 4 because it's the largest power of 2 below 2*pi; 2**-23 because it is the smallest mantissa possible in IEEE 754 (for float)
    static float threshold = 1e-5f;

    enum SquareFace { kTop, kRight, kBottom, kLeft, kNone }

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
                while (range_end - range_begin > delta)
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

    private static void AugmentDictionary(Dictionary<QuadraticBezier, QuadraticBezier> edge_pattern, List<QuadraticBezier> start_discontinuities, List<QuadraticBezier> end_discontinuities)
    {
        SortedDictionary<float, QuadraticBezier> edge_map = new SortedDictionary<float, QuadraticBezier>(); // HACK: to be foolproof this needs to be Tuple<QB, QB> to store forwards and backwards references; the hack should work as long as the level design is simple. (I think that level designs that break this also break the physics code too.)

        //create quick map for locating consecutive QB's on edge of square
        BuildEdgeMap(edge_map, start_discontinuities);
        BuildEdgeMap(edge_map, end_discontinuities);

        //add edges that weren't added by BuildDictionary (along  the square (0,0) -> (1,0) -> (1,1) -> (0,1) )
        //using function calls is a better idea (and getting rid of the enum)
        optional<QuadraticBezier> last_bezier = new optional<QuadraticBezier>();
        optional<QuadraticBezier> this_bezier = NextDiscontinuity(edge_map, last_bezier);
        optional<QuadraticBezier> first_bezier = this_bezier;

        while (this_bezier != first_bezier)
        {
            if (this_bezier.exists && last_bezier.exists)
            {
                edge_pattern.Add(last_bezier.data, this_bezier.data);
            }
            else if (!this_bezier.exists && last_bezier.exists)
            {
                edge_pattern.Add(last_bezier.data, first_bezier.data);
            }

            last_bezier = this_bezier;
            this_bezier = NextDiscontinuity(edge_map, this_bezier);
        }

        //using arc and EvaluateNormal information
        //at the discontinuity use Mathf.Sign(EvaluateNormal().y) to find the direction the edge isn't going and add it to one of 1 or 4 or 8 lists (2 directions * 4 corners)
        //other info you need Sign(UV.x) and Sign(UV.z) which gives you which of the four corners the point is located at

        //after you have created the four lists, you can then start drawing edges between discontinuities (and the points (0,0), (1,0), (1,1), (0,1) ) no other points should be introduced
        //these points will have a control point at (UV1 + UV2) / 2
    }

    private static void BuildDictionary(Dictionary<QuadraticBezier, QuadraticBezier> edge_pattern)
    {
        for (int index = 0; index < lines.Count; ++index) //for each QuadraticBezier and the next; (including last then first !)
        {
            QuadraticBezier QB1 = lines[index];
            QuadraticBezier QB2 = lines[(index + 1) % lines.Count];

            if (QB1.end_UV == QB2.begin_UV) // take identical points and link them
            {
                edge_pattern.Add(QB1, QB2); //should work since it's referencing the QuadraticBezier inside of the List<QB>
            }
        }
    }

    private static void BuildEdgeMap(SortedDictionary<float, QuadraticBezier> edge_map, List<QuadraticBezier> discontinuities)
    {
        //create quick map for locating consecutive QB's on edge of square
        for (int index = 0; index < discontinuities.Count; ++index)
        {
            if (discontinuities[index].end_UV.x == 0.0f) //left
            {
                edge_map.Add(discontinuities[index].end_UV.y + 3, discontinuities[index]);
            }
            else if (discontinuities[index].end_UV.x == 1.0f) //right
            {
                edge_map.Add(1f - discontinuities[index].end_UV.y + 1, discontinuities[index]);
            }
            else if (discontinuities[index].end_UV.y == 0.0f) //bottom
            {
                edge_map.Add(1f - discontinuities[index].end_UV.x + 2, discontinuities[index]);
            }
            else //top
            {
                edge_map.Add(discontinuities[index].end_UV.x + 0, discontinuities[index]);
            }
        }
    }

    private static void BuildShape()
    {
        //each "shape" may contain more than one shape (e.g. a zig-zag on the southern hemisphere between two octahedral faces will have #zig + #zag + 1 shapes)

        Dictionary<QuadraticBezier, QuadraticBezier> edge_pattern = new Dictionary<QuadraticBezier, QuadraticBezier>();
        BuildDictionary(edge_pattern);
        List<QuadraticBezier> start_discontinuities = new List<QuadraticBezier>();
        List<QuadraticBezier> end_discontinuities = new List<QuadraticBezier>();
        DiscontinuityLocations(start_discontinuities, end_discontinuities);
        AugmentDictionary(edge_pattern, start_discontinuities, end_discontinuities); // add edges that are along the square (0,0) -> (1,0) -> (1,1) -> (0,1)

        while (edge_pattern.Count != 0) // make sure there are no more shapes left to process
        {
            QuadraticBezier first_edge = edge_pattern.GetEnumerator().Current.Key;
            QuadraticBezier current_edge = first_edge;

            SVGBuilder.BeginShape();
            while (current_edge != first_edge) // process every edge in each shape
            {
                SVGBuilder.SetEdge(current_edge);
                edge_pattern.Remove(current_edge);
            }
            SVGBuilder.EndShape();
        }
    }

    private static void ClampToEdges()
    {
        for(int index = 0; index < lines.Count; ++index) //for each QuadraticBezier and the next; (including last then first !)
        {
            QuadraticBezier QB1 = lines[index];
            QuadraticBezier QB2 = lines[(index + 1) % lines.Count];

            if ((QB1.end_UV - QB2.begin_UV).magnitude > threshold) //if end is not close to begin, there is a discontinuity
            {
                ProjectOntoSquare(ref QB1.end_UV, QB1.control_point); // project the points onto the edge of a square
                ProjectOntoSquare(ref QB2.begin_UV, QB2.control_point);
            }
        }
    }

    private static void DiscontinuityLocations(List<QuadraticBezier> start_discontinuities, List<QuadraticBezier> end_discontinuities)
    {
        for (int index = 0; index < lines.Count; ++index) //for each QuadraticBezier and the next; (including last then first !)
        {
            QuadraticBezier QB1 = lines[index];
            QuadraticBezier QB2 = lines[(index + 1) % lines.Count];

            if (QB1.end_UV != QB2.begin_UV) //if there is a discontinuity
            {
                start_discontinuities.Add(QB1);
                end_discontinuities.Add(QB2);
            }
        }
    }

    private static Vector2 Intersection(Vector2 begin, Vector2 after_begin, Vector2 before_end, Vector2 end)
    {
        float numerator_x, numerator_y, denominator;

        numerator_x = (begin.x * after_begin.y - begin.y * after_begin.x) * (before_end.x - end.x) -
                      (before_end.x * end.y - before_end.y * end.x) * (begin.x - after_begin.x);

        numerator_y = (begin.x * after_begin.y - begin.y * after_begin.x) * (before_end.y - end.y) -
                      (before_end.x * end.y - before_end.y * end.x) * (begin.y - after_begin.y);

        denominator = (begin.x - after_begin.x) * (before_end.y - end.y) -
                      (begin.y - after_begin.y) * (before_end.x - end.x);

        return new Vector2(numerator_x / denominator, numerator_y / denominator);
    }

    private static float MaxErrorLocation(ArcOfSphere arc, float begin, float end)
    {
        Vector2 L1 = SpaceConverter.SphereToUV(arc.Evaluate(begin));
        Vector2 L2 = SpaceConverter.SphereToUV(arc.Evaluate(end));

        // 2) use binary / bisection search to find the point of maximal error
        while (end - begin > delta)
        {
            float midpoint = (begin + end) / 2;

            float error_left  = Point_Line_Distance(L1, L2, SpaceConverter.SphereToUV(arc.Evaluate(midpoint - delta)));
            float error_right = Point_Line_Distance(L1, L2, SpaceConverter.SphereToUV(arc.Evaluate(midpoint + delta)));

            if (error_left < error_right) //error begin should be replaced since it has less error
            {
                begin = midpoint;
            }
            else //error end should be replaced since it has less error
            {
                end = midpoint;
            }
        }

        return (begin + end) / 2; // return location of max error
    }

    private static optional<QuadraticBezier> NextDiscontinuity(SortedDictionary<float, QuadraticBezier> edge_map, optional<QuadraticBezier> last_bezier)
    {
        int index = 0; //never resets on second or later function calls

        return null;
    }

    private static void OverlapSharedEdges()
    {
        for (int index = 0; index < lines.Count; ++index) //for each QuadraticBezier and the next; (including last then first !)
        {
            QuadraticBezier QB1 = lines[index];
            QuadraticBezier QB2 = lines[(index + 1) % lines.Count];

            if ((QB1.end_UV - QB2.begin_UV).magnitude < threshold) //if QB1.end ~= QB2.begin
            {
                QB1.end = QB2.begin = (QB1.end + QB2.begin) / 2; //make the points equal
            }
        }
    }

    private static float Point_Line_Distance(Vector2 L1, Vector2 L2, Vector2 P)
    {
        float numerator = Mathf.Abs((L2.y - L1.y) * P.x - (L2.x - L1.x) * P.y + L2.x * L1.y - L2.y * L1.x);
        float denominator = Mathf.Sqrt(Mathf.Pow((L2.y - L1.y), 2) + Mathf.Pow((L2.x - L1.x), 2));

        return numerator / denominator;
    }

    private static void ProjectOntoSquare(ref Vector2 point, Vector2 control_point)
    {
        Vector2 upper = Intersection(control_point, point, Vector2.up, Vector2.one);
        Vector2 lower = Intersection(control_point, point, Vector2.zero, Vector2.right);
        Vector2 left  = Intersection(control_point, point, Vector2.zero, Vector2.up);
        Vector2 right = Intersection(control_point, point, Vector2.right, Vector2.one);

        if (upper.y == 1 && 0 <= upper.x && upper.x <= 1) // is the upper intersection valid? (on the upper boarder of a unit square?)
        {
            point = upper;
        }
        else if (lower.y == 0 && 0 <= lower.x && lower.x <= 1) // is the lower intersection valid? 
        {
            point = lower;
        }
        else if (right.x == 1 && 0 <= right.y && right.y <= 1) // is the right intersection valid?
        {
            point = right;
        }
        else if (left.x == 0 && 0 <= left.y && left.y <= 1) // if the left intersection valid?
        {
            point = left;
        }
        else
        {
            DebugUtility.Print("ProjectOntoSquare failed.");
        }
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
        float midpoint = MaxErrorLocation(arc, begin, end);

        Vector2 L1 = SpaceConverter.SphereToUV(arc.Evaluate(begin));
        Vector2 L2 = SpaceConverter.SphereToUV(arc.Evaluate(end));
        Vector2 P = SpaceConverter.SphereToUV(arc.Evaluate(midpoint));

        if (Point_Line_Distance(L1, L2, P) > threshold) // if the max error is greater than a threshold, recursively add the left and right halves into the list of lines
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