using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class GameUI : UILayer {
    [SerializeField]
    private Text statusText;
    [SerializeField]
    private Button EndTurnButton;

    public event Action OnEndTurnClick;

    protected override void OnShow() {
        EndTurnButton.onClick.AddListener(OnEndTurnButtonClick);
    }

    public void SetStatusText(string text) {
        statusText.text = text;
    }

    private void OnEndTurnButtonClick() {
        OnEndTurnClick.InvokeSafe();
    }

    private void Start () {}
	
	private void Update () {}
}
