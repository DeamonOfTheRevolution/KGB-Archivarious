using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum DocumentType
{
    PersonInfo,       // Личное дело
    Interrogation,    // Протокол допроса
    Certificate,      // Справка
    Inspection,       // Протокол осмотра
    EventDescription, // Описание события
    CaseFolder,       // Папка дела
    ItemDescription,  // Опись предмета
    Confiscation,     // Протокол изъятия
    Manual            // Инструкция по эксплуатации
}

[Serializable]
public enum ArchiveObjectType
{
    Person,   // Личное дело
    Event,    // Событие
    Evidence  // Вещдок
}

[CreateAssetMenu(fileName = "New Document", menuName = "Archive/Document")]
public class BaseDocument : ScriptableObject
{
    public string documentID;          // хх-АА-хххх-А
    public DocumentType docType;
    public DateTime dateCreated;
    
    // Общие поля, заполняемые в зависимости от типа
    public string personName;
    public string description;
    public string expertName;
    public string organizationStamp;
    public Sprite photo;
    
    // Классификационные параметры (заполняются из правил)
    [HideInInspector] public int secrecyLevel;  // 0-3
    [HideInInspector] public int storageYears;
    
    // Для папки дела - список вложенных документов
    public List<BaseDocument> attachedDocuments = new List<BaseDocument>();
    
    // Возвращает тип архивируемого объекта, если документ - его основа
    public virtual ArchiveObjectType GetArchiveObjectType()
    {
        switch (docType)
        {
            case DocumentType.PersonInfo: return ArchiveObjectType.Person;
            case DocumentType.EventDescription: return ArchiveObjectType.Event;
            case DocumentType.ItemDescription:
            case DocumentType.Confiscation: return ArchiveObjectType.Evidence;
            default: return ArchiveObjectType.Event; // fallback
        }
    }
}