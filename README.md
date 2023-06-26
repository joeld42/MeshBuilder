# MeshBuilder
Simple Mesh Building helper for Unity

## Introduction

I've rewritten some version of this script for a handful of projects. It's not very performant or complicated, but it's a good starting point. I figured rather than write it again for the next project I would just make an open source one and then I can just use that going forward, and maybe other people will find it useful.

## Basic Usage

To use this, just copy the `MeshBuilder.cs` class into your project. Create a meshbuilder in `Start` or similar. Then you can add vertex data like this:

```js
int index = builder.pushVert( pos, nrm, st );
```

Then you can add triangles or quads like this:

```js
int triIndex = builder.pushTri( indexA, indexB, indexC );
```
You can also pass in a `submesh` index to `pushTri` or `pushQuad` if you have multiple materials.

You can call these in any order, whatever makes the most sense for your generation code. All it really does is just assemble a list of verts and triangle.

When you're done, call `FinalizeMesh` and it will assemble the vert and tri arrays and assign them to the mesh. This isn't very smart about it, if you need to update the mesh every frame or something you're better off managing the data yourself, but most of the time I find I'm just building meshes up front.

## Hello Triangle

Here's a complete example, that creates a simple triangle:

```js
        SimpleMeshBuilder builder = new SimpleMeshBuilder();
        

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
```

## More Examples

There's a few basic examples included:

![Example Meshes](/screenshots/examples.png)

* __Hello Triangle__ -- this the triangle shown in the code snippet above. Note that this uses a slightly custom shader to show the vertex colors because Unity's default Lit shader doesn't support them.
* __Spiral__ -- A more real-world example of creating a complex shape, in this case a spiral with capped ends.
* __Multi-Material__ -- A simple example of using submeshes to create an object with multiple materials. 

## Questions

Feel free to ping me at `joeld42@gmail.com` if you have any questions.