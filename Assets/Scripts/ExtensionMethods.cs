using UnityEngine;

public static class ExtensionMethods
{ 
    public static Vector2 ToWorld(this Vector2 screenPosition)
    {
        return Camera.main.ScreenToWorldPoint(screenPosition);
    }
}
