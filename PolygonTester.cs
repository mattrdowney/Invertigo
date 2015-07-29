using UnityEngine;

public class PolygonTester : MonoBehaviour {
	void Start () {
		// Create Vector2 vertices
		Vector2[] vertices2D = new Vector2[] {
			new Vector2(0,0),
			new Vector2(10,50),
			new Vector2(40,50),
			new Vector2(50,90),
			new Vector2(30,100),
			new Vector2(0,120),
			new Vector2(140,130),
			new Vector2(130,80),
			new Vector2(100,120),
			new Vector2(110,60),
			new Vector2(150,40),
			new Vector2(120,0),
		};
		
		// Use the triangulator to get indices for creating triangles
		Triangulator tr = new Triangulator(vertices2D);
		int[] indices = tr.Triangulate();
		
		// Create the Vector3 vertices
		Vector3[] vertices = new Vector3[vertices2D.Length];
		for (int i=0; i<vertices.Length; i++) {
			vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
		}
		
		// Create the mesh
		Mesh msh = new Mesh();
		msh.vertices = vertices;
		msh.triangles = indices;
		msh.RecalculateNormals();
		msh.RecalculateBounds();
		
		// Set up game object with mesh;
		gameObject.AddComponent(typeof(MeshRenderer));
		MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		filter.mesh = msh;
	}
}