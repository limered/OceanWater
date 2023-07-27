using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace OceanWater;

public partial class OceanMesh : MeshInstance3D
{
	[Export] public Vector2 Size { get; set; } = new(100, 100);

	public override void _Ready()
	{
		var surfaceArray = new Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();
		var normals = new List<Vector3>();
		var indices = new List<int>();

		GenerateMesh(verts, uvs, normals, indices);

		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

		if (Mesh is ArrayMesh arrayMesh)
		{
			arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
			GD.Print("Mesh Loaded");
			GD.PrintRich(arrayMesh);
		}
		
		ResourceSaver.Save(Mesh, "res://Ocean.tres", ResourceSaver.SaverFlags.Compress);
	}

	private void GenerateMesh(
		ICollection<Vector3> verts, 
		ICollection<Vector2> uvs, 
		ICollection<Vector3> normals, 
		ICollection<int> indices)
	{
		var uvScale = new Vector2(1f / Size.X, 1f / Size.Y);
		
		for (var x = 0; x < (int)Size.X; x++)
		{
			for (var z = 0; z < (int)Size.Y; z++)
			{
				verts.Add(new Vector3(x, 0, z));
				uvs.Add(new Vector2(x, z) * uvScale);
				normals.Add(Vector3.Up);
				
				if (x < Size.X - 1 && z < Size.Y - 1)
				{
					var i = x * (int)Size.Y + z;
					indices.Add(i);
					indices.Add(i + (int)Size.Y);
					indices.Add(i + 1);
					
					indices.Add(i + 1);
					indices.Add(i + (int)Size.Y);
					indices.Add(i + (int)Size.Y + 1);
				}
			}
		}
	}

	public override void _Process(double delta)
	{
	}
}
