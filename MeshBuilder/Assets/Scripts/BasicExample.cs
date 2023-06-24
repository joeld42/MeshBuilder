using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SimpleMeshBuilder builder = new SimpleMeshBuilder();

        GameObject objMesh1 = new GameObject();
        objMesh1.AddComponent<MeshRenderer>()
    }

    Mesh BuildHelloMesh( SimpleMeshBuilder builder )
    {
        builder.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
