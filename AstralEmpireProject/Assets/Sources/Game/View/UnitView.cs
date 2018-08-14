using UnityEngine;
using Model;
using System;
using Random = UnityEngine.Random;

public sealed class UnitView : MonoBehaviour {
    [SerializeField]
    private Unit unit = null;

    // [SerializeField]
    // private BaseHullView hullView; // Set from editor

   // [SerializeField]
   // private BaseWeaponView weaponView; // Set from editor

    [SerializeField]
    private TextMesh lifeLabel = null; // Set from editor

    public void Init(Unit unitModel, Vector3 position) {
        unit = unitModel;
        transform.localPosition = position;
        transform.localEulerAngles = new Vector3(0, 180f * unit.Faction.SideId, 0); // hack for rotation
        SetUnitColor(unit.Faction.BaseColor, unit.Faction.FactionColor);
    }

    public Vector3 GetHitPoint() {
        return transform.position + new Vector3(0, 0.2f, 0);
    }

    public void MoveTo(Vector3[] path, Action endMoveAction = null) {
     //   hullView.Move(path, endMoveAction);
    }

    public void FireTo(UnitView enemy, Action endFireAction = null) {
     //   hullView.Aim(enemy.transform.position, () => {
     //       weaponView.Fire(enemy, endFireAction);
     //   });
    }


    void UpdateHitPointsLabel() {
        if (lifeLabel != null) {
            lifeLabel.text = unit.HitPoints.ToString();
        }
    }

    [ContextMenu("Show death animation")]
    public void Death() {
        if (this == null)
            return;
        SetUnitMaterial(Resources.Load<Material>("Materials/Dead"));
        if (lifeLabel != null) {
            Timer.Add(0.8f, (anim) => {
                if (lifeLabel != null) {
                    lifeLabel.color = new Color(1, 1, 1, 1f - anim);
                }
            });
        }
        // Falling
        var currentPosition = transform.position;
        var needPosition = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), -Random.Range(0.4f, 1.0f), Random.Range(-0.3f, 0.3f));
        var currentRotation = transform.rotation;
        var needRotation = transform.rotation * Quaternion.AngleAxis(Random.Range(30f, 60f), Random.onUnitSphere.normalized);
        var fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        Timer.Add(-0.1f, 1.6f, (anim) => {
            if (this == null)
                return;
            if (transform == null)
                return;
            transform.position = Vector3.Lerp(currentPosition, needPosition, fallCurve.Evaluate(anim));
            transform.rotation = Quaternion.Lerp(currentRotation, needRotation, fallCurve.Evaluate(anim));
        });
    }

    void SetUnitMaterial(Material material) {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++) {
            TextMesh mesh = renderers[i].GetComponent<TextMesh>();
            if (mesh == null) {
                renderers[i].sharedMaterial = material;
            }
        }
    }

    void SetUnitColor(Color baseColor, Color factionColor) {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++) {
            TextMesh mesh = renderers[i].GetComponent<TextMesh>();
            if (mesh == null) {
                renderers[i].material.color = factionColor; // TODO - optimize - save materials with similar color
                renderers[i].material.SetColor("_Color", baseColor);
                renderers[i].material.SetColor("_BaseColor", baseColor);
                renderers[i].material.SetColor("_FactionColor", factionColor);
            }
        }
    }

    void Start() {
        UpdateHitPointsLabel();
    }

    public void OnStartTurn() {
    }

    public void OnHit() {
        UpdateHitPointsLabel();
    }

#if UNITY_EDITOR
    BesieCurve curve = null;
    void OnDrawGizmos() {
        if (curve != null) {
            Vector3 prevPoint = curve.PathPoints[0];
            foreach (var point in curve.PathPoints) {
                Gizmos.DrawWireCube(point, Vector3.one * 0.1f);
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }
    }
#endif
}
