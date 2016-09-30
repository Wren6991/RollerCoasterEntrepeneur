using UnityEngine;
using System.Collections;

public class TrackSegment : MonoBehaviour {
    public Vector3 pos_a, pos_b;
    // Bezier control points (in same coordinate system as start and end points)
    public Vector3 control_a, control_b;
    public Vector3 up_a, up_b;
    public float width = 1.0f, height = 0.5f;
    public int n_points = 20;

    const int vverts_per_point = 2;
    const int vtris_per_point = vverts_per_point * 6;
    Mesh vmesh;
    Vector3[] vverts, normals;
    int[] vtris;

    const int pverts_per_point = 4;
    const int ptris_per_point = pverts_per_point * 6;
    Mesh pmesh;
    Vector3[] pverts;
    int[] ptris;

    // Get track centreline at parametric coord t, based on the control and endpoints.
    Vector3 GetBezier(float t)  {
        if (t == 0)
            return pos_a;
        //float ratio = (1 - t) / t;
        //return t * t * t * (pos_b + ratio * (control_b + ratio * (control_a + ratio * pos_a)));
        float s = 1 - t;
        return s * s * s * pos_a + 3 * s * s * t * control_a + 3 * s * t * t * control_b + t * t * t * pos_b;
    }

    // Create a hollow tube of "size" verts per point, where "stride" is the total number of vertices per point.
    void TriangleLoop(int[] tris, int tris_offs, int vert_offs, int size, int stride)
    {
        for (int i = 0; i < size; ++i)
        {
            tris[tris_offs++] = vert_offs + i;
            tris[tris_offs++] = vert_offs + (i + 1) % size;
            tris[tris_offs++] = vert_offs + i + stride;
            tris[tris_offs++] = vert_offs + (i + 1) % size + stride;
            tris[tris_offs++] = vert_offs + i + stride;
            tris[tris_offs++] = vert_offs + (i + 1) % size;
        }
    }

    // Set up the index buffers (only done once) and then call UpdateMesh() to setup the verts
    void GenerateMesh()
    {
        for (int i = 0; i < n_points - 1; ++i)
        {
            TriangleLoop(vtris, i * vtris_per_point, i * vverts_per_point, 2, vverts_per_point);
            TriangleLoop(ptris, i * ptris_per_point, i * pverts_per_point, pverts_per_point, pverts_per_point);
        }
        UpdateMesh();
    }

    void UpdateMesh()
    {
        Vector3 forward = (control_a - pos_a).normalized;
        Vector3 up = up_a;
        Vector3 right = Vector3.Cross(forward, up).normalized;
        Vector3 pos = pos_a, last_pos = pos_a;

        for (int i = 0; i < n_points; ++i)
        {
            float t = i / (float)(n_points - 1);
            pos = GetBezier(t);
            if (i != 0)
            {
                forward = (pos - last_pos).normalized;
                up = Vector3.Slerp(up_a, up_b, t);
                up = (up - forward * Vector3.Dot(forward, up)).normalized;
                right = Vector3.Cross(forward, up).normalized;
            }
            vverts[i * vverts_per_point + 0] = pos - right * (0.5f * width);
            vverts[i * vverts_per_point + 1] = pos + right * (0.5f * width);
            normals[i * vverts_per_point + 0] = up;
            normals[i * vverts_per_point + 1] = up;

            pverts[i * pverts_per_point + 0] = pos - right * (0.5f * width) + up * (0.5f * height);
            pverts[i * pverts_per_point + 1] = pos + right * (0.5f * width) + up * (0.5f * height);
            pverts[i * pverts_per_point + 2] = pos + right * (0.5f * width) - up * (0.5f * height);
            pverts[i * pverts_per_point + 3] = pos - right * (0.5f * width) - up * (0.5f * height);
            last_pos = pos;
        }

        vmesh.vertices = vverts;
        vmesh.triangles = vtris;
        vmesh.normals = normals;

        pmesh.vertices = pverts;
        pmesh.triangles = ptris;
    }

	void Start () {
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        vmesh = new Mesh();
        mf.mesh = vmesh;

        vverts = new Vector3[n_points * vverts_per_point];
        vtris = new int[(n_points - 1) * vtris_per_point];
        normals = new Vector3[n_points * vverts_per_point];

        pmesh = new Mesh();

        pverts = new Vector3[n_points * pverts_per_point];
        ptris = new int[(n_points - 1) * ptris_per_point];
        GenerateMesh();

        MeshCollider mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = pmesh;

    }

    // Update is called once per frame
    void Update () {
	
	}
}
