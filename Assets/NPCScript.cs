using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OpenAI;

public class NPCScript : MonoBehaviour
{

    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private TextMeshPro m_uiTMP;
    [SerializeField] private ThinkingAnim m_tinkingAnim;
    [SerializeField] private NPCBackground m_background;
    [SerializeField] private TTSVoice m_voice = TTSVoice.Alloy;

    GameObject m_player;

    private int m_uiState;
    string uiState0 = "Press and hold \"E\" to talk";

    private void Start()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
    }


    private void Update()
    {
        if (m_player == null)
            return;


        float dist = Vector3.Distance(m_player.gameObject.transform.position, gameObject.transform.position);
        if (dist <=1.5 && m_uiState == 0)
        {
            m_uiTMP.text = uiState0;
            AudioRedorder.AddNPC(this);
            AudioRedorder.Enabled = true;
            m_uiState = 1;
        }
        else if(dist>1.5 && m_uiState!= 0)
        {
            m_uiTMP.text = string.Empty;
            AudioRedorder.Enabled = false;
            AudioRedorder.RemoveNPC(this);
            m_uiState = 0;
        }
    }

    public TTSVoice Voice
    {
        get { return m_voice; }
    }

    public List<ChatMessage> GetBackGround()
    {
        return m_background.GetBackgroundInfo();
    }

    public void PlayAudioClip(AudioClip clip)
    {
        m_audioSource.clip = clip;
        m_audioSource.Play();
    }

    public void StartRecording()
    {
        m_uiTMP.text = "Listening...";
        m_uiState = 1;
    }


    public void StartThninkingAnimation()
    {
        m_tinkingAnim.StartAnimation();
        m_uiTMP.text = string.Empty;
        m_uiState = 2;
    }

    public void StopThninkingAnimation()
    {
        m_tinkingAnim.StopAnimation();
        m_uiState = 3;
        if(Vector3.Distance(m_player.gameObject.transform.position, gameObject.transform.position)<=1.5f)
        {
            m_uiTMP.text = uiState0;
        }
        else
        {
            m_uiTMP.text = string.Empty;
        }
    }
}
