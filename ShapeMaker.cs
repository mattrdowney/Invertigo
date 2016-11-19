using UnityEngine;
using UnityEditor;

public class ShapeMaker
{
	//static ArcOfSphere selected;

	public static Edge StartShape(Transform block_transform, Vector3 left_edge, Vector3 right_edge)
	{
		Edge edge1 = Edge.StartEdge(block_transform, left_edge, right_edge);
		Edge edge2 = Edge.LinkRight(edge1, left_edge);

		Corner corner1 = SpawnCorner(edge1, edge2, null);
		Corner corner2 = SpawnCorner(edge2, edge1, null);

		return edge2;
	}

	public static Edge DivideEdge(Edge edge, Vector3 division_point)
	{
		Corner left_corner  = edge.prev as Corner; //FIXME: logic after this regarding nullity
		Corner right_corner = edge.next as Corner;

		Edge edge2 = Edge.LinkRight(left_corner, division_point);

		edge.Initialize(division_point, edge.Evaluate(edge.End()));

		SpawnCorner(edge2, edge, null);
		SpawnCorner(left_corner.prev as Edge, left_corner.next as Edge, left_corner);
		SpawnCorner(right_corner.prev as Edge, right_corner.next as Edge, right_corner);
		
		return edge;
	}

	public static Corner SpawnCorner(Edge left, Edge right, Corner old)
	{
		if(old)
		{
			Undo.DestroyObjectImmediate(old.gameObject);
		}

		Corner result;

		if(Edge.IsConvex(left, right))
		{
			result = ConvexCorner.Spawn(left, right);
		}
		else
		{
			result = ConcaveCorner.Spawn(left, right);
		}

		result.Initialize(left, right);

		result.Save();
		left.Save();
		right.Save();

		return result;
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
