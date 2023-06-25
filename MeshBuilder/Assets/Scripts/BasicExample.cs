using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicExample : MonoBehaviour
{

    [Header("Materials")] 
    public Material MaterialVertColor;
    public Material MaterialPlain;
    public Material MaterialSecond;
    
    // For layout
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
        LayoutAndLabel( objSpiral, "Spiral");
    }

    // ===[ Simple Example ]===========================================
    // This is a simple example, it creates a mesh with a single triangle
    // with UV and vertex color data. 
    GameObject BuildHelloMesh( SimpleMeshBuilder builder )
    {
        builder.Reset();
        
        int a = builder.pushVert(
            new Vector3(1.0f, 0.0f, -1.0f),
            Vector3.up,
            new Vector2(0.0f, 0.0f),
            new Color32(255, 10, 10, 255));
        
        int b = builder.pushVert(
            new Vector3(-1.0f, 0.0f,  -1.0f),
            Vector3.up,
            new Vector2(0.0f, 1.0f),
            new Color32(10, 10, 255, 255));
        
        int c = builder.pushVert(
            new Vector3( 0.0f, 0.0f,  1.5f),
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

    GameObject BuildSpiralMesh(SimpleMeshBuilder builder)
    {
        builder.Reset();

        float Tau = Mathf.PI * 2.0f;
        float height = 1.5f;
        float revolutions = 3.0f;
        float seg = 0.02f;
        float innerRadius = 0.4f;
        int innerRings = 16;
        int lastRowIndex = -1;
        for (float t = 0.0f; t <= revolutions; t += seg)
        {
            float y = Mathf.Lerp(-height, height, t / revolutions);
            
            Quaternion spiralRot = Quaternion.AngleAxis( t * 360.0f, Vector3.up );
            Vector3 center = spiralRot * new Vector3( 1.0f, y, 0.0f );
                
            // Inner radius
            int ringStartIndex = builder.VertexCount;
            for (int i = 0; i < innerRings; i++)
            {
                float t2 = (float) i / (float) innerRings;
                float a2 = t2 * Tau;
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
        
        // Create a new GameObject and assign the mesh
        GameObject objSpiral = new GameObject();
        MeshRenderer rndr = objSpiral.AddComponent<MeshRenderer>();
        MeshFilter mf = objSpiral.AddComponent<MeshFilter>();
        
        mf.mesh = builder.FinalizeMesh(mf.mesh, "Spiral");
        rndr.material = MaterialPlain;

        return objSpiral;
    }



    void LayoutAndLabel(GameObject obj, string label)
    {
        obj.name = label;
        
        GameObject objLabel = new GameObject();
        objLabel.name = label + " Label";
        objLabel.transform.SetParent( obj.transform );
        objLabel.transform.localPosition = Vector3.forward * -1.2f;
        objLabel.transform.rotation = Quaternion.Euler( 90.0f, 0f, 0f );
        TextMesh textMesh = objLabel.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.fontSize = 48;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.UpperCenter;

        // Arrange the meshes in two rows
        float layoutSz = 3.0f;
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


    }
}
