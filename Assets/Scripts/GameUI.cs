using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public WorkDayManager workDayManager;
    public GameObject taskButtonPrefab;
    public Transform taskListParent;

    private List<Button> taskButtons = new List<Button>();

    void Start()
    {
        // Создаём по кнопке для каждой задачи, но они будут включаться/выключаться позже
        for (int i = 0; i < workDayManager.todaysTasks.Count; i++)
        {
            DayTask task = workDayManager.todaysTasks[i];
            GameObject btnObj = Instantiate(taskButtonPrefab, taskListParent);
            Button btn = btnObj.GetComponent<Button>();
            btn.GetComponentInChildren<Text>().text = task.description;
            // Передаём вызов в WorkDayManager.SelectTask с правильной задачей
            btn.onClick.AddListener(() => workDayManager.SelectTask(task));
            taskButtons.Add(btn);
        }
        // Начальное состояние – активна только первая задача
        UpdateTaskButtons(0);
    }

    /// <summary>Разрешает нажимать только кнопку с указанным индексом.</summary>
    public void UpdateTaskButtons(int activeIndex)
    {
        for (int i = 0; i < taskButtons.Count; i++)
        {
            taskButtons[i].interactable = (i == activeIndex);
        }
    }

    // Ниже – обработчики для кнопок инструментов. Привяжите их в инспекторе к соответствующим UI-кнопкам.

    public void OnArchiveButton() => workDayManager.ArchiveSelected();
    public void OnComposeButton() => workDayManager.ComposeCase();
    public void OnAddToCaseButton() => workDayManager.AddToCaseComposition();
    public void OnDearchiveButton() => workDayManager.DearchiveTask();
    public void OnProvideInfoButton() => workDayManager.ProvideInformation();
    public void OnDestroyButton() => workDayManager.DestroyArchivedObject();
    public void OnAppendButton() => workDayManager.AppendToCase();
    public void OnTransferButton() => workDayManager.TransferMaterial();
    public void OnEndDayButton() => workDayManager.EndDay(); // если нужна отдельная кнопка
}