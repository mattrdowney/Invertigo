using UnityEngine;
using System.Collections;

public class ShapeToSVG : MonoBehaviour {

	public static void Append(GameObject shape)
    {
        ArcOfSphere first_edge = shape.GetComponentInChildren<Edge>();
        SVGBuilder.BeginShape();
        SVGBuilder.SetPoint(SpaceConverter.SphereToUV(first_edge.Evaluate(first_edge.Begin())));
        ArcOfSphere arc = first_edge;
        do
        {
            float x_sign_beg = Mathf.Sign(arc.Evaluate(0f).x);
            float y_sign_beg = Mathf.Sign(arc.Evaluate(0f).y);
            float z_sign_beg = Mathf.Sign(arc.Evaluate(0f).z);

            float dx_sign_beg = Mathf.Sign(arc.Evaluate(0.0000001f).x - arc.Evaluate(0f).x);
            float dy_sign_beg = Mathf.Sign(arc.Evaluate(0.0000001f).y - arc.Evaluate(0f).y);
            float dz_sign_beg = Mathf.Sign(arc.Evaluate(0.0000001f).z - arc.Evaluate(0f).z);

            float x_sign_end = Mathf.Sign(arc.Evaluate(0f).x);
            float y_sign_end = Mathf.Sign(arc.Evaluate(0f).y);
            float z_sign_end = Mathf.Sign(arc.Evaluate(0f).z);

            float dx_sign_end = Mathf.Sign(arc.Evaluate(arc.End()).x - arc.Evaluate(0.9999999f*arc.End()).x);
            float dy_sign_end = Mathf.Sign(arc.Evaluate(arc.End()).y - arc.Evaluate(0.9999999f*arc.End()).y);
            float dz_sign_end = Mathf.Sign(arc.Evaluate(arc.End()).z - arc.Evaluate(0.9999999f*arc.End()).z);

            /** pseudo-code:
             *  0) if you want to be safe(r?) iterate this function twice for arcs with angle >=180
             *  1) get x, y, and z sign at beginning and end of arc
             *  2) get beginning of arc's slope's sign for xyz (0, 0.0000001) and end of arc's slope's sign for xyz (end * 0.9999999, end)
             *  3) binary search and discard ranges with matching slope signs and position signs at ends; and then update the slope signs.
             *  4) when you find a position where the slope switches but the signs are the same, discard all information in range
             *  5) when you find a position where the positional sign changes, log the exact position of the switch with as much precision as possible
             *  6) when you find that position, you must then switch the x, y, z signs at the new beginning of the arc and the slope signs xyz at the beginning of the arc
             *  7) continue from #3 until you begin = end for positional and slope signs
             *
             *  after you have the positions where the x, y, and z signs flip, you have the seams for the octahedron.
             *  FIXME: make previous algorithm also log slope change positions to make next algorithm accurate
             *  
             *  1) for each segment remaining in range arc.Begin() to arc.End()
             *  2) use bisection search to find the point of maximal error
             *  3) if the max error is greater than a threshold, add that point to the list of render points
             *  4) recursively restart from #2 for the left and right ranges until all segments have less than the threshold of error
             *  5) restart from #1 until all segments in original set have been considered
             */

            for (float iteration = 1; iteration <= 1000; ++iteration) //TODO: use exactly the number iterations neccessary to minimize error below threshold in svg
            {
                SVGBuilder.SetPoint(SpaceConverter.SphereToUV(arc.Evaluate(arc.End() * (iteration / 1000)))); //Assert: begin = 0 for radius of 0
            }
            arc = arc.next.next; //skip corners
        } while (arc != first_edge);
        SVGBuilder.EndShape();
    }
}