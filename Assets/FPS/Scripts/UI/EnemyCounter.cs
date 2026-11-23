using UnityEngine;
using UnityEngine.UI;

public class EnemyCounter : MonoBehaviour
{
    [Header("Enemies")]
    [Tooltip("Text component for displaying enemy objective progress")]
    public Text enemiesText;

    EnemyManager m_EnemyManager;
    int m_LastRemaining = -1;
    int m_LastTotal = -1;

    void Awake()
    {
        m_EnemyManager = FindAnyObjectByType<EnemyManager>();
        DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyCounter>(m_EnemyManager, this);
    }

    void Update()
    {
        if (m_EnemyManager == null || enemiesText == null)
            return;

        int remaining = m_EnemyManager.numberOfEnemiesRemaining;
        int total = m_EnemyManager.numberOfEnemiesTotal;

        if (remaining != m_LastRemaining || total != m_LastTotal)
        {
            m_LastRemaining = remaining;
            m_LastTotal = total;
            enemiesText.text = remaining.ToString() + "/" + total.ToString();
        }
    }
}
