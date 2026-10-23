using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DocumentCubeView : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI idText;
    public TextMeshProUGUI typeText;
    public BaseDocument document { get; private set; }

    private System.Action<BaseDocument, GameObject> onClickCallback;

    public void Setup(BaseDocument doc, System.Action<BaseDocument, GameObject> callback)
    {
        document = doc;
        onClickCallback = callback;
        UpdateVisuals();
        SetInteractable(false); // документ изначально неактивен
    }

    public void SetInteractable(bool state)
    {
        enabled = state;
        DraggableDocument drag = GetComponent<DraggableDocument>();
        if (drag != null) drag.enabled = state;
    }

    void UpdateVisuals()
    {
        if (document != null)
        {
            idText.text = document.documentID;
            typeText.text = document.docType.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!enabled) return;
        onClickCallback?.Invoke(document, gameObject);
    }
}