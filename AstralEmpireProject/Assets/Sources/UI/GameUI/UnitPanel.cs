using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UI element for player unit presentation and base actions
public sealed class UnitPanel : MonoBehaviour {
    [SerializeField]
    private Text unitName; // Set from editor
    [SerializeField]
    private Text lifeCount; // Set from editor
    [SerializeField]
    private Button cancelButton; // Set from editor

    private Action OnCancelButtonClickCallback;
    public event Action OnCancelUnitClick;

    public void Show(Unit unit = null) {
        if (unit == null) {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        unitName.text = unit.Id;
        lifeCount.text = string.Format("{0}/{1}", unit.HitPoints, unit.MaxHitPoints);
    }

	private void Start () {
        cancelButton.onClick.AddListener(OnCancelButtonClickHandler);
	}

    private void OnCancelButtonClickHandler() {
        OnCancelUnitClick.InvokeSafe();
    }
}
