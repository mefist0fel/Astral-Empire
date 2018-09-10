using UnityEngine;
using UnityEngine.UI;
using System;
using Model;

public sealed class GameUI : UILayer {
    [SerializeField]
    private Text statusText;
    [SerializeField]
    private Button endTurnButton;
    [SerializeField]
    private UnitPanel unitPanel;

    public event Action OnEndTurnClick;
    public event Action OnCancelUnitClick;

    protected override void OnShow() {
        ShowUnit();
    }

    public void ShowUnit(Unit unit = null) {
        if (unit == null) {
            unitPanel.Show(null);
            endTurnButton.SetActive(true);
            return;
        }
        unitPanel.Show(unit);
        endTurnButton.SetActive(false);
    }

    public void SetStatusText(string text) {
        statusText.text = text;
    }

    private void OnEndTurnButtonClick() {
        OnEndTurnClick.InvokeSafe();
    }

    private void Start () {
        unitPanel.OnCancelUnitClick += OnCancelUnitClickHandler;
        endTurnButton.onClick.AddListener(OnEndTurnButtonClick);
    }

    private void OnCancelUnitClickHandler() {
        OnCancelUnitClick.InvokeSafe();
    }
}
