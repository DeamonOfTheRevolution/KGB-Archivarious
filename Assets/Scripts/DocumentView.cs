using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DocumentView : MonoBehaviour, IPointerClickHandler
{
    private BaseDocument document;
    private System.Action<BaseDocument, GameObject> onClickCallback; // изменён тип делегата

    public Text idText;
    public Text typeText;

    public void Setup(BaseDocument doc, System.Action<BaseDocument, GameObject> callback)
    {
        document = doc;
        onClickCallback = callback;
        idText.text = doc.documentID;
        typeText.text = doc.docType.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(document, gameObject); // передаём gameObject вместе с данными
    }
}