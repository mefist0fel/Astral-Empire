using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameUI : UILayer {
    [SerializeField]
    private Button OnEndTurnButton;

    protected override void OnShow() {
        OnEndTurnButton.onClick.AddListener(OnEndTurnClick);
    }

    private void OnEndTurnClick() {
        Debug.LogError("OnEndTurnClick");
    }

    private void Start () {}
	
	private void Update () {}
}
