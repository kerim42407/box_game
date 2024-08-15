using UnityEngine;

public enum EventType
{
    Positive,
    Negative,
    Neutral
}

[CreateAssetMenu(fileName = "NewEvent", menuName = "Game/Event", order = 1)]
public class EventBase : ScriptableObject
{
    public string eventName;
    public EventType eventType;
    public string description;
    public int value;

    public void ApplyEvent(LocationController locationController)
    {
        locationController.productivity += value;
        locationController.events.Add(this);
        //Debug.Log($"Event Applied: {eventName}, Value: {value}");
    }

    public void RemoveEvent(LocationController locationController)
    {
        locationController.productivity -= value;
        locationController.events.Remove(this);
    }
}