using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMeshes : MonoBehaviour
{
    const float TAU = Mathf.PI * 2.0f;
    
    [Header("Materials")] 
    public Material MaterialVertColor;
    public Material MaterialPlain;
    public Material MaterialSecond;
    
    // For layout
    private List<Transform> meshes = new();
    private Vector3 _layoutPos;
    
    void Start()
    {
        _layoutPos = new Vector3(-1.0f, 0.0f, 1.0f);
        
        SimpleMeshBuilder builder = new SimpleMeshBuilder();

        // Simple "Hello World" Mesh
        GameObject objTriangle = BuildHelloMesh(builder);
        LayoutAndLabel( objTriangle, "Hello Triangle");
        
        // More complicated spiral example
        GameObject objSpiral = BuildSpiralMesh(builder);
        objSpiral.transform.rotation = Quaternion.AngleAxis( 50.0f, Vector3.right );
        LayoutAndLabel( objSpiral, "Spiral");
        
        // Example with Multiple Materials
        GameObject objChecks = BuildMultiMtlMesh(builder);
        objChecks.transform.rotation = Quaternion.AngleAxis( 20.0f, Vector3.right );
        objChecks.transform.localScale = Vector3.one * 0.7f;
        LayoutAndLabel( objChecks, "Multi-Material");
    }

    // ===[ Simple Example ]===========================================
    // This is a simple example, it creates a mesh with a single triangle
    // with UV and vertex color data. 
    GameObject BuildHelloMesh( SimpleMeshBuilder builder )
    {
        builder.Reset();

        float yy = 0.75f;
        int a = builder.pushVert(
            new Vector3(2.1f, 0.0f, -2.1f + yy),
            Vector3.up,
            new Vector2(0.0f, 0.0f),
            new Color32(255, 10, 10, 255));
        
        int b = builder.pushVert(
            new Vector3(-2.1f, 0.0f,  -2.1f + yy),
            Vector3.up,
            new Vector2(0.0f, 1.0f),
            new Color32(10, 10, 255, 255));
        
        int c = builder.pushVert(
            new Vector3( 0.0f, 0.0f,  1.5f + yy),
            Vector3.up,
            new Vector2(1.0f, 1.0f),
            new Color32(10, 255, 10, 255));

        builder.pushTri(a, b, c );

        // Create a new GameObject and assign the mesh
        GameObject objMesh1 = new GameObject();
        MeshRenderer rndr = objMesh1.AddComponent<MeshRenderer>();
        MeshFilter mf = objMesh1.AddComponent<MeshFilter>();
        
        mf.mesh = builder.FinalizeMesh(mf.mesh, "HelloMesh");
        rndr.material = MaterialVertColor;

        return objMesh1;
    }

    // ===[ Spiral Example ]===========================================
    // A more complicated example, this builds a spiral with end caps. 
    GameObject BuildSpiralMesh(SimpleMeshBuilder builder)
    {
        builder.Reset();

        
        float height = 1.5f;
        float revolutions = 3.0f;
        
        float seg = 0.02f;
        float innerRadius = 0.4f;
        int innerRings = 16;
        int lastRowIndex = -1;
        Quaternion spiralRot = Quaternion.identity;
        Vector3 center = Vector3.zero;
        for (float t = 0.0f; t <= revolutions; t += seg)
        {
            float y = Mathf.Lerp(-height, height, t / revolutions);
            
            spiralRot = Quaternion.AngleAxis( t * 360.0f, Vector3.up );
            center = spiralRot * new Vector3( 1.0f, y, 0.0f );
                
            // Inner radius
            int ringStartIndex = builder.VertexCount;
            for (int i = 0; i < innerRings; i++)
            {
                float t2 = (float) i / (float) innerRings;
                float a2 = t2 * TAU;
                Vector3 dir = new Vector3(Mathf.Cos(a2), Mathf.Sin(a2), 0.0f ) * innerRadius;
                dir = spiralRot * dir;

                builder.pushVert(center + dir, 
                                    Vector3.up, // Ignore normal since we'll let unity calculate them
                                    new Vector2( t2, t / revolutions));
            }
            
            // Now create a row of quads
            if (lastRowIndex >= 0)
            {
                for (int i = 0; i < innerRings; i++)
                {
                    builder.pushQuad( 
                        lastRowIndex + (i + 1) % innerRings, lastRowIndex + i,
                            ringStartIndex + (i + 1) % innerRings, ringStartIndex + i );
                }
            }

            lastRowIndex = ringStartIndex;
        }
        
        
        // Make the end caps
        Quaternion spiralRot2 = Quaternion.AngleAxis( 0 * 360.0f, Vector3.up );
        Vector3 center2 = spiralRot2 * new Vector3( 1.0f, -height, 0.0f );
        MakeEndCap( builder, innerRings, innerRadius, center2, Quaternion.identity, 0, true );
        
        MakeEndCap( builder, innerRings, innerRadius, center, spiralRot, lastRowIndex, false );
        
        // Create a new GameObject and assign the mesh
        GameObject objSpiral = new GameObject();
        MeshRenderer rndr = objSpiral.AddComponent<MeshRenderer>();
        MeshFilter mf = objSpiral.AddComponent<MeshFilter>();
        
        mf.mesh = builder.FinalizeMesh(mf.mesh, "Spiral");
        rndr.material = MaterialPlain;
        
        return objSpiral;
    }
    
    // End caps
    void MakeEndCap( SimpleMeshBuilder builder, int innerRings, float innerRadius, Vector3 center, Quaternion rot, int prevRowIndex, bool flipped )
    {
        Vector3 fwd = flipped ? Vector3.forward : Vector3.back;
        int capRings = 3;
        for (int capRing = 0; capRing <= capRings; capRing++)
        {
            int rowStartIndex = builder.VertexCount;
            float capT = (float)(capRing + 1) / (float)(capRings + 2);
            float capAng = capT * (Mathf.PI / 2.0f);
            
            for (int i = 0; i < innerRings; i++)
            {
                float t2 = (float) i / (float) innerRings;
                float a2 = t2 * TAU;
                Vector3 dir = new Vector3(Mathf.Cos(a2), Mathf.Sin(a2), 0.0f);
                
                Vector3 capDir = rot * (Mathf.Sin(capAng) * fwd) +
                                 rot * (Mathf.Cos(capAng) * dir);

                Vector3 capPnt = capDir * innerRadius;

                builder.pushVert(center + capPnt, Vector3.up, new Vector2(0.5f, 0.5f) );
            }
            
            for (int i = 0; i < innerRings; i++)
            {
                int A = prevRowIndex + (i + 1) % innerRings;
                int B = prevRowIndex + i;
                int C = rowStartIndex + (i + 1) % innerRings;
                int D = rowStartIndex + i;
                if (flipped)
                {
                    builder.pushQuad(C, D, A, B );
                }
                else
                {
                    builder.pushQuad(A, B, C, D);
                }
            }

            prevRowIndex = rowStartIndex;
        }
        
        // Close the center
        Vector3 centerPnt = fwd * innerRadius;
        centerPnt = rot * centerPnt;
        int centerNdx = builder.pushVert(center + centerPnt, Vector3.up, new Vector2(0.5f, 0.5f) );
        
        for (int i = 0; i < innerRings; i++)
        {
            
            int A = prevRowIndex + (i + 1) % innerRings;
            int B = prevRowIndex + i;
            int C = centerNdx;
            if (flipped)
            {
                builder.pushTri(C, B, A );
            }
            else
            {
                builder.pushTri(A, B, C );
            }
        }
    }



    void LayoutAndLabel(GameObject obj, string label)
    {
        obj.name = label;

        // Arrange the meshes in two rows
        float layoutSz = 4.0f;
        obj.transform.position = _layoutPos * layoutSz;
        if (_layoutPos.x < 2.5f)
        {
            _layoutPos.x += 1.0f;
        }
        else
        {
            _layoutPos.x = -1.0f;
            _layoutPos.z = _layoutPos.z - 2.0f;
        }

        // Put a label below the objects
        GameObject objLabel = new GameObject();
        objLabel.name = label + " Label";
        objLabel.transform.position = obj.transform.position + Vector3.forward * -2.5f;
        objLabel.transform.rotation = Quaternion.Euler(90.0f, 0f, 0f);
        TextMesh textMesh = objLabel.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.fontSize = 48;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.UpperCenter;

        // Add to the list of meshes so we can rotate them
        meshes.Add(obj.transform);
    }
    
    // ===[ MultiMaterial Example ]===========================================
    //  An checkerboard with multiple submeshes with different materials 
    GameObject BuildMultiMtlMesh(SimpleMeshBuilder builder)
    {
        builder.Reset();
        
        int checks = 6;
        float ch = (float) checks / 2.0f;
        for (int j = 0; j < checks + 1; j++)
        {
            for (int i = 0; i < checks + 1; i++)
            {
                builder.pushVert(new Vector3((float) i - ch, 0.0f, - ((float) j - ch) ),
                    Vector3.up,
                    new Vector2((float) i / (float) checks, (float) i / (float) checks)
                );
            }
        }

        for (int j = 0; j < checks; j++)
        {
            for (int i = 0; i < checks; i++)
            {
                int ndx = (j * (checks + 1)) + i;

                // Choose which submesh based on which check were on
                int submesh = 0; // first material
                if ((i & 1) == (j & 1))
                {
                    submesh = 1; // second material
                }
                builder.pushQuad( ndx, ndx+1,
                        ndx + checks+1, ndx + checks+2, 
                        submesh );
            }
        }
        
        GameObject objChecks = new GameObject();
        MeshRenderer rndr = objChecks.AddComponent<MeshRenderer>();
        MeshFilter mf = objChecks.AddComponent<MeshFilter>();
        
        mf.mesh = builder.FinalizeMesh(mf.mesh, "Checks");
        Material[] mtls = new Material[2];
        mtls[0] = MaterialPlain;
        mtls[1] = MaterialSecond;
        rndr.materials = mtls;
        
        return objChecks;
    }

    private void Update()
    {
        Quaternion rot = Quaternion.AngleAxis(30.0f * Time.deltaTime, Vector3.up);
        foreach (Transform t in meshes)
        {
            t.rotation = t.rotation * rot;
        }
    }
}
