using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArchiveRules", menuName = "Archive/Rules")]
public class ArchiveRules : ScriptableObject
{
    [System.Serializable]
    public class SecrecyRule
    {
        public string description;
        public List<Criteria> criteria;
        public int resultingSecrecy;   // 0=несекретно, 1=ДСП, 2=секретно, 3=сов.секретно
        public int resultingYears;
    }

    [System.Serializable]
    public class Criteria
    {
        public enum CriteriaType { DocumentType, HasKeyword, SecrecyLevel, ObjectClass, ExpertName }
        public CriteriaType type;
        public string value;     // для проверки
        public int intValue;     // для числовых сравнений
    }

    public List<SecrecyRule> rules = new List<SecrecyRule>();

    // Определяет параметры для заданного набора документов (одного или дела)
    public (int secrecy, int years) Evaluate(List<BaseDocument> docs)
    {
        foreach (var rule in rules)
        {
            if (MatchesRule(docs, rule))
                return (rule.resultingSecrecy, rule.resultingYears);
        }
        // По умолчанию низкая секретность
        return (0, 5);
    }

    private bool MatchesRule(List<BaseDocument> docs, SecrecyRule rule)
    {
        foreach (var crit in rule.criteria)
        {
            if (!CheckSingleCriterion(docs, crit))
                return false;
        }
        return true;
    }

    private bool CheckSingleCriterion(List<BaseDocument> docs, Criteria crit)
    {
        // Упрощённая проверка: ищем хотя бы один документ, удовлетворяющий критерию
        foreach (var doc in docs)
        {
            switch (crit.type)
            {
                case Criteria.CriteriaType.DocumentType:
                    if (doc.docType.ToString() == crit.value) return true;
                    break;
                case Criteria.CriteriaType.HasKeyword:
                    if (!string.IsNullOrEmpty(doc.description) && doc.description.Contains(crit.value))
                        return true;
                    break;
                case Criteria.CriteriaType.SecrecyLevel:
                    if (doc.secrecyLevel >= crit.intValue) return true;
                    break;
                // Можно расширить для других полей
            }
        }
        return false;
    }
}