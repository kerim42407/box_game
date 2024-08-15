using Michsky.MUIP;
using UnityEngine;

public enum NotificationEventType
{
    Positive,
    Negative,
    Neutral
}

[CreateAssetMenu(fileName = "NewEvent", menuName = "Game/NotificationEvent", order = 2)]
public class NotificationEventBase : ScriptableObject
{
    public EventType eventType;
    public string eventName;
    public string description;
    public NotificationManager notificationManager;

    public void ShowNotification(Transform canvas)
    {
        Instantiate(notificationManager, canvas);
        notificationManager.Open();
        notificationManager.OpenNotification();
    }
}