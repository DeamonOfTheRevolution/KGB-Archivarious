using UnityEngine;
using UnityEngine.EventSystems;

public class ShelfCellDropHandler : MonoBehaviour, IDropHandler
{
    [SerializeField] private int cellIndex; // индекс в массиве ячеек ArchiveShelf
    private ArchiveShelf shelf;

    void Start()
    {
        shelf = GetComponentInParent<ArchiveShelf>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Получаем перетаскиваемый объект
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        DocumentCubeView docView = droppedObj.GetComponent<DocumentCubeView>();
        if (docView == null) return;

        // Попытаться заархивировать документ в эту ячейку
        if (shelf != null)
        {
            shelf.TryStoreDocumentFromDrop(droppedObj, cellIndex);
        }
    }
}