using UnityEngine;
using System.Collections;

public class TrackSegment : MonoBehaviour {
    public Vector3 pos_a, pos_b;
    // Bezier control points (in same coordinate system as start and end points)
    public Vector3 control_a, control_b;
    public Vector3 up_a, up_b;
    public float width = 1.0f;
    public int n_points = 20;

    Mesh mesh;
    Vector3[] verts;
    int[] tris;
    Vector3[] normals;

    // Get track centreline at parametric coord t, based on the control and endpoints.
    Vector3 GetBezier(float t)  {
        if (t == 0)
            return pos_a;
        //float ratio = (1 - t) / t;
        //return t * t * t * (pos_b + ratio * (control_b + ratio * (control_a + ratio * pos_a)));
        float s = 1 - t;
        return s * s * s * pos_a + 3 * s * s * t * control_a + 3 * s * t * t * control_b + t * t * t * pos_b;
    }

	void Start () {
        const int verts_per_point = 2;
        const int tris_per_point = verts_per_point * 3;
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        mesh = new Mesh();
        mf.mesh = mesh;

        Vector3 forward = (control_a - pos_a).normalized;
        Vector3 up = up_a;
        Vector3 right = Vector3.Cross(forward, up).normalized;
        Vector3 pos = pos_a, last_pos = pos_a;

        verts = new Vector3[n_points * verts_per_point];
        tris = new int[(n_points - 1) * tris_per_point];
        normals = new Vector3[n_points * verts_per_point];
        for (int i = 0; i < n_points; ++i)
        {
            // Create triangles if this is not the first pass through the loop
            if (i != 0)
            {
                int tri_base = (i - 1) * tris_per_point;
                int vert_base = (i - 1) * verts_per_point;
                tris[tri_base + 0] = vert_base;
                tris[tri_base + 1] = vert_base + 1;
                tris[tri_base + 2] = vert_base + 2;
                tris[tri_base + 3] = vert_base + 3;
                tris[tri_base + 4] = vert_base + 2;
                tris[tri_base + 5] = vert_base + 1;
            }
            float t = i / (float)(n_points - 1);
            pos = GetBezier(t);
            if (i != 0)
            {
                forward = (pos - last_pos).normalized;
                up = Vector3.Slerp(up_a, up_b, t);
                up = (up - forward * Vector3.Dot(forward, up)).normalized;
                right = Vector3.Cross(forward, up).normalized;
            }
            verts[i * verts_per_point + 0] = pos - right * (0.5f * width);
            verts[i * verts_per_point + 1] = pos + right * (0.5f * width);
            normals[i * verts_per_point + 0] = up;
            normals[i * verts_per_point + 1] = up;

            last_pos = pos;
        }
           
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.normals = normals;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
