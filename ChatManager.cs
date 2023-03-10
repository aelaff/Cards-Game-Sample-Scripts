using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Chat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    ChatClient chatClient;
    private const string appId = "";
    private string appVersion;
    private string otherPlayerID;
    public GameplayUIInit gameplayUI;
    public GameObject ChatButton;
    public Button AddFriendButton;
    public Button AddFriendButton2;

    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnChatStateChange(ChatState state)
    {
        print("isChange");
    }

    public void OnConnected()
    {
        Debug.Log("OnConnected");
        ChatButton.SetActive(true);
    }

    public void OnDisconnected()
    {
        Debug.Log("OnDisconnected");
        ChatButton.SetActive(false);
    }

    public void Disconnect()
    {
        chatClient.Disconnect();
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        if (sender.Equals(otherPlayerID))
        {
            Debug.Log("msg " + message + ", sender " + sender);
            gameplayUI.OnNewMessage(message + "");
        }
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
    }

    public void OnUserSubscribed(string channel, string user)
    {
        
        
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
    }

    public void OnUnsubscribed(string[] channels)
    {
    }

    public void InitChat(string localPlayer, string remotePlayer)
    {
        FirebasePlayer fbPlayer = new FirebasePlayer {uid = remotePlayer};

        if (FirebaseManager.Instance.player.IsFriendOrMe(fbPlayer))
        {
            AddFriendButton2.gameObject.SetActive(false);
            AddFriendButton2.interactable = false;
        }

        if (FirebaseManager.Instance.player.IsBlocked(remotePlayer))
        {
            ChatButton.SetActive(false);
            AddFriendButton.interactable = false;
        }
        else
        {
            chatClient = new ChatClient(this);
            ExitGames.Client.Photon.Chat.AuthenticationValues authValues =
                new ExitGames.Client.Photon.Chat.AuthenticationValues(localPlayer);
            chatClient.ChatRegion = "EU";
            otherPlayerID = remotePlayer;
            chatClient.Connect(appId, appVersion, authValues);
            ChatButton.SetActive(true);
            AddFriendButton.interactable = true;
        }
    }

    public bool TrySendMessage(string msg)
    {
        if (chatClient != null && chatClient.CanChat && msg.Trim().Length > 0)
        {
            chatClient.SendPrivateMessage(otherPlayerID, msg);
            return true;
        }
        else
        {
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (chatClient != null)
            chatClient.Service();
    }
}