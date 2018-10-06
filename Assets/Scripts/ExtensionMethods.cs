using UnityEngine;

public static class ExtensionMethods
{ 
    public static Vector2 ToWorld(this Vector2 screenPosition)
        => Camera.main.ScreenToWorldPoint(screenPosition);

    public static bool HasComponent<T>(this Component component) where T : Component 
        => component.GetComponent<T>() != null;

    public static bool HasComponent<T>(this GameObject go) where T : Component
        => go.GetComponent<T>() != null;
}
