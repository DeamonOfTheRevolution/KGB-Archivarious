using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Task", menuName = "Archive/Day Task")]
public class DayTask : ScriptableObject
{
    public enum TaskType { Archive, Dearchive, ComposeCase, ProvideInfo, DestroyObject, AppendCase, TransferMaterial }
    public TaskType taskType;
    public string description;
    public List<BaseDocument> requiredDocuments = new List<BaseDocument>();
    public List<BaseDocument> optionalDocuments = new List<BaseDocument>();
    public string targetDocumentID;
    public int expectedSecrecy;
    public int expectedYears;

    [HideInInspector] public bool completed = false;
    [HideInInspector] public bool success = false;
    [HideInInspector] public int completedDocumentCount = 0;

    public void RegisterDocumentProcessed(BaseDocument doc)
    {
        completedDocumentCount++;
        if (completedDocumentCount >= requiredDocuments.Count)
        {
            completed = true;
            success = true; // в прототипе считаем успех при полной обработке
        }
    }
}