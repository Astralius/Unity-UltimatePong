using System;
using UnityEngine;
using UnityEngine.Events;

public class DragSlider : MonoBehaviour
{
    public PositionChangedEvent PositionChanged;
}

[Serializable]
public class PositionChangedEvent : UnityEvent<float> { }