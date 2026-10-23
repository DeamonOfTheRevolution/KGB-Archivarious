using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorkDayManager : MonoBehaviour
{
    [Header("References")]
    public ArchiveRules rules;
    public ArchiveShelf shelf;
    public Transform deskParent;
    public GameObject documentCubePrefab;
    public Text logText;
    public Button endDayButton;

    [Header("Tasks")]
    public List<DayTask> todaysTasks;

    [Header("Request Info UI")]
    public InputField requestInputField;
    public Text requestResultText;

    // Текущая активная задача и её состояние
    public DayTask activeTask { get; private set; }   // для доступа из других скриптов
    private int currentTaskIndex = 0;
    private bool currentTaskSelected = false;
    private List<BaseDocument> currentTaskDocuments = new List<BaseDocument>();
    private List<GameObject> currentTaskObjects = new List<GameObject>();

    // Выбранный документ (может использоваться кнопочными действиями)
    private BaseDocument selectedDocument;
    private GameObject selectedDocumentObject;

    // Для задачи «Составить дело»
    private List<BaseDocument> caseComposition = new List<BaseDocument>();

    // Ссылка на UI для обновления кнопок задач
    public GameUI gameUI;

    void Start()
    {
        shelf.workDayManager = this;
        endDayButton.onClick.AddListener(EndDay);
        StartCoroutine(GenerateDay());
    }

    IEnumerator GenerateDay()
    {
        yield return null;
        if (todaysTasks.Count > 0)
        {
            ActivateTask(0);
            logText.text = "Рабочий день начался. Выберите первую задачу в списке.";
        }
        else
        {
            logText.text = "На сегодня нет задач. Можно завершить день.";
        }
    }

    /// <summary>Переключает рабочую область на указанную задачу (индекс в todaysTasks).</summary>
    void ActivateTask(int index)
    {
        if (index < 0 || index >= todaysTasks.Count) return;

        // Очистка предыдущей задачи
        ClearCurrentTaskDocuments();
        currentTaskIndex = index;
        currentTaskSelected = false;
        activeTask = todaysTasks[index];

        // Создаём документы для новой задачи
        foreach (var doc in activeTask.requiredDocuments)
            SpawnDocumentForTask(doc);
        foreach (var doc in activeTask.optionalDocuments)
            SpawnDocumentForTask(doc);

        // Обновляем UI – активна только кнопка текущей задачи
        if (gameUI != null)
            gameUI.UpdateTaskButtons(currentTaskIndex);
    }

    /// <summary>Создаёт на столе куб-документ и сразу делает его неактивным.</summary>
    void SpawnDocumentForTask(BaseDocument docAsset)
    {
        GameObject obj = Instantiate(documentCubePrefab, deskParent);
        BaseDocument docInstance = Instantiate(docAsset);
        DocumentCubeView view = obj.GetComponent<DocumentCubeView>();
        view.Setup(docInstance, OnDocumentClicked);
        view.SetInteractable(false);          // документ не кликабелен до выбора задачи

        if (obj.GetComponent<DraggableDocument>() == null)
            obj.AddComponent<DraggableDocument>();

        currentTaskDocuments.Add(docInstance);
        currentTaskObjects.Add(obj);
    }

    /// <summary>Удаляет все объекты текущей задачи со стола.</summary>
    void ClearCurrentTaskDocuments()
    {
        foreach (var obj in currentTaskObjects)
            Destroy(obj);
        currentTaskDocuments.Clear();
        currentTaskObjects.Clear();
        selectedDocument = null;
        selectedDocumentObject = null;
        caseComposition.Clear();
    }

    /// <summary>Обработчик клика по документу (вызывается из DocumentCubeView).</summary>
    void OnDocumentClicked(BaseDocument doc, GameObject obj)
    {
        if (!currentTaskSelected) return;
        selectedDocument = doc;
        selectedDocumentObject = obj;
        logText.text = $"Выбран документ: {doc.documentID}";
    }

    // ================== МЕТОДЫ, ВЫЗЫВАЕМЫЕ ИЗ UI ==================

    /// <summary>Выбор задачи пользователем (нажатие на кнопку задачи в списке).</summary>
    public void SelectTask(DayTask task)
    {
        // Разрешаем выбор только текущей задачи
        if (task != activeTask)
        {
            logText.text = "Эта задача сейчас недоступна. Выполните текущую.";
            return;
        }

        // Если задача ещё не активирована – делаем документы интерактивными
        if (!currentTaskSelected)
        {
            foreach (var obj in currentTaskObjects)
            {
                DocumentCubeView view = obj.GetComponent<DocumentCubeView>();
                if (view != null) view.SetInteractable(true);
            }
            currentTaskSelected = true;
            logText.text = $"Задача: {task.description}. Работайте с документами.";
        }
    }

    /// <summary>Оценка документа по правилам (используется из ArchiveShelf).</summary>
    public (int secrecy, int years) EvaluateDocument(BaseDocument doc)
    {
        List<BaseDocument> docs = new List<BaseDocument> { doc };
        if (doc.docType == DocumentType.CaseFolder)
            docs = doc.attachedDocuments;
        return rules.Evaluate(docs);
    }

    /// <summary>Вызывается из ArchiveShelf при успешном помещении документа в ячейку.</summary>
    public void OnDocumentArchived(BaseDocument doc, int secrecy, int years)
    {
        if (activeTask == null || !currentTaskSelected) return;

        // Ищем среди документов текущей задачи
        int idx = currentTaskDocuments.IndexOf(doc);
        if (idx < 0) return;

        // Регистрируем выполненный документ в задаче и удаляем его
        activeTask.RegisterDocumentProcessed(doc);
        Destroy(currentTaskObjects[idx]);
        currentTaskDocuments.RemoveAt(idx);
        currentTaskObjects.RemoveAt(idx);

        logText.text = $"Документ {doc.documentID} заархивирован.";

        if (activeTask.completed)
        {
            logText.text += " Задача выполнена!";
            CompleteCurrentTask();
        }
    }

    /// <summary>Завершает текущую задачу и переходит к следующей (или завершает день).</summary>
    void CompleteCurrentTask()
    {
        ClearCurrentTaskDocuments();   // убираем всё, что осталось

        if (currentTaskIndex + 1 < todaysTasks.Count)
        {
            ActivateTask(currentTaskIndex + 1);
            logText.text += "\nНовая задача доступна. Выберите её в списке.";
        }
        else
        {
            logText.text += "\nВсе задачи выполнены! Можно завершить день.";
            activeTask = null;
        }
    }

    // ================== ОПЕРАЦИИ ДЛЯ КНОПОК ИНСТРУМЕНТОВ ==================

    /// <summary>Архивация через кнопку (использует выделенный документ).</summary>
    public void ArchiveSelected()
    {
        if (activeTask == null || !currentTaskSelected)
        {
            logText.text = "Сначала выберите задачу.";
            return;
        }
        if (selectedDocument == null || selectedDocumentObject == null)
        {
            logText.text = "Выделите документ на столе.";
            return;
        }

        var (sec, yrs) = EvaluateDocument(selectedDocument);
        string msg;
        if (shelf.TryStoreDocument(selectedDocument, sec, yrs, selectedDocumentObject, out msg))
        {
            logText.text = msg;
            // TryStoreDocument уже вызывает OnDocumentArchived
        }
        else
        {
            logText.text = msg;
        }
    }

    /// <summary>Дезархивация – уничтожить просроченные документы (упрощённо).</summary>
    public void DearchiveTask()
    {
        if (activeTask == null || !currentTaskSelected || activeTask.taskType != DayTask.TaskType.Dearchive)
        {
            logText.text = "Нет активной задачи на дезархивацию.";
            return;
        }
        List<BaseDocument> expired = shelf.GetExpiredDocuments();
        if (expired.Count == 0)
        {
            logText.text = "Нет документов с истекшим сроком хранения.";
            return;
        }
        // Уничтожаем первый попавшийся
        BaseDocument doc = expired[0];
        shelf.RemoveDocument(doc);
        logText.text = $"Документ {doc.documentID} дезархивирован и уничтожен.";
        activeTask.RegisterDocumentProcessed(doc);
        if (activeTask.completed) CompleteCurrentTask();
    }

    /// <summary>Добавить выделенный документ во временную папку дела.</summary>
    public void AddToCaseComposition()
    {
        if (selectedDocument == null)
        {
            logText.text = "Сначала выделите документ.";
            return;
        }
        if (!caseComposition.Contains(selectedDocument))
            caseComposition.Add(selectedDocument);
        logText.text = $"Документ добавлен в состав дела. Всего: {caseComposition.Count}.";
    }

    /// <summary>Сформировать папку дела и положить её на стол.</summary>
    public void ComposeCase()
    {
        if (activeTask == null || !currentTaskSelected || activeTask.taskType != DayTask.TaskType.ComposeCase)
        {
            logText.text = "Нет активной задачи на составление дела.";
            return;
        }
        if (caseComposition.Count == 0)
        {
            logText.text = "Добавьте документы в дело (кнопкой «В дело»).";
            return;
        }

        // Создаём папку дела и её визуальный куб
        BaseDocument caseFolder = ScriptableObject.CreateInstance<BaseDocument>();
        caseFolder.docType = DocumentType.CaseFolder;
        caseFolder.documentID = "26-06-0100-A"; // пример
        caseFolder.attachedDocuments = new List<BaseDocument>(caseComposition);
        var (sec, yrs) = EvaluateDocument(caseFolder);
        caseFolder.secrecyLevel = sec;
        caseFolder.storageYears = yrs;

        SpawnDocumentForTask(caseFolder);
        logText.text = "Папка дела сформирована. Заархивируйте её.";
        activeTask.RegisterDocumentProcessed(caseFolder);
        if (activeTask.completed) CompleteCurrentTask();
    }

    /// <summary>Выдать информацию по запросу (поиск документа в архиве по ID).</summary>
    public void ProvideInformation()
    {
        if (activeTask == null || !currentTaskSelected || activeTask.taskType != DayTask.TaskType.ProvideInfo)
        {
            logText.text = "Нет активной задачи на выдачу информации.";
            return;
        }
        string query = requestInputField?.text;
        if (string.IsNullOrEmpty(query))
        {
            logText.text = "Введите ID документа в поле запроса.";
            return;
        }
        BaseDocument found = shelf.FindDocumentById(query);
        if (found != null)
        {
            requestResultText.text = $"Найден: {found.documentID} – {found.description}";
            activeTask.RegisterDocumentProcessed(found);
            if (activeTask.completed) CompleteCurrentTask();
        }
        else
        {
            requestResultText.text = "Документ не найден.";
        }
    }

    /// <summary>Уничтожить объект архива по ID из задачи.</summary>
    public void DestroyArchivedObject()
    {
        if (activeTask == null || !currentTaskSelected || activeTask.taskType != DayTask.TaskType.DestroyObject)
        {
            logText.text = "Нет активной задачи на уничтожение.";
            return;
        }
        string targetId = activeTask.targetDocumentID;
        if (string.IsNullOrEmpty(targetId))
        {
            logText.text = "В задаче не указан идентификатор объекта.";
            return;
        }
        BaseDocument doc = shelf.FindDocumentById(targetId);
        if (doc != null)
        {
            shelf.RemoveDocument(doc);
            logText.text = $"Объект {targetId} уничтожен.";
            activeTask.RegisterDocumentProcessed(doc);
            if (activeTask.completed) CompleteCurrentTask();
        }
        else
        {
            logText.text = "Объект с таким ID не найден в архиве.";
        }
    }

    /// <summary>Дополнить дело: добавить выделенный документ в существующую папку в архиве.</summary>
    public void AppendToCase()
    {
        if (activeTask == null || !currentTaskSelected || activeTask.taskType != DayTask.TaskType.AppendCase)
        {
            logText.text = "Нет активной задачи на дополнение дела.";
            return;
        }
        if (selectedDocument == null)
        {
            logText.text = "Выделите документ, который хотите добавить.";
            return;
        }
        string caseId = activeTask.targetDocumentID;
        BaseDocument caseFolder = shelf.FindDocumentById(caseId);
        if (caseFolder != null && caseFolder.docType == DocumentType.CaseFolder)
        {
            caseFolder.attachedDocuments.Add(selectedDocument);
            shelf.RemoveDocument(selectedDocument); // если он уже был в архиве
            logText.text = $"Документ добавлен в дело {caseId}.";
            activeTask.RegisterDocumentProcessed(selectedDocument);
            if (activeTask.completed) CompleteCurrentTask();
        }
        else
        {
            logText.text = "Папка дела не найдена в архиве.";
        }
    }

    /// <summary>Передать материал (удалить из архива и сформировать накладную).</summary>
    public void TransferMaterial()
    {
        if (activeTask == null || !currentTaskSelected || activeTask.taskType != DayTask.TaskType.TransferMaterial)
        {
            logText.text = "Нет активной задачи на передачу материала.";
            return;
        }
        string targetId = activeTask.targetDocumentID;
        BaseDocument doc = shelf.FindDocumentById(targetId);
        if (doc != null)
        {
            shelf.RemoveDocument(doc);
            logText.text = $"Материал {targetId} передан. Накладная создана.";
            activeTask.RegisterDocumentProcessed(doc);
            if (activeTask.completed) CompleteCurrentTask();
        }
        else
        {
            logText.text = "Материал не найден в архиве.";
        }
    }

    // ================== ЗАВЕРШЕНИЕ ДНЯ ==================
    public void EndDay()
    {
        int success = todaysTasks.Count(t => t.success);
        logText.text = $"День завершён. Успешно выполнено задач: {success}/{todaysTasks.Count}.";
    }
}