using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShelfCell
{
    public string cellID;
    public int allowedSecrecyLevel;
    public int allowedStorageYears;
    public Transform slotTransform;
    public GameObject triggerZone; // объект с триггером и скриптом ShelfCellDropHandler
    [HideInInspector] public BaseDocument storedDocument;
    [HideInInspector] public GameObject storedDocumentObject; // визуальный объект
}

public class ArchiveShelf : MonoBehaviour
{
    public List<ShelfCell> cells = new List<ShelfCell>();

    // Ссылка на WorkDayManager (назначается в редакторе или через Find)
    public WorkDayManager workDayManager;

    public bool TryStoreDocumentFromDrop(GameObject docObject, int cellIndex)
    {
        if (cellIndex < 0 || cellIndex >= cells.Count) return false;
        ShelfCell cell = cells[cellIndex];
        DocumentCubeView docView = docObject.GetComponent<DocumentCubeView>();
        BaseDocument doc = docView?.document;
        if (doc == null || cell.storedDocument != null) return false;

        // Оценка правил (через менеджер)
        if (workDayManager != null)
        {
            var (secrecy, years) = workDayManager.EvaluateDocument(doc);
            if (secrecy <= cell.allowedSecrecyLevel && years <= cell.allowedStorageYears)
            {
                // Архивируем
                cell.storedDocument = doc;
                doc.secrecyLevel = secrecy;
                doc.storageYears = years;
                // Перемещаем объект на место
                docObject.transform.position = cell.slotTransform.position;
                docObject.transform.rotation = cell.slotTransform.rotation;
                // Отключаем перетаскивание
                Destroy(docObject.GetComponent<DraggableDocument>());
                // Увеличиваем счётчик выполненных задач
                workDayManager.OnDocumentArchived(doc, secrecy, years);
                return true;
            }
        }
        return false;
    }

    // Старый метод для кнопочной архивации можно оставить
    public bool TryStoreDocument(BaseDocument doc, int targetSecrecy, int targetYears, GameObject docObject, out string message)
    {
        foreach (var cell in cells)
        {
            if (cell.storedDocument == null &&
                targetSecrecy <= cell.allowedSecrecyLevel &&
                targetYears <= cell.allowedStorageYears)
            {
                cell.storedDocument = doc;
                doc.secrecyLevel = targetSecrecy;
                doc.storageYears = targetYears;
                if (docObject != null)
                {
                    docObject.transform.position = cell.slotTransform.position;
                    Destroy(docObject.GetComponent<DraggableDocument>());
                }
                workDayManager?.OnDocumentArchived(doc, targetSecrecy, targetYears);
                message = $"Документ {doc.documentID} помещён в ячейку {cell.cellID}.";
                return true;
            }
        }
        message = "Нет подходящей свободной ячейки.";
        return false;
    }
    public List<BaseDocument> GetExpiredDocuments()
    {
        List<BaseDocument> expired = new List<BaseDocument>();
        foreach (var cell in cells)
        {
            if (cell.storedDocument != null && cell.storedDocument.storageYears <= 0)
                expired.Add(cell.storedDocument);
            // Упрощённо: считаем, что срок истёк, если storageYears == 0
        }
        return expired;
    }

    public BaseDocument FindDocumentById(string id)
    {
        foreach (var cell in cells)
        {
            if (cell.storedDocument != null && cell.storedDocument.documentID == id)
                return cell.storedDocument;
        }
        return null;
    }

    public void RemoveDocument(BaseDocument doc)
    {
        foreach (var cell in cells)
        {
            if (cell.storedDocument == doc)
            {
                cell.storedDocument = null;
                // Если нужно, уничтожить визуальный объект – но у нас нет прямой ссылки.
                // В полной версии надо хранить GameObject документа в ячейке.
                break;
            }
        }

    }
}