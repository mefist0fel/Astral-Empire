using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    [Serializable]
    public sealed class Container {
        [JsonProperty("items")]
        private Dictionary<string, int> items = new Dictionary<string, int>();

        private System.Action onUpdate = null;

        public int this[string itemId] {
            get { return GetItem(itemId); }
            set { SetItem(itemId, value); }
        }

        [JsonIgnore]
        public Dictionary<string, int> Items {
            get { return items; }
        }

        const string randomId = "random";

        public void SetItems(Container items) {
            if (items != null && items.items != null) {
                foreach (var item in items.items) {
                    SetItem(item.Key, item.Value);
                }
            }
        }

        public void SetItems(Dictionary<string, int> items) {
            if (items != null) {
                foreach (var item in items) {
                    SetItem(item.Key, item.Value);
                }
            }
        }

        public int GetItem(string itemId) {
            if (itemId == String.Empty) {
                return 0;
            }
            if (itemId.StartsWith(randomId)) {
                int randomMaxValue;
                if (!int.TryParse(itemId.Remove(0, randomId.Length), out randomMaxValue)) {
                    randomMaxValue = 100;
                }
                return UnityEngine.Random.Range(0, randomMaxValue);
            }
            if (items.ContainsKey(itemId))
                return items[itemId];
            return 0;
        }

        public void SetItem(string itemId, int count = 0) {
            // Remove zero element items
            if (count == 0) {
                if (items.ContainsKey(itemId)) {
                    items.Remove(itemId);
                }
                UpdateResourceCount(itemId, count);
                return;
            }
            if (items.ContainsKey(itemId)) {
                items[itemId] = count;
            } else {
                items.Add(itemId, count);
            }
            UpdateResourceCount(itemId, count);
        }

        private void UpdateResourceCount(string itemId, int count) {
            if (onUpdate != null) {
                onUpdate();
            }
        }

        public void SubscribeAll(System.Action onResourceUpdate) {
            onUpdate += onResourceUpdate;
        }
    }
}
