using OpenAI;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Collections;
using UnityEngine.Networking;


public class AudioRedorder : MonoBehaviour
{

    [SerializeField] private string openAIKey = "api-key";
    [SerializeField] private TTSModel model = TTSModel.TTS_1;


    private static AudioRedorder m_singleton;
    private List<string> m_divices;

    private float speed = 1f;
    private readonly string outputFormat = "mp3";


    private bool m_isRecording;
    private AudioClip m_recordClip;
    private OpenAIApi m_api;

    private string m_recordedClipPath;
    private string m_tmpTTSPath;
    private static bool m_enabled;
    private static HashSet<NPCScript> m_npcs = new HashSet<NPCScript>();


    private void Awake()
    {
        //Screen.SetResolution(1366, 768, true);
        Screen.SetResolution(3840, 2160, true);
    }

    void Start()
    {
        m_divices = new List<string>(Microphone.devices);
        m_api = new OpenAIApi(openAIKey);
        m_recordedClipPath = Application.streamingAssetsPath + "/record.wav";
        m_tmpTTSPath = Application.streamingAssetsPath + "tts.wav";
    }




    private async void Update()
    {
        if (!m_enabled)
            return;


        if(!m_isRecording && Input.GetKeyDown(KeyCode.E))
        {
            m_isRecording = true;
            m_recordClip = Microphone.Start(m_divices[0], true, 30, 44100);
            foreach (var npc in m_npcs)
            {
                npc.StartRecording();
            }
        }
        else if(m_isRecording && Input.GetKeyUp(KeyCode.E))
        {
            Microphone.End(m_divices[0]);
            m_isRecording = false;
            var save = new SavWav();
            try
            {
                var audioclip = save.TrimSilence(m_recordClip, 0.10f);
                save.Save(m_recordedClipPath, audioclip);
                foreach (var npc in m_npcs)
                {
                    npc.StartThninkingAnimation();
                }
                await Create_Audio_Transcriptions();
            }
            catch
            {
                Debug.Log("adf");
                foreach (var npc in m_npcs)
                {
                    npc.StopThninkingAnimation();
                }
            }
        }
    }

    public static bool Enabled
    {
        get { return m_enabled; }
        set { m_enabled = value; }
    }

    public static void AddNPC(NPCScript npc)
    {
        m_npcs.Add(npc);
    }

    public static void RemoveNPC(NPCScript npc)
    {
        m_npcs.Remove(npc);
    }

    public async Task<byte[]> RequestTextToSpeech(string text, TTSVoice voice)
    {
        Debug.Log("Sending new request to OpenAI TTS.");
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAIKey);

        TTSPayload payload = new TTSPayload
        {
            model = this.model.EnumToString(),
            input = text,
            voice = voice.ToString().ToLower(),
            response_format = this.outputFormat,
            speed = this.speed
        };

        string jsonPayload = JsonUtility.ToJson(payload);

        var httpResponse = await httpClient.PostAsync(
            "https://api.openai.com/v1/audio/speech",
            new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

        byte[] response = await httpResponse.Content.ReadAsByteArrayAsync();

        if (httpResponse.IsSuccessStatusCode)
        {
            return response;
        }
        Debug.Log("Error: " + httpResponse.StatusCode);
        return null;
    }





    public async Task Create_Audio_Transcriptions()
    {
        var req = new CreateAudioTranscriptionsRequest
        {
            File = m_recordedClipPath,
            Model = "whisper-1",
            Language = "en"
        };
        var res = await m_api.CreateAudioTranscription(req);


        List<ChatMessage> messages = new List<ChatMessage>();
        var npc = new List<NPCScript>(m_npcs);
        messages.AddRange(npc[0].GetBackGround());
        messages.Add(new ChatMessage { Role = "user", Content = res.Text });
        TTSVoice voice = npc[0].Voice;

        var chatreq = new CreateChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            MaxTokens = 110,
            Messages = messages
        };
        var chatres = await m_api.CreateChatCompletion(chatreq);
        
        foreach (var chat in chatres.Choices)
        {
            Debug.Log(chat.Message.Role);
            Debug.Log(chat.Message.Content);
        }


        byte[] audioData = await RequestTextToSpeech(chatres.Choices[0].Message.Content,voice);
        File.WriteAllBytes(m_tmpTTSPath, audioData);
        foreach (var n in m_npcs)
        {
            n.StopThninkingAnimation();
        }
        StartCoroutine(LoadAndPlayAudio(m_tmpTTSPath));
    }

    public async Task Create_Chat_Completion()
    {
        var req = new CreateChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage>()
                {
                    new ChatMessage() { Role = "user", Content = "Hello!" }
                }
        };
        var res = await m_api.CreateChatCompletion(req);
    }

    private IEnumerator LoadAndPlayAudio(string filePath)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            foreach (var npc in m_npcs)
            {
                npc.PlayAudioClip(DownloadHandlerAudioClip.GetContent(www));
            }
        }
        else
        {
            Debug.LogError("Audio file loading error: " + www.error);
        }

        File.Delete(filePath);
    }

}
