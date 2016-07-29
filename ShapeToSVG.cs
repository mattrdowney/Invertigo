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
            for (float iteration = 1; iteration <= 1000; ++iteration) //TODO: use exactly the number iterations neccessary to minimize error below threshold in svg
            {
                SVGBuilder.SetPoint(SpaceConverter.SphereToUV(arc.Evaluate(arc.End()*(iteration/1000)))); //Assert: begin = 0 for radius of 0
            }
            arc = arc.next.next; //skip corners
        } while (arc != first_edge);
        SVGBuilder.EndShape();
    }
}