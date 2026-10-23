using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class DraggableDocument : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 offset;
    private Rigidbody rb;
    private bool isDragging = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    /// <summary>
    /// Получает камеру, связанную с событием (приоритет: pressEventCamera, enterEventCamera, Camera.main)
    /// </summary>
    private Camera GetEventCamera(PointerEventData eventData)
    {
        return eventData.pressEventCamera ?? eventData.enterEventCamera ?? Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        Vector3 worldPos = GetWorldPoint(eventData);
        offset = transform.position - worldPos;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 worldPos = GetWorldPoint(eventData);
        Vector3 targetPos = worldPos + offset;
        targetPos.y = transform.position.y;   // сохраняем высоту объекта над столом
        rb.MovePosition(targetPos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    /// <summary>
    /// Определяет мировую точку под курсором, используя физический рейкаст или горизонтальную плоскость.
    /// </summary>
    private Vector3 GetWorldPoint(PointerEventData eventData)
    {
        Camera cam = GetEventCamera(eventData);
        if (cam == null)
        {
            Debug.LogError("DraggableDocument: No camera found for event.");
            return transform.position;
        }

        Ray ray = cam.ScreenPointToRay(eventData.position);

        // Попытка пересечения с любым коллайдером в сцене
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.point;
        }

        // Если ни один коллайдер не задёт — строим горизонтальную плоскость на высоте объекта
        float y = transform.position.y;
        Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, y, 0));

        if (horizontalPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        // Самый крайний случай (когда плоскость параллельна лучу)
        return transform.position;
    }
}