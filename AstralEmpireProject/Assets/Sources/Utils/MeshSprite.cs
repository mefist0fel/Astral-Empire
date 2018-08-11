using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class MeshSprite : MonoBehaviour {
					
	[SerializeField] private Material material;
								
	[SerializeField] private Vector2 pivot = new Vector2(0.5f, 0.5f);		
	[SerializeField] private Vector2 spriteScale = new Vector2(1, 1);
	[SerializeField] Color color = Color.white;
	[SerializeField] private Rect UV = new Rect(0, 0, 1, 1);
	
	protected MeshFilter meshFilter;						
	protected MeshRenderer meshRenderer;		

	bool created = false;					
	
	protected bool visibility = true;
	
	public bool visible {
        get {
            return visibility;
        }
        set {
			if (visibility != value) {
				SetVisible(value);
			}
        }
    }
	
	public Vector2 scale {
        get {
            return spriteScale;
        }
        set {
			if (spriteScale != value) {
				SetScale(value);
			}
        }
    }
	
	public Material spriteAnimation {
        get {
            return material;
        }
        set {
			if (material != value) {
				meshRenderer.material = material;
			}
        }
    }
	
	private void SetVisible(bool value){
		if(visibility != value && meshRenderer != null){
			visibility = value;								
			meshRenderer.enabled = value;
		}
	}

	public static MeshSprite Create(Transform parent, Vector3 position, Vector2 size, Material material, bool mirrorHorizontal = false, bool mirrorVertical = false){		
		GameObject spriteObject = new GameObject();
		spriteObject.name = "sprite";
		if (parent != null) {
			spriteObject.transform.parent = parent;
			spriteObject.layer = parent.gameObject.layer;
		}
		spriteObject.transform.localPosition = position;
		
		MeshSprite sprite = spriteObject.AddComponent<MeshSprite>();
		sprite.UV = new Rect(0, 0, 1, 1);
		if (mirrorHorizontal)
		{
			sprite.UV.x = 1;
			sprite.UV.width = -1;
		}
		if (mirrorVertical)
		{
			sprite.UV.y = 1;
			sprite.UV.height = -1;
		}
		sprite.spriteScale = size;
		sprite.material = material;
		if (material.HasProperty("_color"))
		{
			sprite.color = material.color;
		}
		return sprite;
	}
	
	public static MeshSprite Create(Transform parent, Vector2 size, Material material, bool mirrorHorizontal = false, bool mirrorVertical = false){
		return Create(parent, Vector3.zero, size, material, mirrorHorizontal, mirrorVertical);
	}
	
	Mesh CreateMesh(Vector2 scale, Vector2 pivot, Rect textureCoords){
		if (scale.x < 0){
			textureCoords.x += textureCoords.width;
			textureCoords.width *= -1;
			scale.x *= -1;
		}
		if (scale.y < 0){
			textureCoords.y += textureCoords.height;
			textureCoords.height *= -1;
			scale.y *= -1;
		}
		Vector3[] vertices = new Vector3[] {
			new Vector3(0, 0, 0),          
			new Vector3(0, scale.y, 0),     
			new Vector3(scale.x, scale.y, 0),
			new Vector3(scale.x, 0, 0) 
		};
		
		Vector3 shift = Vector2.Scale(pivot, scale);
		for (int i = 0; i < vertices.Length; i++) {
		  vertices[i] -= shift;
		}		
		var uv = new[] {
			new Vector2(textureCoords.xMin, textureCoords.yMin),
			new Vector2(textureCoords.xMin, textureCoords.yMax),
			new Vector2(textureCoords.xMax, textureCoords.yMax),
			new Vector2(textureCoords.xMax, textureCoords.yMin)
		};
		
		var triangles = new[] {
			0, 1, 2,
			0, 2, 3
		};
		return new Mesh { vertices = vertices, uv = uv, triangles = triangles };
	}
	
	void Awake () {
		meshRenderer = gameObject.GetComponent<MeshRenderer>();
		if (meshRenderer == null)
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshFilter = gameObject.GetComponent<MeshFilter>();
		if (meshFilter == null)
			meshFilter = gameObject.AddComponent<MeshFilter>();
	}
	void Start(){
		if (material != null){
			material.color = color;
			meshRenderer.material = material;
		}
		meshFilter.mesh = CreateMesh(spriteScale, pivot, UV);
		created = true;
	}
	
	public void SetPivot(Vector2 anchor){
		pivot = anchor;
		meshFilter.mesh = CreateMesh(spriteScale, pivot, UV);
	}
	
	public void SetPivot(float x, float y){
		pivot = new Vector2(x, y);
		meshFilter.mesh = CreateMesh(spriteScale, pivot, UV);
	}
	
	public void SetScale(Vector2 scaleSize){
		spriteScale = scaleSize;
		meshFilter.mesh = CreateMesh(spriteScale, pivot, UV);
	}
	
	public void SetColor(Color newColor) {
		color = newColor;
		if (created) {
			meshRenderer.sharedMaterial.color = color;
			meshRenderer.sharedMaterial.SetColor("_TintColor", color);
		}
	}
	
	#if UNITY_EDITOR		
	Vector2 currentPivor = Vector2.one * 0.5f;
	Vector2 currentSpriteScale = Vector2.one;
	Material currentMaterial = null;
	Color currentColor = Color.white;
	
    void OnDrawGizmos() {
		bool needChange = false;
		if (currentMaterial != material){
			currentMaterial = material;
			meshRenderer.material = material;
		}
		if (currentColor != color && material != null){
			currentColor = color;
			SetColor(color);
		}
		if (currentPivor != pivot){
			currentPivor = pivot;
			needChange = true;
		}
		if (currentSpriteScale != spriteScale){
			currentSpriteScale = spriteScale;
			needChange = true;
		}
		if (needChange){
			meshFilter.mesh = CreateMesh(spriteScale, pivot, UV);
			Resources.UnloadUnusedAssets();
		}
    }	
	#endif
}
