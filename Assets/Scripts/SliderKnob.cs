using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
public class SliderKnob : MonoBehaviour, IDragHandler
{
    public SpriteRenderer Foreground;
    public DragSlider Slider;
   
    private new Transform transform;
    private Vector2 knobPosition;
    private float positionLimit;

    public void OnDrag(PointerEventData eventData)
    {
        var dragWorldPosition = eventData.position.ToWorld().x;
        if (Mathf.Abs(eventData.delta.x) > 0f)
        {
            knobPosition.x = Mathf.Clamp(dragWorldPosition, -positionLimit, positionLimit);
            transform.position = knobPosition;
            Slider?.PositionChanged?.Invoke(knobPosition.x);
        }        
    }
    
    private void Start()
    {
        transform = GetComponent<Transform>();
        knobPosition = transform.position;
        positionLimit = CalculatePositionLimit();
    }

    private float CalculatePositionLimit()
    {
        var knobHalfwidth = GetComponent<SpriteRenderer>().bounds.extents.x;
        var backgroundHalfwidth = Foreground.bounds.extents.x;
        return backgroundHalfwidth - knobHalfwidth;
    }
}
