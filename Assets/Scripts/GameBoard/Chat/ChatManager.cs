using System.Collections;
using System.Collections.Generic;
using TCGSim;
using UnityEngine;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    [SerializeField]
    ChatMessage chatMessagePrefab;

    CanvasGroup chatContent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetChatContent(CanvasGroup chatContent)
    {
        this.chatContent = chatContent;
    }

    public void AddMessage(string message)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            ChatMessage chatMessage = Instantiate(chatMessagePrefab, chatContent.gameObject.transform);
            chatMessage.SetText(message);
        }); 
    }
}
