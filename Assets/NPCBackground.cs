using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;


public class NPCBackground : MonoBehaviour
{
    [SerializeField] string m_role;
    [SerializeField] string m_content;
    private List<ChatMessage> m_bgSettings;


    private void Awake()
    {
        m_bgSettings = new List<ChatMessage>();
    }

    private void Start()
    {
        m_bgSettings.Add(new ChatMessage { Content = m_content, Role = m_role });
    }

    public List<ChatMessage> GetBackgroundInfo()
    {
        return m_bgSettings;
    }

}
