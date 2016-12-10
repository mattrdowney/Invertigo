using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShapeToSVG : MonoBehaviour
{
    static readonly float delta = 4 * Mathf.Pow(2, -23); // 4 because it's the largest power of 2 below 2*pi; 2**-23 because it is the smallest mantissa possible in IEEE 754 (for float)
    const float threshold = 1e-5f;

    static List<QuadraticBezier> lines;
    static List<QuadraticBezier> start_discontinuities;
    static List<QuadraticBezier> end_discontinuities;
    static Dictionary<QuadraticBezier, QuadraticBezier> edge_pattern; // HACK: to be foolproof this needs to be Tuple<QB, QB> to store forwards and backwards references; the hack should work as long as the level design is simple. (I think that level designs that break this also break the physics code too.)
    static SortedList<float, QuadraticBezier> start_edge_map;
    static SortedList<float, QuadraticBezier> end_edge_map;
    static bool swapped;
    static bool start;
    static IEnumerator<KeyValuePair<float, QuadraticBezier>> start_edge_map_iter;
    static IEnumerator<KeyValuePair<float, QuadraticBezier>> end_edge_map_iter;

    public static void AddLine(ArcOfSphere edge, float begin, float end)
    {
        Vector2 begin_UV = SpaceConverter.SphereToUV(edge.Evaluate(begin));
        Vector2 end_UV = SpaceConverter.SphereToUV(edge.Evaluate(end));

        if (Vector2.Distance(begin_UV, end_UV) > threshold)
        {
            Vector2 delta_begin_UV = SpaceConverter.SphereToUV(edge.Evaluate(begin + 64*delta)); // 4096 == sqrt(bits(2^mantissa))
            Vector2 delta_end_UV = SpaceConverter.SphereToUV(edge.Evaluate(end - 64*delta));
            Vector2 control_point = Intersection(begin_UV, delta_begin_UV, delta_end_UV, end_UV);

            DebugUtility.Log("AddLine:", begin_UV, end_UV);

            lines.Add(new QuadraticBezier(edge, begin_UV, control_point, end_UV, begin, end));
        }
    }

    public static void Append(GameObject shape)
    {
        lines = new List<QuadraticBezier>();

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
            UpdateSigns(arc, ref xyz_signs_begin, arc.Begin(), delta);
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

            arc = arc.next; // move to next arc
            while(arc.GetType().IsSubclassOf(typeof(Corner))) // skip corners //XXX: Corner is abstract... for now
            {
                arc = arc.next;
            }
        } while (arc != first_edge);
        ClampToEdges();
        OverlapSharedEdges();
        BuildShape();
    }

    private static void AugmentDictionary()
    {
        start_edge_map = new SortedList<float, QuadraticBezier>();
        end_edge_map = new SortedList<float, QuadraticBezier>();

        //create quick map for locating consecutive QB's on edge of square
        for (int index = 0; index < start_discontinuities.Count; ++index)
        {
            start_edge_map.Add(EdgeMapKey(start_discontinuities[index].end_UV), start_discontinuities[index]);
            DebugUtility.Log("start_edge_map", EdgeMapKey(start_discontinuities[index].end_UV));
        }
        for (int index = 0; index < end_discontinuities.Count; ++index)
        {
            end_edge_map.Add(EdgeMapKey(end_discontinuities[index].begin_UV), end_discontinuities[index]);
            DebugUtility.Log("end_edge_map", EdgeMapKey(end_discontinuities[index].begin_UV));
        }
        
        optional<QuadraticBezier> last_bezier = GetFirstDiscontinuity();
        optional<QuadraticBezier> this_bezier = GetSecondDiscontinuity();
        optional<QuadraticBezier> first_bezier = last_bezier;

        while (true) // TODO: clean up / make pretty / make elegant
        {
            Debug.Log(last_bezier);
            Debug.Log(this_bezier);

            if (last_bezier.exists && this_bezier.exists)
            {
                StitchTogether(last_bezier.data, this_bezier.data);
            }

            last_bezier = NextDiscontinuity();
            this_bezier = NextDiscontinuity();

            if (last_bezier == first_bezier || this_bezier == first_bezier)
            {
                break;
            }
        }
    }

    private static void BuildDictionary()
    {
        for (int index = 0; index < lines.Count; ++index) //for each QuadraticBezier and the next; (including last then first !)
        {
            QuadraticBezier QB1 = lines[index];
            QuadraticBezier QB2 = lines[(index + 1) % lines.Count];

            if (!NearBoundary(QB1.end_UV) && !NearBoundary(QB2.begin_UV)) // take identical points and link them
            {
                edge_pattern.Add(QB1, QB2); //should work since it's referencing the QuadraticBezier inside of the List<QB>
            }
        }
    }

    private static void BuildShape()
    {
        //each "shape" may contain more than one shape (e.g. a zig-zag on the southern hemisphere between two octahedral faces will have #zig + #zag + 1 shapes)

        edge_pattern = new Dictionary<QuadraticBezier, QuadraticBezier>();
        BuildDictionary();
        DiscontinuityLocations();
        AugmentDictionary(); // add edges that are along the square (0,0) -> (1,0) -> (1,1) -> (0,1)
        while (edge_pattern.Count != 0) // make sure there are no more shapes left to process
        {
            Dictionary<QuadraticBezier, QuadraticBezier>.Enumerator iter = edge_pattern.GetEnumerator();
            iter.MoveNext();
            QuadraticBezier first_edge = iter.Current.Key;
            QuadraticBezier current_edge = first_edge;

            SVGBuilder.BeginShape();
            do // process every edge in each shape
            {
                SVGBuilder.SetEdge(current_edge);
                QuadraticBezier temp_edge = current_edge;
                DebugUtility.Log("Cycling:", current_edge.end_UV, current_edge.begin_UV);
                current_edge = edge_pattern[current_edge];
                edge_pattern.Remove(temp_edge);
            } while (current_edge != first_edge);
            SVGBuilder.EndShape();
        }
    }

    private static void ClampToEdges()
    {
        for (int index = 0; index < lines.Count; ++index) //for each QuadraticBezier and the next; (including last then first !)
        {
            QuadraticBezier QB1 = lines[index];
            QuadraticBezier QB2 = lines[(index + 1) % lines.Count];
            
            if (NearBoundary(QB1.end_UV)) //if end is not close to begin, there is a discontinuity
            {
                ProjectOntoSquare(ref QB1.end_UV, QB1.control_point); // project the points onto the edge of a square
                ProjectOntoSquare(ref QB2.begin_UV, QB2.control_point);
            }
        }
    }

    private static Vector3 ClockwiseDirection(float edge_interpolation)
    {
        Vector2 unit_square_point;

        if (edge_interpolation < 1)
        {
            unit_square_point = new Vector2(edge_interpolation, 1);
        }
        else if (edge_interpolation < 2)
        {
            unit_square_point = new Vector2(1, 2 - edge_interpolation);
        }
        else if (edge_interpolation < 3)
        {
            unit_square_point = new Vector2(3 - edge_interpolation, 0);
        }
        else
        {
            unit_square_point = new Vector2(0, edge_interpolation - 3);
        }

        Vector2 circle_point = (unit_square_point - new Vector2(0.5f, 0.5f)).normalized;

        Vector3 augmented_circle_point = new Vector3(circle_point.x, 0, circle_point.y);

        Vector3 result = Vector3.Cross(Vector3.up, augmented_circle_point).normalized;

        int i = 0;

        return result;
    }

    private static void DiscontinuityLocations()
    {
        start_discontinuities = new List<QuadraticBezier>();
        end_discontinuities = new List<QuadraticBezier>();
        for (int index = 0; index < lines.Count; ++index) //for each QuadraticBezier and the next; (including last then first !)
        {
            QuadraticBezier QB = lines[index];
            
            if (NearBoundary(QB.begin_UV) && !NearBoundary(QB.end_UV)) // just because this can work does not mean it will always work (at least in theory)... 
            {
                DebugUtility.Log(QB.begin_UV);
                end_discontinuities.Add(QB);
            }
            else if (NearBoundary(QB.end_UV) && !NearBoundary(QB.begin_UV))
            {
                DebugUtility.Log(QB.end_UV);
                start_discontinuities.Add(QB);
            }
            else if (NearBoundary(QB.begin_UV) && NearBoundary(QB.end_UV) && //TODO: CHECK: include all vertexes unless the line is parallel to the boundary
                Mathf.Floor(EdgeMapKey(QB.end_UV)) != Mathf.Floor(EdgeMapKey(QB.begin_UV)))
            {
                DebugUtility.Log(QB.end_UV);
                DebugUtility.Log(QB.begin_UV);
                end_discontinuities.Add(QB);
                start_discontinuities.Add(QB);
            }
        }
    }
    /** length travelled along the perimeter of a unit square starting from the top left and moving clockwise
     *  @param uv the coordinates along the boundary of the unit square (both coordinates must be [0,1] and at least one coordinate must be exactly 0 or 1
     *  @return length travelled along the perimeter of a unit square starting from the top left and moving clockwise
     */
    private static float EdgeMapKey(Vector2 uv) // isolate the behavior that varies!
    {
        if (uv.y == 1.0f) // top
        {
            return 0.0f + (uv.x);
        }
        else if (uv.x == 1.0f) // right
        {
            return 1.0f + (1.0f - uv.y);
        }
        else if (uv.y == 0.0f) // bottom
        {
            return 2.0f + (1.0f - uv.x);
        }
        else if (uv.x == 0.0f) // left
        {
            return 3.0f + (uv.y);
        }
        DebugUtility.Log("ShapeToSVG.cs: EdgeMapKey(): input error");
        return 4.0f;
    }

    private static optional<QuadraticBezier> GetFirstDiscontinuity()
    {
        swapped = false; // FIXME: gotta hate bools
        start = false;
        start_edge_map_iter = start_edge_map.GetEnumerator();

        if (!start_edge_map_iter.MoveNext())
        {
            return new optional<QuadraticBezier>();
        }
        
        float similarity = Vector3.Dot(ClockwiseDirection(start_edge_map_iter.Current.Key),
            start_edge_map_iter.Current.Value.arc.EvaluateRight(start_edge_map_iter.Current.Value.end));

        DebugUtility.Log("Similarity: ", similarity);

        if (similarity < 0)
        {
            SortedList<float, QuadraticBezier> swapper = start_edge_map;
            start_edge_map = end_edge_map;
            end_edge_map = swapper;
            optional<QuadraticBezier> result = GetFirstDiscontinuity();
            swapped = true;
            return result;
        }

        DebugUtility.Log("First: ", start_edge_map_iter.Current.Key);

        return start_edge_map_iter.Current.Value;
    }

    private static optional<QuadraticBezier> GetSecondDiscontinuity()
    {
        end_edge_map_iter = end_edge_map.GetEnumerator();

        if (!end_edge_map_iter.MoveNext())
        {
            return new optional<QuadraticBezier>();
        }

        while (end_edge_map_iter.Current.Key <= start_edge_map_iter.Current.Key)
        {
            if (!end_edge_map_iter.MoveNext())
            {
                end_edge_map_iter = end_edge_map.GetEnumerator();
                end_edge_map_iter.MoveNext();
                break;
            }
        }
        float difference = end_edge_map_iter.Current.Key - start_edge_map_iter.Current.Key;
        if ( 0 < difference && difference <= 16 * delta) // the edge is too close and was placed on the wrong side due to floating point imprecision
        {
            if (!end_edge_map_iter.MoveNext())
            {
                end_edge_map_iter = end_edge_map.GetEnumerator();
                end_edge_map_iter.MoveNext();
            }
        }

        DebugUtility.Log("Second: ", end_edge_map_iter.Current.Key);

        return end_edge_map_iter.Current.Value;
    }

    private static Vector2 Intersection(Vector2 begin, Vector2 after_begin, Vector2 before_end, Vector2 end) // TODO: make this function do what it says
    {
        float numerator_x = (begin.x * after_begin.y - begin.y * after_begin.x) * (before_end.x - end.x) -
                            (before_end.x * end.y - before_end.y * end.x) * (begin.x - after_begin.x);

        float numerator_y = (begin.x * after_begin.y - begin.y * after_begin.x) * (before_end.y - end.y) -
                            (before_end.x * end.y - before_end.y * end.x) * (begin.y - after_begin.y);

        float denominator = (begin.x - after_begin.x) * (before_end.y - end.y) -
                            (begin.y - after_begin.y) * (before_end.x - end.x);

        Vector2 result = new Vector2(numerator_x / denominator, numerator_y / denominator);

        if (System.Single.IsNaN(result.x) || // if there are infinite solutions (or none)
                System.Single.IsPositiveInfinity(result.x) ||
                System.Single.IsNegativeInfinity(result.x)) // checking for x implicitly checks for y
        {
            return (begin + end) / 2;
        }

        return result;
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

    private static bool NearBoundary(Vector2 uv)
    {
        return uv.x >= 1.0f - threshold || uv.x <= threshold ||
               uv.y >= 1.0f - threshold || uv.y <= threshold;
    }

    private static optional<Vector2> NextCorner(float first, float last, bool clockwise)
    {
        float end = last;
        if (end != (int) end)
        {
            end = NextEdge(last, clockwise);
        }

        float next = NextEdge(first, clockwise);
        if (next != end)
        {
            switch ((int)next)
            {
                case 0: case 4:
                    return Vector2.up;
                case 1:
                    return Vector2.one;
                case 2:
                    return Vector2.right;
                case 3:
                    return Vector2.zero;
            } 
        }

        return new optional<Vector2>();
    }

    private static optional<QuadraticBezier> NextDiscontinuity() // ugly, ugly, ugly
    {
        start = !start;
        if (start)
        {
            if (start_edge_map_iter.MoveNext())
            {
                return start_edge_map_iter.Current.Value;
            }
            else
            {
                start_edge_map_iter = start_edge_map.GetEnumerator();
                if (start_edge_map_iter.MoveNext())
                {
                    return start_edge_map_iter.Current.Value;
                }
                else
                {
                    return new optional<QuadraticBezier>();
                }
            }
        }
        else
        {
            if (end_edge_map_iter.MoveNext())
            {
                return end_edge_map_iter.Current.Value;
            }
            else
            {
                end_edge_map_iter = end_edge_map.GetEnumerator();
                if (end_edge_map_iter.MoveNext())
                {
                    return end_edge_map_iter.Current.Value;
                }
                else
                {
                    return new optional<QuadraticBezier>();
                }
            }
        }
    }

    private static float NextEdge(float current, bool clockwise)
    {
        float next;
        if (clockwise)
        {
            if (Mathf.Ceil(current) == current)
            {
                next = current + 1;
            }
            else
            {
                next = Mathf.Ceil(current);
            }
            if (next >= 4)
            {
                next -= 4;
            }
        }
        else // counterclockwise
        {
            if (Mathf.Floor(current) == current)
            {
                next = current - 1;
            }
            else
            {
                next = Mathf.Floor(current);
            }
            if (next < 0)
            {
                next += 4;
            }
        }
        return next;
    }

    private static void OverlapSharedEdges()
    {
        for (int index = 0; index < lines.Count; ++index) //for each QuadraticBezier and the next; (including last then first !)
        {
            QuadraticBezier QB1 = lines[index];
            QuadraticBezier QB2 = lines[(index + 1) % lines.Count];

            if (Vector2.Distance(QB1.end_UV, QB2.begin_UV) < threshold) //if QB1.end ~= QB2.begin
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

    private static void ProjectOntoSquare(ref Vector2 point, Vector2 control_point) // TODO: CHECK: this had an error and might still have one
    {
        Vector2 upper = Intersection(control_point, point, Vector2.up, Vector2.one);
        Vector2 lower = Intersection(control_point, point, Vector2.zero, Vector2.right);
        Vector2 left  = Intersection(control_point, point, Vector2.zero, Vector2.up);
        Vector2 right = Intersection(control_point, point, Vector2.right, Vector2.one);

        Vector2 closestPoint = Vector2.zero;
        float distance = Mathf.Infinity;

        if (upper.y == 1 && 0 <= upper.x && upper.x <= 1) // is the upper intersection valid? (on the upper boarder of a unit square?)
        {
            if (distance > Vector2.Distance(upper, point)) // if upper intersection is the closest so far
            {
                closestPoint = upper;
                distance = Vector2.Distance(upper, point);
            }
        }
        if (lower.y == 0 && 0 <= lower.x && lower.x <= 1) // is the lower intersection valid? 
        {
            if (distance > Vector2.Distance(lower, point))
            {
                closestPoint = lower;
                distance = Vector2.Distance(lower, point);
            }
        }
        if (right.x == 1 && 0 <= right.y && right.y <= 1) // is the right intersection valid?
        {
            if (distance > Vector2.Distance(right, point))
            {
                closestPoint = right;
                distance = Vector2.Distance(right, point);
            }
        }
        if (left.x == 0 && 0 <= left.y && left.y <= 1) // if the left intersection valid?
        {
            if (distance > Vector2.Distance(left, point))
            {
                closestPoint = left;
                distance = Vector2.Distance(left, point);
            }
        }

        point = closestPoint;
    }

    private static bool SameSigns(ref int[,] data_A, ref int[,] data_B) // for the purpose of this function, zero is not considered a sign
    {
        for (int derivative = 0; derivative < 2; ++derivative)
        {
            for (int dimension = 0; dimension < 3; ++dimension)
            {
                if (data_A[derivative, dimension] != data_B[derivative, dimension])
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static void StitchTogether(QuadraticBezier cursor, QuadraticBezier last)
    {
        if (swapped)
        {
            QuadraticBezier swapper = cursor;
            cursor = last;
            last = swapper;
        }

        DebugUtility.Log("Stitching:", cursor.end_UV, last.begin_UV);
        float cursor_key = EdgeMapKey(cursor.end_UV);
        float last_key = EdgeMapKey(last.begin_UV);

        bool clockwise = Vector3.Dot(ClockwiseDirection(cursor_key),
            cursor.arc.EvaluateRight(cursor.end)) > 0;

        // add edges that weren't added by BuildDictionary (along  the square (0,0) -> (1,0) -> (1,1) -> (0,1) )
        optional<Vector2> corner = NextCorner(cursor_key, last_key, clockwise);
        Vector2 control_point;
        QuadraticBezier intermediate_line;
        while (corner.exists)
        {
            Debug.Log(corner);

            control_point = (cursor.end_UV + corner.data) / 2;
            intermediate_line = new QuadraticBezier(null, cursor.end_UV, control_point, corner.data, -1f, -1f);
            edge_pattern.Add(cursor, intermediate_line);
            cursor = intermediate_line;
            cursor_key = EdgeMapKey(cursor.end_UV);
            corner = NextCorner(cursor_key, last_key, clockwise);
        }
        control_point = (cursor.end_UV + last.begin_UV) / 2;
        intermediate_line = new QuadraticBezier(null, cursor.end_UV, control_point, last.begin_UV, -1f, -1f);
        edge_pattern.Add(cursor, intermediate_line);
        edge_pattern.Add(intermediate_line, last);
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