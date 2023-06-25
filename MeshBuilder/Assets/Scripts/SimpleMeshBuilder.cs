using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMeshBuilder
{
    
    public struct BuildTri
    {
        public int submesh;
        public int a;
        public int b;
        public int c;
    }

    public struct BuildVert
    {
        public Vector3 pos;
        public Vector3 nrm;
        public Vector2 st;
        public Color32 color;
    }

    private const int kMaxVerts = 63000;
    private static Color32 defaultVertColor = new Color32(255, 255, 255, 255);

    private List<BuildTri> _buildTris = new();
    private List<BuildVert> _buildVerts = new();
    private int _maxSubmesh = 0;

    // Accessors
    public int VertexCount
    {
        get => _buildVerts.Count;
    }

    public int TriangleCount
    {
        get => _buildTris.Count;
    }

    public bool IsFull
    {
        get { return _buildVerts.Count >= kMaxVerts; }
    }

    public int pushVert(Vector3 pos, Vector3 nrm, Vector2 st)
    {
        return pushVert(pos, nrm, st, defaultVertColor);
    }

    public int pushVert(Vector3 pos, Vector3 nrm, Vector2 st, Color32 col)
    {
        BuildVert bv = new BuildVert();
        bv.pos = pos;
        bv.nrm = nrm;
        bv.st = st;
        bv.color = col;
        return pushBuildVert(bv);
    }

    public int pushBuildVert(BuildVert v)
    {
        // fail silently here, you may want to assert or return a sentinal value
        if (IsFull)
        {
            return 0;
        }

        int addAtIndex = _buildVerts.Count;
        _buildVerts.Add(v);

        return addAtIndex;
    }

    /// Returns the index of the added triangle
    public int pushTri(int ndxA, int ndxB, int ndxC, int submesh = 0)
    {
        int addTriIndex = _buildTris.Count;
        BuildTri tri = new BuildTri();
        tri.a = ndxA;
        tri.b = ndxB;
        tri.c = ndxC;
        tri.submesh = submesh;
        if (submesh > _maxSubmesh)
        {
            _maxSubmesh = submesh;
        }
        _buildTris.Add( tri );

        return addTriIndex;
    }

    /// Returns the index of the first added triangle of the quad.
    public int pushQuad(int ndxA, int ndxB, int ndxC, int ndxD, int submesh = 0)
    {
        pushTri(ndxA, ndxB, ndxC, submesh);
        return pushTri(ndxC, ndxB, ndxD, submesh );
    }

    public void Reset()
    {
        _buildTris.Clear();
        _buildVerts.Clear();
        _maxSubmesh = 0;
    }

    public Mesh FinalizeMesh(Mesh mesh, string meshName, bool recalcNormals = true )
    {
        mesh.name = meshName;

        Vector3[] verts = new Vector3[_buildVerts.Count];
        Vector3[] nrms = new Vector3[_buildVerts.Count];
        Vector2[] texcoords = new Vector2[_buildVerts.Count];
        Color32[] colors = new Color32[_buildVerts.Count];
        
        for (int i = 0; i < _buildVerts.Count; i++)
        {
            verts[i] = _buildVerts[i].pos;
            nrms[i] = _buildVerts[i].nrm;
            texcoords[i] = _buildVerts[i].st;
            colors[i] = _buildVerts[i].color;
        }
        
        // Assign the mesh data
        mesh.Clear();
        mesh.vertices = verts;
        mesh.normals = nrms;
        mesh.uv = texcoords;
        mesh.colors32 = colors;
        
        int[] triCountForSubmesh = new int[_maxSubmesh + 1];
        
        // Count how many tris are in each submesh
        int submeshTriCount = 0;
        for (int i = 0; i < _buildTris.Count; i++)
        {
            triCountForSubmesh[_buildTris[i].submesh] += 1;
        }
        
        // Copy in the submesh triangles
        mesh.subMeshCount = _maxSubmesh + 1;
        for (int i = 0; i <= _maxSubmesh; i++)
        {
            int subndx = 0;
            int[] submeshTris = new int[triCountForSubmesh[i] * 3];
            
            for (int j = 0; j < _buildTris.Count; j++)
            {
                if (_buildTris[j].submesh == i)
                {
                    BuildTri tri = _buildTris[j];
                    submeshTris[subndx * 3 + 0] = tri.a;
                    submeshTris[subndx * 3 + 1] = tri.b;
                    submeshTris[subndx * 3 + 2] = tri.c;

                    subndx++;
                }
            }
            
            mesh.SetTriangles( submeshTris, i );
        }

        if (recalcNormals)
        {
            mesh.RecalculateNormals();
        }

        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        
        return mesh;
    }
}    
