using System;
using UnityEngine;
using System.Collections.Generic;

public sealed class MeshGenerator {
	string name;
	List<Vector3> vertices = new List<Vector3> ();
	List<Vector3> normals = new List<Vector3> ();
	List<Vector2> textureCoords = new List<Vector2> ();
	List<int> faces = new List<int> ();
	int faceCount = 0;

	public MeshGenerator(string meshName = "") {
		name = meshName;
	}

	public void AddQuad( Vector3 vertexTL, Vector3 vertexTR, Vector3 vertexBR, Vector3 vertexBL) {
		var normal = (
			Utils.GetNormal (vertexTL, vertexTR, vertexBR) * 0.5f +
			Utils.GetNormal (vertexTL, vertexBR, vertexBL) * 0.5f
		).normalized;
		AddQuad (vertexTL, vertexTR, vertexBR, vertexBL, normal);
	}

	public void AddQuad( Vector3 vertexTL, Vector3 vertexTR, Vector3 vertexBR, Vector3 vertexBL, Vector3 normal ) {
		AddQuad (vertexTL, vertexTR, vertexBR, vertexBL, normal, normal, normal, normal);
	}

	public void AddQuad(
		Vector3 vertexTL, Vector3 vertexTR, Vector3 vertexBR, Vector3 vertexBL,
		Vector3 normalTL, Vector3 normalTR, Vector3 normalBR, Vector3 normalBL
	) {
		vertices.Add (vertexTL);
		vertices.Add (vertexTR);
		vertices.Add (vertexBR);
		vertices.Add (vertexBL);
		textureCoords.Add (new Vector2(0, 0));
		textureCoords.Add (new Vector2(0, 1));
		textureCoords.Add (new Vector2(1, 1));
		textureCoords.Add (new Vector2(1, 0));
		normals.Add (normalTL);
		normals.Add (normalTR);
		normals.Add (normalBR);
		normals.Add (normalBL);
		faces.Add (faceCount + 0);
		faces.Add (faceCount + 1);
		faces.Add (faceCount + 2);
		faces.Add (faceCount + 0);
		faces.Add (faceCount + 2);
		faces.Add (faceCount + 3);
		faceCount += 4;
	}

	public void AddTriangle(
		Vector3 vertexA,
		Vector3 vertexB,
		Vector3 vertexC
	) {
		var normal = Utils.GetNormal (vertexA, vertexB, vertexC).normalized;
		AddTriangle (vertexA, vertexB, vertexC, normal, normal, normal);
	}

	public void AddTriangle(
		Vector3 vertexA,
		Vector3 vertexB,
		Vector3 vertexC,
		Vector3 normalA,
		Vector3 normalB,
		Vector3 normalC
	) {
		vertices.Add (vertexA);
		vertices.Add (vertexB);
		vertices.Add (vertexC);
		textureCoords.Add (new Vector2(0, 0));
		textureCoords.Add (new Vector2(0, 1));
		textureCoords.Add (new Vector2(1, 1));
		normals.Add (normalA);
		normals.Add (normalB);
		normals.Add (normalC);
		faces.Add (faceCount + 0);
		faces.Add (faceCount + 1);
		faces.Add (faceCount + 2);
		faceCount += 3;
	}

	public Mesh Generate() {
		return new Mesh() {
			name = name,
			vertices = vertices.ToArray(),
			normals = normals.ToArray(),
			triangles = faces.ToArray()
		};
	}
}