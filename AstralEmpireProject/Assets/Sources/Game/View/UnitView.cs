﻿using UnityEngine;
using Model;
using System;
using Random = UnityEngine.Random;

namespace View {
    public sealed class UnitView : MonoBehaviour, ITargetView {
        [SerializeField]
        private float moveSpeed = 1f;
        [SerializeField]
        private Unit unit = null;
        [SerializeField]
        private Material deathMaterial = null;
        // [SerializeField]
        // private Renderer[] coloredRenderers = null;

        [SerializeField]
        private Vector3 hitOffset = new Vector3(0, 0.2f, 0); // Set from editor

        // [SerializeField]
        // private BaseWeaponView weaponView; // Set from editor

        [SerializeField]
        private LifeView lifeView = null; // Set from editor

        public Vector3 TargetPosition { get { return transform.position; } }


        public void Init(Unit unitModel, Vector3 position) {
            unit = unitModel;
            transform.localPosition = position;
            SetUnitColor(unit.Faction.BaseColor, unit.Faction.FactionColor);
        }

        public Vector3 GetHitPoint() {
            return transform.position + hitOffset;
        }

        public void MoveTo(Vector3[] path, Action endMoveAction = null) {
            var moveBesie = new BesieCurve(path);
            Timer.Add(moveBesie.Lenght / moveSpeed, (anim) => {
                if (this == null)
                    return;
                transform.position = moveBesie.GetPositionOnCurve(moveBesie.Lenght * anim);
            }, endMoveAction);
        }

        private void UpdateHitPointsLabel() {
            lifeView.UpdateLife(unit);
        }

        [ContextMenu("Show death animation")]
        public void Death() {
            if (this == null)
                return;
            SetUnitMaterial(deathMaterial);
            lifeView.Hide(unit);
            // Falling
            var currentPosition = transform.position;
            var needPosition = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), -Random.Range(0.4f, 1.0f), Random.Range(-0.3f, 0.3f));
            var fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            Timer.Add(-0.1f, 1.6f, (anim) => {
                if (this == null)
                    return;
                if (transform == null)
                    return;
                transform.position = Vector3.Lerp(currentPosition, needPosition, fallCurve.Evaluate(anim));
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

        public void OnAttack(ITargetView targetView) {
            // StatusTextView.Create(damage.ToString(), Color.red, unitView.GetHitPoint());
            UpdateHitPointsLabel();
        }

        public void OnHit(int damage) {
            StatusTextView.Create(damage.ToString(), Color.red, GetHitPoint());
            UpdateHitPointsLabel();
        }

#if UNITY_EDITOR
        BesieCurve curve = null;
        private void OnDrawGizmos() {
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
}