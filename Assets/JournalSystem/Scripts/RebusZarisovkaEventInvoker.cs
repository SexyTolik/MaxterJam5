using UnityEngine;
using UnityEngine.Events;

public class RebusZarisovkaEventInvoker : MonoBehaviour
{
    public UnityEvent<int> unityEvent = new UnityEvent<int>();

    public int RebusID = -1;

    void OnMouseDown()
    {
        unityEvent.Invoke(RebusID);
    }
}
