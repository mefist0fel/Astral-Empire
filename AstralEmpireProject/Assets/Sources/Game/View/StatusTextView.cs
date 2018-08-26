using UnityEngine;

public sealed class StatusTextView : MonoBehaviour {
    [SerializeField]
    private TextMesh statusText; // Set from editor
    [SerializeField]
    private float fullTime = 1.5f;

    private Vector3 startPosition;
    private Vector3 flyVector;
    private float startAlpha = 1f;
    private float timer = 0;

    public static StatusTextView Create(string text, Color color, Vector3 position) {
        var textView = Instantiate(Resources.Load<StatusTextView>("Prefabs/status_text"));
        textView.Init(text, color, position, Vector3.up);
        return textView;
    }

    public void Init(string text, Color color, Vector3 position, Vector3 moveVector) {
        startPosition = position;
        transform.position = position;
        flyVector = moveVector;
        statusText.text = text;
        startAlpha = color.a;
        statusText.color = color;
        timer = fullTime;
    }

    private void Update() {
        timer -= Time.deltaTime;
        float anim = timer / fullTime;
        transform.position = startPosition + flyVector * (1f - anim);
        statusText.color = new Color(statusText.color.r, statusText.color.g, statusText.color.b, startAlpha * anim * 2f);
        if (timer <= 0)
            Destroy(gameObject);
    }
}
