using UnityEngine;

public class ShapeMaker
{
	//static ArcOfSphere selected;

	public static Edge StartShape(Transform block_transform, Vector3 left_edge, Vector3 right_edge)
	{
		Edge edge1 = Edge.StartEdge(block_transform, left_edge, right_edge);
		Edge edge2 = Edge.LinkRight(edge1, left_edge);

		ConvexCorner corner1 = ConvexCorner.Spawn(edge1, edge2); //could be concave
		ConvexCorner corner2 = ConvexCorner.Spawn(edge2, edge1); //ditto

		return edge2;
	}

	public static Edge DivideEdge(Edge edge, Vector3 division_point)
	{
		Corner left_corner  = edge.prev as Corner; //FIXME: logic after this regarding nullity
		Corner right_corner = edge.next as Corner;

		Edge edge2 = Edge.LinkRight(left_corner, division_point);

		edge.Initialize(division_point, edge.Evaluate(edge.End()));

		Corner corner;

		if(Edge.IsConvex(edge, edge2))
		{
			corner = ConvexCorner.Spawn(edge2, edge);
		}
		else
		{
			corner = ConcaveCorner.Spawn(edge2, edge);
		}

		left_corner .Initialize(left_corner .prev, left_corner .next);
		right_corner.Initialize(right_corner.prev, right_corner.next);
		
		return edge;
	}

	/*public ArcOfSphere RemoveCorner()
	{
		DebugUtility.Assert(LengthRadius() == 0, "Trying to remove edge!");
		
		ArcOfSphere left_edge  = prev;
		ArcOfSphere right_edge = next;
		ArcOfSphere left_corner  = left_edge.prev;
		ArcOfSphere right_corner = right_edge.next;
		
		left_edge.Save();
		left_corner.Save();
		right_corner.Save();
		
		Vector3 begin = left_edge.Evaluate(left_edge.Begin());
		Vector3 end   = right_edge.Evaluate(right_edge.End());
		
		left_edge.Relink(left_corner, right_corner);
		left_edge.Initialize(begin, end);
		
		Undo.DestroyObjectImmediate(this.gameObject);
		Undo.DestroyObjectImmediate(right_edge.gameObject);
		//DestroyImmediate(this.gameObject);
		//DestroyImmediate(right_edge.gameObject);
		
		left_corner .InitializeCorner(left_corner .prev, left_corner .next);
		right_corner.InitializeCorner(right_corner.prev, right_corner.next);
		
		return left_edge;
	}*/
}
