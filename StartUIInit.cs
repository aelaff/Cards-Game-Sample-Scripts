using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Facebook.MiniJSON;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Firebase.Database;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class StartUIInit : MonoBehaviour
{
    public Text playerName, playerScore, playerLocation;
    public RawImage playerPhoto;
    public InputField searchField;
    public Transform searchResultContent;
    public GameObject searchResultPrefab;
    public Transform friendsContent;
    public GameObject friendPrefab;
    public Transform notificationContent;
    public GameObject notificationPrefab;
    public GameObject invitePrefab;
    public TopNotificationManager topNotificationManager;

    public Transform LeaderBoardContent;
    public GameObject LeaderBoardPrefab;
    
    public Text StatsScore, StatsSWins,StatsStates;
    public Image[] Badges;
    
    public static StartUIInit Instance;

    // Use this for initialization
    void Start()
    {
        Instance = this;
        Disconnect();
    }

    public void Disconnect()
    {
        if (PhotonNetwork.connectedAndReady)
            PhotonNetwork.Disconnect();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupPlayerUI();
        SetupLeaderboard();
    }

    void SetupLeaderboard()
    {
        FirebaseManager.Instance.root.Child(Constants.playersNode).OrderByChild(Constants.scoreNode).LimitToLast(15)
            .GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("Fail To Load");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    Debug.Log("asdkalsdkl " + snapshot.GetRawJsonValue());
                    foreach (DataSnapshot h in snapshot.Children)
                    {
                        FirebasePlayer firebasePlayer = FirebasePlayer.FromJson(h.GetRawJsonValue());
                        GameObject go = Instantiate(LeaderBoardPrefab, LeaderBoardContent);
                        LeaderBoardItemHolder leaderBoardItemHolder = go.GetComponent<LeaderBoardItemHolder>();
                        leaderBoardItemHolder.playername.text = firebasePlayer.GetNameFormated().Split()[0];
                        leaderBoardItemHolder.score.text = firebasePlayer.score + " points";
                        
                        int max = 0;
                        if (firebasePlayer.score < 100)
                        {
                            max = 1;
                        }
                        else if (firebasePlayer.score < 200)
                        {
                            max = 2;
                        }
                        else if (firebasePlayer.score < 300)
                        {
                            max = 3;
                        }
                        else if (firebasePlayer.score < 400)
                        {
                            max = 4;
                        }
                        else
                        {
                            max = 5;
                        }

                        leaderBoardItemHolder.badge.sprite = Badges[max - 1].sprite;
                        leaderBoardItemHolder.url = firebasePlayer.photoURL;
                        go.transform.SetAsFirstSibling();
                    }
                }
            });
    }

    void SetupPlayerUI()
    {
        FirebaseManager firebaseManager = FirebaseManager.Instance;
        FirebasePlayer player = firebaseManager.player;
        playerName.text = player.GetNameFormated();
        playerScore.text = player.score + "";
        StatsScore.text = player.score + " points";
        StatsSWins.text = player.wins + " wins";
        String status;
        int max = 0;
        if (player.score < 100)
        {
            status = "Rookie Player";
            max = 1;
        }
        else if (player.score < 200)
        {
            max = 2;
            status = "Bronze Player";
        }
        else if (player.score < 300)
        {
            max = 3;
            status = "Silver Player";
        }
        else if (player.score < 400)
        {
            max = 4;
            status = "Golden Player";
        }
        else
        {
            max = 5;
            status = "Diamond Player";
        }

        for (int i = 0; i < max; i++)
        {
            Badges[i].color = Color.white;
        }
        StatsStates.text = status;
        playerLocation.text = player.location ?? "Unknown";
        StartCoroutine("GetUserImageAsync", player.photoURL);
        SetupNotificationsUI();
        SetupFriendsUI();
        SearchFor();
    }

    void SetupNotificationsUI()
    {
        FirebaseManager.Instance.ListenForFriendsChanges();
        FirebaseManager.Instance.ListenForInvitesChanges();
    }

    void SetupFriendsUI()
    {
        for (int i = 0; i < friendsContent.childCount; i++)
        {
            friendsContent.GetChild(i).gameObject.SetActive(false);
        }

        foreach (var item in FirebaseManager.Instance.player.getFriends())
        {
            switch (item.Value.friendStatus)
            {
                case Constants.friends:
                    GameObject obj = null;
                    FriendInfoPrefab friendInfoPrefab;
                    for (int i = 0; i < friendsContent.childCount; i++)
                    {
                        if (!friendsContent.GetChild(i).gameObject.activeSelf)
                        {
                            obj = friendsContent.GetChild(i).gameObject;
                            break;
                        }
                    }

                    if (obj == null || obj.GetComponent<FriendInfoPrefab>() == null)
                        obj = Instantiate(friendPrefab, friendsContent) as GameObject;


                    obj.SetActive(true);
                    friendInfoPrefab = obj.GetComponent<FriendInfoPrefab>();
                    friendInfoPrefab.uid = item.Key;
                    continue;
                case Constants.blocked:
                    //FriendNotificationPrefab[] friendNotificationPrefabs = GameObject.FindObjectsOfType<FriendNotificationPrefab>();
                    Debug.Log("blocked don't show a thing");
                    //foreach (var item1 in friendNotificationPrefabs)
                    //{
                    //if (!string.IsNullOrEmpty(item1.Uid) && item1.Uid.Equals(item.Key))
                    //    item1.gameObject.SetActive(false);
                    //}
                    continue;
                case Constants.request:
                case Constants.pending:
                    GameObject obj1 = null;
                    FriendNotificationPrefab friendInfoPrefab1;
                    for (int i = 0; i < friendsContent.childCount; i++)
                    {
                        if (!friendsContent.GetChild(i).gameObject.activeSelf)
                        {
                            obj1 = friendsContent.GetChild(i).gameObject;
                            break;
                        }
                    }

                    if (obj1 == null || obj1.GetComponent<FriendNotificationPrefab>() == null)
                        obj1 = Instantiate(notificationPrefab, friendsContent) as GameObject;


                    friendInfoPrefab1 = obj1.GetComponent<FriendNotificationPrefab>();
                    friendInfoPrefab1.friendState = item.Value.friendStatus;
                    obj1.SetActive(true);
                    friendInfoPrefab1.Uid = item.Key;
                    continue; // do hear;

                default:
                    Debug.Log("default " + item.Value.friendStatus);
                    continue;
            }
        }
    }

    public void SearchFor()
    {
        string value = searchField.text.ToLower();
        Debug.Log("Search For " + value);
        for (int i = 0; i < searchResultContent.childCount; i++)
        {
            searchResultContent.GetChild(i).gameObject.SetActive(false);
        }

        FirebaseManager firebaseManager = FirebaseManager.Instance;
        firebaseManager.SearchFor(value);
    }

    public void CancelSearch()
    {
        FirebaseManager.Instance.CancelSearch();
    }

    public void Logout()
    {
        PlayerPrefs.SetString(Constants.LastLoginProvider, "");
        FirebaseManager firebaseManager = FirebaseManager.Instance;
        firebaseManager.LogOut();
        LoadLoginScene();
    }

    private void LoadLoginScene()
    {
        SceneManager.LoadScene(Constants.loginScene);
    }

    IEnumerator GetUserImageAsync(string url)
    {
        //WWW userImageReq = new WWW(url);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url, true);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Get downloaded asset bundle
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (texture != null)
            {
                playerPhoto.texture = texture;
            }
        }

        //userImage.texture = userImageReq.texture;
        Debug.Log("Image downloaded");
    }

    public void OnNewSearchResult(string value)
    {
        FirebasePlayer result = FirebasePlayer.FromJson(value);

        if (FirebaseManager.Instance.player.IsFriendOrMe(result))
            return;

        GameObject obj = null;
        PlayerInfoPrefab playerInfoPrefab;

        for (int i = 0; i < searchResultContent.childCount; i++)
        {
            if (!searchResultContent.GetChild(i).gameObject.activeSelf)
            {
                obj = searchResultContent.GetChild(i).gameObject;
                break;
            }
        }

        if (obj == null)
            obj = Instantiate(searchResultPrefab, searchResultContent) as GameObject;
        obj.SetActive(true);
        playerInfoPrefab = obj.GetComponent<PlayerInfoPrefab>();
        playerInfoPrefab.playerData = result;
    }

    void ShowNot(String title, String body)
    {
#if UNITY_ANDROID
        var notification = new AndroidNotification();
        notification.Title = title;
        notification.Text = body;
        notification.FireTime = System.DateTime.Now.AddMinutes(0);
#endif
#if UNITY_IOS

            var timeTrigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = new TimeSpan(0, 0, 0),
                Repeats = false
            };
            
            var iosnotification = new iOSNotification()
            {
                // You can optionally specify a custom Identifier which can later be 
                // used to cancel the notification, if you don't set one, an unique 
                // string will be generated automatically.
                
                Title = title,
                Body = body,
                Subtitle = title,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.NotificationPresentationOptionAlert,
                CategoryIdentifier = "category_a",
                ThreadIdentifier = "Thread" + System.DateTime.Now,
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(iosnotification);
#endif
    }


    //GameInvite
    public void OnNewFriendChangedResult(string key, string rawJsonValue)
    {
        print("isChange");
        Dictionary<string, object> collection = Json.Deserialize(rawJsonValue) as Dictionary<string, object>;
        Debug.Log("deserialized: " + collection.GetType());

        FirebasePlayer player = FirebaseManager.Instance.player;

        FriendsData friendsData = FriendsData.FromJson(rawJsonValue);
        FriendNotificationPrefab[] friendNotificationPrefabs = GameObject.FindObjectsOfType<FriendNotificationPrefab>();

        player.getFriends()[key] = friendsData;

        switch (friendsData.friendStatus)
        {
            case Constants.friends:
                //update friends list
                Debug.Log("friends update friends list");
                foreach (var item in friendNotificationPrefabs)
                {
                    if (!string.IsNullOrEmpty(item.Uid) && item.Uid.Equals(key))
                        item.gameObject.SetActive(false);
                }

                SetupFriendsUI();
                break;
            case Constants.request:
            case Constants.pending:
                //show cancel action
                Debug.Log("pending show accept, block or cancel");
                //show accept, block or cancel 
                Debug.Log("request show cancel action");
                Debug.Log("friendsData.friendStatus " + friendsData.friendStatus);

                GameObject obj = null;
                FriendNotificationPrefab friendNotificationPrefab;

                for (int i = 0; i < notificationContent.childCount; i++)
                {
                    if (!notificationContent.GetChild(i).CompareTag("GameInvite") &&
                        !notificationContent.GetChild(i).gameObject.activeSelf)
                    {
                        obj = notificationContent.GetChild(i).gameObject;
                        break;
                    }
                }

                foreach (var item in friendNotificationPrefabs)
                {
                    if (!string.IsNullOrEmpty(item.Uid) && item.Uid.Equals(key))
                        obj = item.gameObject;
                }

                if (obj == null)
                {
                    obj = Instantiate(notificationPrefab, friendsContent) as GameObject;
                }


                obj.SetActive(true);
                friendNotificationPrefab = obj.GetComponent<FriendNotificationPrefab>();
                friendNotificationPrefab.Uid = key;
                friendNotificationPrefab.friendState = friendsData.friendStatus;
//                if (string.Equals(friendsData.friendStatus, Constants.pending))
//                {
//                    ShowNot("New Friend reqeust","Click for more...");
//                }

                break;
            case Constants.blocked:
                //don't show a thing
                Debug.Log("blocked don't show a thing");

                var iteeem = GameObject.FindObjectsOfType<FriendInfoPrefab>();

                print("itemmmmmmmmmmmmmLingth = " + iteeem.Length);
                print("itemmmmmmmmmmmmm = " + iteeem);
                foreach (var item in GameObject.FindObjectsOfType<FriendInfoPrefab>())
                {
                    if (!string.IsNullOrEmpty(item.Uid) && item.Uid.Equals(key))
                        item.gameObject.SetActive(false);
                }

                break;
            default:
                Debug.Log("default " + friendsData.friendStatus);
                break;
        }
    }


    public void OnNewInviteChangedResult(string key, string rawJsonValue)
    {
        Dictionary<string, object> collection = Json.Deserialize(rawJsonValue) as Dictionary<string, object>;
        Debug.Log("deserialized: " + collection.GetType());

        FirebasePlayer player = FirebaseManager.Instance.player;

        InviteData inviteData = InviteData.FromJson(rawJsonValue);
        //FriendInvitePrefab[] friendInvitePrefabs = GameObject.FindObjectsOfType<FriendInvitePrefab>();

        Debug.Log("Invite is " + inviteData.inviteStatus);

        switch (inviteData.inviteStatus)
        {
            case Constants.accepted:
                //update friends list                
                //start game
                Debug.Log("OnNewInviteChangedResult Room key " + inviteData.roomKey);
                break;
            case Constants.request:
            case Constants.pending:
                //show cancel action
                Debug.Log("if pending show accept or cancel");
                //show accept, block or cancel 
                Debug.Log("if request show cancel action");

                GameObject obj = null;
                FriendInvitePrefab friendInvitePrefab;

                for (int i = 0; i < notificationContent.childCount; i++)
                {
                    Transform temp = notificationContent.GetChild(i);
                    if (temp.CompareTag("GameInvite") && !temp.gameObject.activeSelf)
                    {
                        obj = temp.gameObject;
                        break;
                    }
                }

                for (int i = 0; i < notificationContent.childCount; i++)
                {
                    Transform temp = notificationContent.GetChild(i);

                    if (temp.CompareTag("GameInvite"))
                    {
                        FriendInvitePrefab item = temp.GetComponent<FriendInvitePrefab>();
                        Debug.Log(item);

                        if (!string.IsNullOrEmpty(item.Uid) && item.Uid.Equals(key))
                        {
                            obj = item.gameObject;
                            break;
                        }
                    }
                }

                if (obj == null)
                    obj = Instantiate(invitePrefab, notificationContent) as GameObject;

                obj.SetActive(true);
                friendInvitePrefab = obj.GetComponent<FriendInvitePrefab>();
                friendInvitePrefab.Uid = key;
                friendInvitePrefab.inviteState = inviteData.inviteStatus;
                friendInvitePrefab.roomKey = inviteData.roomKey;

//                if (string.Equals(inviteData.inviteStatus, Constants.pending))
//                {
//                    ShowNot("A Friend invited you to game","Join him now!!...");
//                }
                
                if (inviteData.inviteStatus.Equals(Constants.request)) //// fix hear
                {
                    for (int i = 0; i < friendsContent.childCount; i++)
                    {
                        FriendInfoPrefab item = friendsContent.GetChild(i).GetComponent<FriendInfoPrefab>();
                        Debug.Log(item);
                        if (item != null && !string.IsNullOrEmpty(item.Uid) && item.Uid.Equals(key))
                            item.SetInvited();
                    }
                }

                //topNotificationManager.Show(friendInvitePrefab);

                break;
            default:
                Debug.Log("default " + inviteData.inviteStatus);
                break;
        }
    }

    public void OnFriendRemoved(string key)
    {
        Debug.Log("OnFriendRemoved");
        FriendNotificationPrefab[] friendNotificationPrefabs = GameObject.FindObjectsOfType<FriendNotificationPrefab>();
        foreach (var item in friendNotificationPrefabs)
        {
            if (!string.IsNullOrEmpty(item.Uid) && item.Uid.Equals(key))
            {
                item.gameObject.SetActive(false);
                FirebaseManager.Instance.player.getFriends().Remove(key);
            }
        }
    }

    public void OnInviteRemoved(string key)
    {
        Debug.Log("OnInviteRemoved");

        for (int i = 0; i < notificationContent.childCount; i++)
        {
            GameObject obj = notificationContent.GetChild(i).gameObject;

            if (obj.CompareTag("GameInvite"))
            {
                FriendInvitePrefab friendInvitePrefab = obj.GetComponent<FriendInvitePrefab>();

                if (!string.IsNullOrEmpty(friendInvitePrefab.Uid) && friendInvitePrefab.Uid.Equals(key))
                    friendInvitePrefab.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < friendsContent.childCount; i++)
        {
            GameObject obj = friendsContent.GetChild(i).gameObject;
            FriendInfoPrefab friendInfoPrefab = obj.GetComponent<FriendInfoPrefab>();
            if (!string.IsNullOrEmpty(friendInfoPrefab.Uid) && friendInfoPrefab.Uid.Equals(key))
                friendInfoPrefab.SetInvitable();
        }
    }

    public void LoadJoinGameScene(int gameTime)
    {
        PlayerPrefs.SetFloat("gameLength", gameTime);
        SceneManager.LoadScene(Constants.joinGameScene);
    }
    
}