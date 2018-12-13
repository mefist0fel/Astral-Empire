using UnityEngine;
using Model;
using System;
using UnityEngine.UI;

namespace View {
    public sealed class LifeView : MonoBehaviour {
        [SerializeField]
        private Text lifeLabel = null; // Set from editor

        public void UpdateLife(Unit unitModel) {
            if (lifeLabel == null)
                return;
            lifeLabel.text = string.Format("{0}/{1}", unitModel.HitPoints, unitModel.MaxHitPoints);
        }

        public void Hide(Unit unitModel) {
            if (lifeLabel == null)
                return;
            Timer.Add(0.8f, (anim) => {
                if (lifeLabel == null)
                    return;
                lifeLabel.color = new Color(1, 1, 1, 1f - anim);
            });
        }
    }
}