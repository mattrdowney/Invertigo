using UnityEngine;
using System.Collections;

public class Octahedron
{
    static optional<Mesh> mesh;

	public static Mesh OctahedronMesh()
    {
        if (mesh.exists)
        {
            return mesh.data;
        }
        else
        {
            mesh = new Mesh();
            Mesh octahedron = mesh.data; // FIXME?

            Vector3[] verts = new Vector3[9];
            Vector2[] uvs   = new Vector2[9];
            int[]     tris  = new int[3 * 8];

            verts[0] = Vector3.up;
            verts[1] = Vector3.right;
            verts[2] = Vector3.forward;
            verts[3] = Vector3.left;
            verts[4] = Vector3.back;
            verts[5] = Vector3.down;
            verts[6] = Vector3.down;
            verts[7] = Vector3.down;
            verts[8] = Vector3.down;

            uvs[0] = new Vector2(0.5f, 0.5f); // up
            uvs[1] = new Vector2(1.0f, 0.5f); // right
            uvs[2] = new Vector2(0.5f, 1.0f); // forward
            uvs[3] = new Vector2(0.0f, 0.5f); // left
            uvs[4] = new Vector2(0.5f, 0.0f); // back
            uvs[5] = new Vector2(1.0f, 1.0f); // down
            uvs[6] = new Vector2(0.0f, 1.0f); // down
            uvs[7] = new Vector2(0.0f, 0.0f); // down
            uvs[8] = new Vector2(1.0f, 0.0f); // down

            tris[0]  = 0; tris[1]  = 2; tris[2]  = 1; //+x, +y, +z
            tris[3]  = 0; tris[4]  = 3; tris[5]  = 2; //-x, +y, +z
            tris[6]  = 0; tris[7]  = 4; tris[8]  = 3; //-x, +y, -z
            tris[9]  = 0; tris[10] = 1; tris[11] = 4; //+x, +y, -z
            tris[12] = 5; tris[13] = 1; tris[14] = 2; //+x, -y, +z
            tris[15] = 6; tris[16] = 2; tris[17] = 3; //-x, -y, +z
            tris[18] = 7; tris[19] = 3; tris[20] = 4; //-x, -y, -z
            tris[21] = 8; tris[22] = 4; tris[23] = 1; //+x, -y, -z

            octahedron.vertices = verts;
            octahedron.uv = uvs;
            octahedron.triangles = tris;

            octahedron.RecalculateBounds();
            octahedron.RecalculateNormals();
            octahedron.Optimize();

            return octahedron;
        }
	}
}
