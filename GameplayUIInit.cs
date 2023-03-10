using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameplayUIInit : MonoBehaviour
{
    public Transform chatContent;
    public GameObject localMessagePrefab, remoteMessagePrafab;
    public InputField chatInputField;
    public FirebasePlayer localPlayer;
    public RpsDemoConnect2 connectScript;
    public ChatManager chatManager;
    public Image otherPlayerImage;
    public Image addPlayerImage;
    public Button passButton;
    public static GameplayUIInit Instance;
    public GameObject winPanal, losePanal;
    public GameObject chatPanel;
    public GameObject notificationImage;

    // Use this for initialization
    void Start()
    {
        Instance = this;
        if (FirebaseManager.Instance != null)
            localPlayer = FirebaseManager.Instance.player;
        else
        {
            localPlayer = new FirebasePlayer("Test User" + ((Time.unscaledTime + 10) * UnityEngine.Random.value * 10),
                "TEST_USER_UID" + ((Time.unscaledTime + 10) * UnityEngine.Random.value * 10), "US",
                "https://pokecharms.com/data/attachment-files/2015/10/236933_Charmander_Picture.png", 213,50);
#if UNITY_EDITOR
            localPlayer.photoURL = "https://pokecharms.com/data/attachment-files/2015/10/236932_Bulbasaur_Picture.png";
#endif
        }

        connectScript.ApplyUserIdAndConnect(localPlayer.GetNameFormated(), localPlayer.uid, localPlayer.location,
            localPlayer.photoURL, localPlayer.score );
    }

    public void OnNewMessage(string message)
    {
        GameObject gameObject = Instantiate(remoteMessagePrafab, chatContent);
        ChatViewHolder chatViewHolder = gameObject.GetComponent<ChatViewHolder>();
        chatViewHolder.userImage.sprite = otherPlayerImage.sprite;
        chatViewHolder.shownText.text = message;
        if (!chatPanel.activeSelf)
        {
            notificationImage.SetActive(true);
        }
        
    }


    public void SendPhotonChatMessege()
    {
        string message = chatInputField.text;
        bool result = chatManager.TrySendMessage(message);
        chatInputField.text = result ? "" : chatInputField.text;
        if (result)
        {
            GameObject gameObject = Instantiate(localMessagePrefab, chatContent);
            ChatViewHolder chatViewHolder = gameObject.GetComponent<ChatViewHolder>();
            chatViewHolder.shownText.text = message;
        }
    }


    public void InitChat(string remotePlayer)
    {
        chatManager.InitChat(localPlayer.uid, remotePlayer);
    }

    public void PassTurn()
    {
        passButton.gameObject.SetActive(false);
        GameManager.Instance.EndTurn();
        GameManager.Instance.ReleaseAllCards();
        GameManager.Instance.photonView.RPC("PassTurn", PhotonTargets.All);
    }

    public void ShowPassButton()
    {
        passButton.gameObject.SetActive(true);
    }

    public void HidePassButton()
    {
        passButton.gameObject.SetActive(false);
    }

    public void Cancel()
    {
        SceneManager.LoadScene(Constants.startScene);
    }

    public void WeWin(bool weWin)
    {
        Camera.main.transform.position = Vector3.right * 200 ;
        winPanal.SetActive(weWin);
        losePanal.SetActive(!weWin);
        winPanal.transform.parent.gameObject.SetActive(true);
    }

    public void UpdatePhoto()
    {
        addPlayerImage.sprite = otherPlayerImage.sprite;
    }

    public void AddFriend()
    {
        Debug.Log("AddFriend");
        if (PhotonNetwork.otherPlayers.Length > 0 && FirebaseManager.Instance != null)
        {
            string fuid = (string) PhotonNetwork.otherPlayers[0].CustomProperties["FirebaseId"];
            FirebaseManager.Instance.SendFriendRequest(fuid);
        }
    }

    public void BlockPerson()
    {
        Debug.Log("Block Person");
        if (PhotonNetwork.otherPlayers.Length > 0 && FirebaseManager.Instance != null)
        {
            string fuid = (string) PhotonNetwork.otherPlayers[0].CustomProperties["FirebaseId"];
            FirebaseManager.Instance.BlockFriend(fuid);
        }
    }
}