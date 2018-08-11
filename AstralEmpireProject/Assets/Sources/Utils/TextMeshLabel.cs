using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TextMeshLabel : MonoBehaviour
{
	[SerializeField] private string text = "";
	[SerializeField] private Font font;			//=Resources.Load("Font/euro");  //Arial-Black(eng)");
	[SerializeField] private Color color = new Color (0, 0, 0, 1);
	[SerializeField] private float size = 1f;
	[SerializeField] private TextAnchor anshor = TextAnchor.MiddleCenter;
	const float fontSizeFactor = 1f;
	public TextMesh labelMesh;											// создаваемые объекты
	public MeshRenderer meshRenderer;
	private bool visibility = true;
	
	public bool visible
	{
		get
		{
			return visibility;
		}
		set
		{
			if (visibility != value)
			{
				SetVisible (value);
			}
		}
	}

	private void SetVisible (bool val)
	{
		if (visibility != val)
		{
			visibility = val;
			meshRenderer.enabled = val;
		}
	}
	
	public static TextMeshLabel Create (Transform parent, Vector3 position, string text, float size = 1f)
	{
		GameObject labelObject = new GameObject ();
		labelObject.name = "label";
		labelObject.transform.parent = parent;
		labelObject.transform.localPosition = position;
		
		TextMeshLabel label = labelObject.AddComponent<TextMeshLabel> () as TextMeshLabel;
		label.SetSize (size);
		label.SetText (text);
		return label;
	}
	
	void Awake ()
	{
		font = Resources.Load ("Fonts/VenusRising") as Font;
		labelMesh = Utils.GetOrCreateComponent<TextMesh> (gameObject);
		labelMesh.font = font;
		labelMesh.text = text;
		labelMesh.characterSize = size;
		labelMesh.anchor = anshor;
		
		meshRenderer = Utils.GetOrCreateComponent<MeshRenderer> (gameObject);
		meshRenderer.material = new Material (labelMesh.font.material);
		SetColor (color);
	}
	
	public void SetSize (float labelSize)
	{
		size = labelSize;
		if (labelMesh != null)
		{
			labelMesh.characterSize = size * fontSizeFactor;
		}
	}
	
	public void SetAnchor (TextAnchor anchor)
	{
		labelMesh.anchor = anchor;
	}
	
	// установить текст
	public void SetText (string txt)
	{
		text = txt;
		if (labelMesh != null)
		{
			labelMesh.text = text;
		}
	}
	
	// задать цвет
	public void SetColor (Color color)
	{
		this.color = color;
		if (labelMesh != null)
		{
			labelMesh.GetComponent<Renderer> ().sharedMaterial.color = color;
		}
	}
}
