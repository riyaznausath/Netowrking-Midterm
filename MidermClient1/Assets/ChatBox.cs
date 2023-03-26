using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatBox : MonoBehaviour
{
    private static ChatBox instance;
    public GameObject chatBoxPanel;
    public GameObject chatBoxPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static ChatBox GetInstance()
    {
        return instance;
    }
    public void CreateMessage(string message)
    {
        GameObject messageBox = GameObject.Instantiate(chatBoxPrefab, chatBoxPanel.transform);
        messageBox.GetComponent<TextMeshProUGUI>().text = message;

    }
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;

        }
        else 
        {
            Destroy(gameObject);
        };

    }

  
}
