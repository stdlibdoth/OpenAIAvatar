using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class ThinkingAnim : MonoBehaviour
{
    [SerializeField] TextMeshPro m_textMeshPro;
    [SerializeField] float m_interval;
    private int m_count;
    private bool m_flag;
    private float m_prevUpdateTime;

    private Coroutine m_coroutine;

    private void Start()
    {
        m_count = 0;
    }

    public void StopAnimation()
    {
        if(m_coroutine != null)
            StopCoroutine(m_coroutine);
        m_textMeshPro.text = string.Empty;
    }

    public void StartAnimation()
    {
        m_coroutine = StartCoroutine(AnimationCoroutine());
    }

    private IEnumerator AnimationCoroutine()
    {
        while (true)
        {
            if (Time.time - m_prevUpdateTime > m_interval)
            {
                var count = m_count % 7;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < count; i++)
                {
                    sb.Append('.');
                }
                m_textMeshPro.text = sb.ToString();
                m_count++;
                m_prevUpdateTime = Time.time;
            }
            yield return null;
        }
    }

}
