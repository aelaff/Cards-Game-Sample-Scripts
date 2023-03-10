using Facebook.MiniJSON;
using Facebook.Unity;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using Google;
using System.Collections.Generic;
using TwitterKit.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Messaging;
using System.Threading.Tasks;
using System;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class FirebaseManager : MonoBehaviour
{
    private FirebaseAuth auth;
    public DatabaseReference root;
    public FirebasePlayer player;
    public GameObject LoginButtonsPanal, LoadingIndicator;
    public static FirebaseManager Instance;
    bool initFirebase;


    void Awake()
    {
        #if UNITY_ANDROID
        var c = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Default Channel",
            Importance = Importance.High,
            Description = "Generic notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(c);
        #endif
        
       
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(Instance.gameObject);
            Instance = this;
        }

        initFirebase = false;
        DontDestroyOnLoad(this);
        CheckAndFixDependencies();
    }

    

    // Use this for initialization
    System.Collections.IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        while (!initFirebase)
        {
            yield return new WaitForEndOfFrame();
        }

        #if UNITY_IOS
        using (var req = new AuthorizationRequest(AuthorizationOption.AuthorizationOptionAlert | AuthorizationOption.AuthorizationOptionBadge, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            };

            string res = "\n RequestAuthorization: \n";
            res += "\n finished: " + req.IsFinished;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            Debug.Log(res);
        }
        #endif

        
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        ShowLoginForm();
        auth = FirebaseAuth.DefaultInstance;

        SetupDB();
        SetupFCM();
        TryToAutoLogin();
    }

    private void TryToAutoLogin()
    {
        string lastLoginProvider = PlayerPrefs.GetString(Constants.LastLoginProvider, "");
        switch (lastLoginProvider)
        {
                case "FB":
                    LoadingIndicator.SetActive(true);
                    LoginWithFacebook();
                    break;
                case "TW":
                    LoadingIndicator.SetActive(true);
                    LoginWithTwitter();
                    break;
                case "GO":
                    LoadingIndicator.SetActive(true);
                    LoginWithGoogle();
                    break;
                default:
                    ShowLoginForm();
                    break;
        }
    }


    void SetupDB()
    {
        // Set this before calling into the realtime database.
#if UNITY_EDITOR
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://solitaire-2d49c.firebaseio.com/");
        FirebaseApp.DefaultInstance.SetEditorP12FileName("solitaire-2d49c-7cd6a5e9c633.p12");
        FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail("solitaire-2d49c@appspot.gserviceaccount.com");
        FirebaseApp.DefaultInstance.SetEditorP12Password("notasecret");
#endif


        root = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void SetupFCM()
    {
        //FirebaseMessaging.TokenReceived -= OnTokenReceived;
        //FirebaseMessaging.MessageReceived -= OnMessageReceived;
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
    }


    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        switch (e.Message.MessageType)
        {
            case "send_event":

                Debug.Log("send_event");
                break;
            case "deleted_messages":
                Debug.Log("deleted_messages");
                break;
            case "send_error":
                Debug.Log("send_error");
                break;
            default:
                Debug.Log("OnMessageReceived " + e.Message.MessageType);
                break;
        }

        string title = e.Message.Data.ContainsKey("title") ? e.Message.Data["title"] : "";
        string body = e.Message.Data.ContainsKey("body") ? e.Message.Data["body"] : "";
        string action = e.Message.Data.ContainsKey("action") ? e.Message.Data["action"] : "";

        Debug.Log("From: " + e.Message.From);
        Debug.Log("Message ID: " + e.Message.MessageId);
        Debug.Log("Message Data: " + string.Format("title:{0}, body:{1}, action:{2}", title, body, action));


        if (!e.Message.NotificationOpened)
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

        Debug.Log("Message Notification Opened: " + e.Message.NotificationOpened);
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("OnTokenReceived: " + token.Token);

        PlayerPrefs.SetString(Constants.tokenNode, token.Token);
        RegisterToken();
    }

    public void RegisterToken()
    {
        string token = PlayerPrefs.GetString(Constants.tokenNode, "");

        if (player != null && player.uid != null && !string.IsNullOrEmpty(player.uid) && !string.IsNullOrEmpty(token))
        {
            Dictionary<string, object> updates = new Dictionary<string, object>();
            string platform = "";

#if UNITY_IOS
                platform = "IOS";
#endif
#if UNITY_ANDROID
            platform = "ANDROID";
#endif
            Debug.LogError("Registering Token");
            Debug.Log("Platform " + platform);
            updates[string.Format("{0}/{1}/{2}", Constants.FCMTokensNode, player.uid, Constants.tokenNode)] = token;
            updates[string.Format("{0}/{1}/{2}", Constants.FCMTokensNode, player.uid, Constants.platformNode)] =
                platform;
            root.UpdateChildrenAsync(updates).ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Register Token Is Canceled or Faulted");
                    return;
                }

                if (task.IsCompleted)
                {
                    Debug.Log("Register Token Request Is Completed");
                }
            });
        }
        else
        {
            Debug.Log("Player or token is Empty");
        }
    }

    public void LoginWithFacebook()
    {
        LoginButtonsPanal.SetActive(false);
        if (!FB.IsInitialized)
        {
            FB.Init(this.OnInitComplete, this.OnHideUnity);
        }
        else
        {
            OnInitComplete();
        }
    }

    private void OnInitComplete()
    {
        string logMessage = string.Format(
            "OnInitCompleteCalled IsLoggedIn='{0}' IsInitialized='{1}'",
            FB.IsLoggedIn,
            FB.IsInitialized);

        Debug.Log(logMessage);
        Debug.Log(FB.GraphApiVersion);
        if (AccessToken.CurrentAccessToken != null)
        {
            Debug.Log(AccessToken.CurrentAccessToken.ToString());
            FB.LogInWithReadPermissions(new List<string>() {"public_profile"},
                this.OnLoginComplete);
        }
        else
        {
            Debug.Log("Access Token is needed");
            FB.LogInWithReadPermissions(new List<string>() {"public_profile"},
                this.OnLoginComplete);
        }
    }

    private void OnLoginComplete(IResult result)
    {
        if (result == null)
        {
            Debug.Log("Null response ");
            LoginFail();
        }

        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("Error response " + result.Error);
            Debug.Log("Error response " + result.RawResult);
            LoginFail();
        }
        else if (result.Cancelled)
        {
            Debug.Log("Cancelled response " + result.RawResult);
            LoginFail();
        }
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
            Debug.Log("OK response " + result.RawResult);
            Dictionary<string, object> response = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
            string token = "";
            bool foundIt = response.TryGetValue("access_token", out token);
            Debug.Log(token);
            if (foundIt && !string.IsNullOrEmpty(token))
            {
                PlayerPrefs.SetString(Constants.LastLoginProvider, "FB");
                SigninWithCredential(token);
            }
            else
            {
                LoginFail();
                Debug.Log("Token is not found or it's empty");
            }
        }
        else
        {
            Debug.Log("Empty response ");
            LoginFail();
        }
    }

    private void LoginFail()
    {
        Debug.Log("Login fail");
        PlayerPrefs.SetString(Constants.LastLoginProvider, "");
        ShowLoginForm();
        LoadingIndicator.SetActive(false);
    }

    private void OnHideUnity(bool isGameShown)
    {
        Debug.Log("Is game shown: " + isGameShown);
    }

    public void LoginWithTwitter()
    {
        LoginButtonsPanal.SetActive(false);
        UnityEngine.Debug.Log("startLogin()");
        // To set API key navigate to tools->Twitter Kit
        Twitter.Init();
        Twitter.LogIn(LoginCompleteWithEmail, (ApiError error) =>
        {
            UnityEngine.Debug.Log(error.message);
            LoginFail();
        });
    }

    public void LogOut()
    {
        auth.SignOut();
    }

    void ShowLoginForm()
    {
        LoginButtonsPanal.SetActive(true);
    }

    void ShowLoading()
    {
        LoadingIndicator.SetActive(true);
    }

    public void LoginCompleteWithEmail(TwitterSession session)
    {
        // To get the user's email address you must have "Request email addresses from users" enabled on https://apps.twitter.com/ (Permissions -> Additional Permissions)
        UnityEngine.Debug.Log("LoginCompleteWithEmail()");
        PlayerPrefs.SetString(Constants.LastLoginProvider, "TW");
        SigninWithCredential(session.authToken);
    }


    public void LoginWithGoogle()
    {
        LoginButtonsPanal.SetActive(false);

        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            RequestEmail = true,
            // Copy this value from the google-service.json file.
            // oauth_client with type == 3
            //android 298767783239-klcn998nhbhqk0qcj9am6454bbg2bd34.apps.googleusercontent.com
            //ios 298767783239-51i7ok010fgfn1kpimlloogkisvjm90c.apps.googleusercontent.com
            WebClientId = "298767783239-51i7ok010fgfn1kpimlloogkisvjm90c.apps.googleusercontent.com"
        };

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        signIn.ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                signInCompleted.SetCanceled();
                LoginFail();
            }
            else if (task.IsFaulted)
            {
                signInCompleted.SetException(task.Exception);
                LoginFail();
            }
            else
            {
                Credential credential =
                    GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>) task).Result.IdToken, null);
                PlayerPrefs.SetString(Constants.LastLoginProvider, "GO");
                auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                {
                    if (authTask.IsCanceled)
                    {
                        signInCompleted.SetCanceled();
                        LoginFail();
                    }
                    else if (authTask.IsFaulted)
                    {
                        signInCompleted.SetException(authTask.Exception);
                        LoginFail();
                    }
                    else
                    {
                        signInCompleted.SetResult(((Task<FirebaseUser>) authTask).Result);
                        ShowUserInfoPanal();
                    }
                });
            }
        });
    }

    private void SigninWithCredential(AuthToken authToken)
    {
        Credential credential = TwitterAuthProvider.GetCredential(authToken.token, authToken.secret);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                LoginFail();
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                LoginFail();
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            ShowUserInfoPanal();
        });
    }

    void SigninWithCredential(string accessToken)
    {
        Credential credential = FacebookAuthProvider.GetCredential(accessToken);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                LoginFail();
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                LoginFail();
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            ShowUserInfoPanal();
        });
    }

    void ShowUserInfoPanal()
    {
        Debug.Log("ShowUserInfoPanal Start");
        player = new FirebasePlayer();

        foreach (var user in auth.CurrentUser.ProviderData)
        {
            player.fullName = user.DisplayName.ToLower();
            player.uid = auth.CurrentUser.UserId;
            player.photoURL = user.PhotoUrl.OriginalString;
        }


        UpdatePlayerData(player);
        Debug.Log("ShowUserInfoPanal End");
    }

    private void UpdatePlayerData(FirebasePlayer player)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>();
        updates[string.Format("{0}/{1}/{2}", Constants.playersNode, player.uid, Constants.fullNameNode)] =
            player.fullName;
        updates[string.Format("{0}/{1}/{2}", Constants.playersNode, player.uid, Constants.idNode)] = player.uid;
        updates[string.Format("{0}/{1}/{2}", Constants.playersNode, player.uid, Constants.photoURLNode)] =
            player.photoURL;

        root.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Update Player Data Is Canceled or Faulted");
                LoginFail();
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Update Player Data Is Completed");
                GetPlayerData();
            }
        });
    }

    private void GetPlayerData()
    {
        root.Child(string.Format("{0}/{1}", Constants.playersNode, player.uid)).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Get Player Data Is Canceled or Faulted");
                LoginFail();
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Get Player Data Is Completed");
                Debug.Log(task.Result.GetRawJsonValue());
                player = FirebasePlayer.FromJson(task.Result.GetRawJsonValue());
                GetPlayerFriends();
                RegisterToken();
            }
        });
    }

    private void GetPlayerFriends()
    {
        root.Child(string.Format("{0}/{1}", Constants.friends, player.uid)).OrderByChild(Constants.friendStatusNode)
            .EqualTo(Constants.friends).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Get Player Friends Is Canceled or Faulted");
                    LoginFail();
                    return;
                }

                if (task.IsCompleted)
                {
                    Dictionary<string, FriendsData> friends;
                    Debug.Log("Get Player Friends Is Completed");
                    if (task.Result != null && task.Result.Exists &&
                        !string.IsNullOrEmpty(task.Result.GetRawJsonValue()))
                    {
                        Dictionary<string, object> dict =
                            Json.Deserialize(task.Result.GetRawJsonValue()) as Dictionary<string, object>;
                        Debug.Log("deserialized: " + dict.GetType());
                        Debug.Log(task.Result.GetRawJsonValue());

                        friends = player.getFriends();
                        friends.Clear();
                        foreach (var item in dict)
                        {
                            friends.Add(item.Key, FriendsData.FromJson(Json.Serialize(item.Value)));
                        }
                    }
                    else
                    {
                        friends = player.getFriends();
                        friends = new Dictionary<string, FriendsData>(0);
                    }

                    LoadStartScene();
                }
            });
    }

    private void LoadStartScene()
    {
        SceneManager.LoadScene(Constants.startScene);
    }


    public void SearchFor(string value)
    {
        root.Child(Constants.playersNode).ChildAdded -= FirebaseSearchResult;
        root.Child(Constants.playersNode).OrderByChild(Constants.fullNameNode).StartAt(value).EndAt(value + "\uf8ff")
            .LimitToLast(50).ChildAdded += FirebaseSearchResult;
    }

    public void CancelSearch()
    {
        root.Child(Constants.playersNode).ChildAdded -= FirebaseSearchResult;
    }

    public void ListenForFriendsChanges()
    {
        root.Child(Constants.friendsNode).Child(player.uid).ChildAdded -= FriendsChanged;
        root.Child(Constants.friendsNode).Child(player.uid).ChildChanged -= FriendsChanged;
        root.Child(Constants.friendsNode).Child(player.uid).ChildRemoved -= FriendsRemoved;

        root.Child(Constants.friendsNode).Child(player.uid).ChildAdded += FriendsChanged;
        root.Child(Constants.friendsNode).Child(player.uid).ChildChanged += FriendsChanged;
        root.Child(Constants.friendsNode).Child(player.uid).ChildRemoved += FriendsRemoved;
    }

    private void FriendsRemoved(object sender, ChildChangedEventArgs e)
    {
        Debug.Log("Friends Removed");

        if (e.DatabaseError != null)
        {
            Debug.Log("Database Error FriendsRemoved");
            return;
        }

        if (StartUIInit.Instance != null)
        {
            Debug.Log(e.Snapshot.Key);
            if (e.Snapshot != null)
            {
                StartUIInit.Instance.OnFriendRemoved(e.Snapshot.Key); //////////////////////

            }
        }
    }

    public void ListenForInvitesChanges()
    {
        root.Child(Constants.invitesNode).Child(player.uid).ChildAdded -= InvitesChanged;
        root.Child(Constants.invitesNode).Child(player.uid).ChildChanged -= InvitesChanged;
        root.Child(Constants.invitesNode).Child(player.uid).ChildRemoved -= InvitesRemoved;

        root.Child(Constants.invitesNode).Child(player.uid).ChildAdded += InvitesChanged;
        root.Child(Constants.invitesNode).Child(player.uid).ChildChanged += InvitesChanged;
        root.Child(Constants.invitesNode).Child(player.uid).ChildRemoved += InvitesRemoved;
    }

    public void InvitesRemoved(object sender, ChildChangedEventArgs e)
    {
        if (StartUIInit.Instance != null)
        {
            Debug.Log(e.Snapshot.GetRawJsonValue());
            if (e.Snapshot != null)
            {
                StartUIInit.Instance.OnInviteRemoved(e.Snapshot.Key);
            }
        }
    }

    private void FriendsChanged(object sender, ChildChangedEventArgs e)
    {
        Debug.Log("Friends Changed");

        if (e.DatabaseError != null)
        {
            Debug.Log("Database Error FriendsChanged");
            return;
        }

        if (StartUIInit.Instance != null)
        {
            Debug.Log(e.Snapshot.GetRawJsonValue());
            if (e.Snapshot != null)
            {
                StartUIInit.Instance.OnNewFriendChangedResult(e.Snapshot.Key, e.Snapshot.GetRawJsonValue()); //////////////////////
                
            }
        }
    }

    private void InvitesChanged(object sender, ChildChangedEventArgs e)
    {
        Debug.Log("Invites Changed");

        if (e.DatabaseError != null)
        {
            Debug.Log("Database Error InvitesChanged");
            return;
        }

        if (StartUIInit.Instance != null)
        {
            Debug.Log(e.Snapshot.GetRawJsonValue());
            if (e.Snapshot != null)
            {
                StartUIInit.Instance.OnNewInviteChangedResult(e.Snapshot.Key, e.Snapshot.GetRawJsonValue());
            }
            else
            {
                StartUIInit.Instance.OnInviteRemoved(e.Snapshot.Key);
            }
        }
    }


    private void FirebaseSearchResult(object sender, ChildChangedEventArgs e)
    {
        Debug.Log("New FirebaseSearchResult");

        if (e.DatabaseError != null)
        {
            Debug.Log("Database Error FirebaseSearchResult");
            return;
        }

        if (e.Snapshot != null)
        {
            Debug.Log(e.Snapshot.GetRawJsonValue());
            if (StartUIInit.Instance != null)
                StartUIInit.Instance.OnNewSearchResult(e.Snapshot.GetRawJsonValue());
        }
    }

    public void SendFriendRequest(string newFriendUid)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>();
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.friendsNode, player.uid, newFriendUid,
                Constants.friendStatusNode)] = Constants.request;
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.friendsNode, newFriendUid, player.uid,
                Constants.friendStatusNode)] = Constants.pending;
        root.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Send Friend Request Is Canceled or Faulted");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Send Friend Request Is Completed");
            }
        });
    }

    public void SendInviteRequest(string newFriendUid, string roomKey)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>();
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.invitesNode, player.uid, newFriendUid,
                Constants.inviteStatusNode)] = Constants.request;
        updates[
                string.Format("{0}/{1}/{2}/{3}", Constants.invitesNode, player.uid, newFriendUid,
                    Constants.roomKeyNode)] =
            roomKey;
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.invitesNode, newFriendUid, player.uid,
                Constants.inviteStatusNode)] = Constants.pending;
        updates[
                string.Format("{0}/{1}/{2}/{3}", Constants.invitesNode, newFriendUid, player.uid,
                    Constants.roomKeyNode)] =
            roomKey;
        root.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Send Invite Request Is Canceled or Faulted");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Send Invite Request Is Completed");
            }
        });
    }


    public void RejectOrCancelInviteRequest(string invitedFriendUid)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>();
        updates[string.Format("{0}/{1}/{2}/", Constants.invitesNode, player.uid, invitedFriendUid)] = null;
        updates[string.Format("{0}/{1}/{2}/", Constants.invitesNode, invitedFriendUid, player.uid)] = null;
        root.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("RejectOrCancel Invite Request Is Canceled or Faulted");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("RejectOrCancel Invite Request Is Completed");
            }
        });
    }

    public void AcceptInviteRequest(string invitedFriendUid)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>();
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.invitesNode, player.uid, invitedFriendUid,
                Constants.inviteStatusNode)] = Constants.accepted;

        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.invitesNode, invitedFriendUid, player.uid,
                Constants.inviteStatusNode)] = Constants.accepted;

        root.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Accept Invite Request Is Canceled or Faulted");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Accept Invite Request Is Completed");
            }
        });
    }

    public void AcceptFriendRequest(string newFriendUid)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>();
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.friendsNode, player.uid, newFriendUid,
                Constants.friendStatusNode)] = Constants.friends;
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.friendsNode, newFriendUid, player.uid,
                Constants.friendStatusNode)] = Constants.friends;
        root.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Accept Friend Request Is Canceled or Faulted");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Accept Friend Request Is Completed");
            }
        });
    }

    public void RejectOrCancelFriendRequest(string newFriendUid)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>();
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.friendsNode, player.uid, newFriendUid,
                Constants.friendStatusNode)] = null;
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.friendsNode, newFriendUid, player.uid,
                Constants.friendStatusNode)] = null;
        root.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Reject Or Cancel Friend Request Is Canceled or Faulted");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Reject Or Cancel Friend Request Is Completed");
            }
        });
    }

    public void BlockFriend(string newFriendUid)
    {
        player.getFriends()[newFriendUid] = new FriendsData()
        {
            friendStatus = Constants.blocked
    };
        Dictionary<string, object> updates = new Dictionary<string, object>();
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.friendsNode, player.uid, newFriendUid,
                Constants.friendStatusNode)] = Constants.blocked;
        updates[
            string.Format("{0}/{1}/{2}/{3}", Constants.friendsNode, newFriendUid, player.uid,
                Constants.friendStatusNode)] = Constants.blocked;
        root.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Block Friend Is Canceled or Faulted");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Block Friend Is Completed");
            }
        });
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Debug.Log("pause");
        }
        else
        {
            Debug.Log("resume");
        }
    }


    void CheckAndFixDependencies()
    {
        FirebaseApp.CheckDependenciesAsync().ContinueWith(checkTask =>
        {
            // Peek at the status and see if we need to try to fix dependencies.
            DependencyStatus status = checkTask.Result;
            if (status != DependencyStatus.Available)
            {
                return FirebaseApp.FixDependenciesAsync().ContinueWith(t =>
                {
                    return FirebaseApp.CheckDependenciesAsync();
                }).Unwrap();
            }
            else
            {
                return checkTask;
            }
        }).Unwrap().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                initFirebase = true;
                // TODO: Continue with Firebase initialization.
                Debug.Log(
                    "all Firebase dependencies found");
            }
            else
            {
                Debug.LogError(
                    "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    public void RemoveScore()
    {
        player.score -= 10;
        player.score = Mathf.Clamp(player.score, 0, int.MaxValue);
        Dictionary<string, object> updates = new Dictionary<string, object>();
        updates[
            string.Format("{0}/{1}/{2}", Constants.playersNode, player.uid, Constants.scoreNode)] = player.score;
        
        root.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("RemoveScore Is Canceled or Faulted");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("RemoveScore Is Completed");
            }
        });
    }
    
    public void AddScore()
    {
        player.score += 35;
        player.score = Mathf.Clamp(player.score, 0, int.MaxValue);
        player.wins += 1;
        Dictionary<string, object> updates = new Dictionary<string, object>();
        updates[
            string.Format("{0}/{1}/{2}", Constants.playersNode, player.uid, Constants.scoreNode)] = player.score;
        updates[
            string.Format("{0}/{1}/{2}", Constants.playersNode, player.uid, Constants.winsNode)] = player.wins;
        
        root.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("AddScore Is Canceled or Faulted");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("AddScore Is Completed");
            }
        });
    }
}