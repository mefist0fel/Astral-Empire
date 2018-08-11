using UnityEngine;

public delegate void Action();

public static class Utils {
    const uint defaultPixelToUnit = 108;

    public static Color WithAlpha(this Color color, float alpha) {
        return new Color(color.r, color.g, color.b, alpha);
    }

    public static T Create<T>(Transform parent, Vector3 position, string name = "") where T : Component {
        GameObject gameObject = new GameObject();
        if (name == "") {
            gameObject.name = typeof(T).Name;
        } else {
            gameObject.name = name;
        }
        if (parent != null) {
            gameObject.transform.parent = parent;
        }
        gameObject.transform.localPosition = position;

        T component = gameObject.AddComponent<T>();
        return component;
    }

    public static T Create<T>(Transform parent, string name = "") where T : Component {
        return Create<T>(parent, Vector3.zero, name);
    }

    public static T Create<T>(string name = "") where T : Component {
        return Create<T>(null, Vector3.zero, name);
    }

    public static GameObject LoadPrefab(string resourceName, Transform parent = null) {
        GameObject prefab = (GameObject)GameObject.Instantiate(Resources.Load(resourceName));
        if (prefab != null) {
            prefab.transform.parent = parent;
        }
        return prefab;
    }

    public static GameObject LoadPrefab(string resourceName, Vector3 position, Transform parent = null) {
        GameObject prefab = LoadPrefab(resourceName, parent);
        if (prefab != null) {
            prefab.transform.localPosition = position;
        }
        return prefab;
    }

    public static T LoadPrefab<T>(string resourceName, Vector3 position, Transform parent = null) where T : Component {
        return LoadPrefab(resourceName, position, parent).GetComponent<T>();
    }

    public static T LoadPrefab<T>(string resourceName, Transform parent = null) where T : Component {
        return LoadPrefab(resourceName, parent).GetComponent<T>();
    }

    public static SpriteRenderer AddSprite(GameObject obj, Texture2D texture, Rect rect, Vector3 pivot, uint pixelToUnit = defaultPixelToUnit) {
        SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = UnityEngine.Sprite.Create(texture, rect, pivot, pixelToUnit);
        return spriteRenderer;
    }

    public static SpriteRenderer AddSprite(GameObject obj, UnityEngine.Sprite sprite) {
        SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        return spriteRenderer;
    }

    public static GameObject CreateGameObject(Transform parent, string name = "") {
        return CreateGameObject(parent, Vector3.zero, name);
    }

    public static GameObject CreateGameObject(Transform parent, Vector3 position, string name = "") {
        GameObject gameObject = new GameObject();
        if (name == "") {
            gameObject.name = "GameObject";
        } else {
            gameObject.name = name;
        }
        gameObject.transform.parent = parent;
        gameObject.transform.localPosition = position;
        return gameObject;
    }

    public static T GetOrCreateComponent<T>(this GameObject gameObject) where T : Component {
        T component = gameObject.GetComponent<T>();
        if (component == null) {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    public static float GetAngle(Vector3 m, Vector3 n) {
        float angle = Vector3.Angle((m - n), Vector3.up);
        if (m.x < n.x) {
            angle = 360 - angle;
        }
        return angle;
    }

    public static float GetAngle(Vector2 m, Vector2 n) {
        float angle = Vector2.Angle((m - n), Vector2.up);
        if (m.x < n.x) {
            angle = 360 - angle;
        }
        return angle;
    }

    public static float GetAngle(Vector2 m) {
        return GetAngle(m, Vector2.zero);
    }

    public static float RoundAngle(float angle) {
        if (angle < 0) {
            angle += 360f;
        }
        if (angle >= 360f) {
            angle -= 360f;
        }
        return angle;
    }

    public static float RotateToAngle(float sourceAngle, float destAngle, float speed) {
        float angle = destAngle;
        if (sourceAngle < destAngle) {
            if (destAngle - sourceAngle <= 180f) {
                angle = sourceAngle + speed;
                if (angle > destAngle) {
                    angle = destAngle;
                }
            } else {
                angle = sourceAngle - speed;
                if (angle < 0) {
                    angle = angle + 360f;
                    if (angle < destAngle) {
                        angle = destAngle;
                    }
                }
            }
        } else if (sourceAngle > destAngle) {
            if (sourceAngle - destAngle <= 180f) {
                angle = sourceAngle - speed;
                if (angle < destAngle) {
                    angle = destAngle;
                }
            } else {
                angle = sourceAngle + speed;
                if (angle > 360f) {
                    angle = angle - 360f;
                    if (angle > destAngle) {
                        angle = destAngle;
                    }
                }
            }
        }
        return angle;
    }

    public static Rect ScreenQuadRect(float x, float y, float w = 1f, float h = 1f) {
        if (Screen.width > Screen.height) {
            float xShift = (float)(Screen.width - Screen.height) / 2f;
            float basePixels = (float)Screen.height / 10f;
            return new Rect(xShift + x * basePixels, y * basePixels, w * basePixels, h * basePixels);
        } else {
            float yShift = (float)(Screen.height - Screen.width) / 2f;
            float basePixels = (float)Screen.width / 10f;
            return new Rect(x * basePixels, yShift + y * basePixels, w * basePixels, h * basePixels);
        }
    }

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t) {
        return a * (1f - t) + b * t;
    }

    public static bool PointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c) //is point in abc triangle
    {

        if (HalfPlaneSign(a, b, c) == HalfPlaneSign(a, b, point) &&
            HalfPlaneSign(c, a, b) == HalfPlaneSign(c, a, point) &&
            HalfPlaneSign(b, c, a) == HalfPlaneSign(b, c, point)) {
            return true;
        }
        return false;
    }


    public static int HalfPlaneSign(Vector2 a, Vector2 b, Vector2 c) // show c point sign for ab line
    {
        Vector2 delta = a - b;
        if ((delta.x * (c.y - a.y) - delta.y * (c.x - a.x)) > 0) {
            return 1;
        } else {
            return -1;
        }
    }

    public static Color GetColor(uint hex) {
        return new Color(
            (float)(hex >> 16 & 0xFF) / 255f,
            (float)(hex >> 8 & 0xFF) / 255f,
            (float)(hex & 0xFF) / 255f);
    }

    public static Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c) {
        var sideBA = b - a;
        var sideCA = c - a;
        return Vector3.Cross(sideBA, sideCA).normalized;
    }
}