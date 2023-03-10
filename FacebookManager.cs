//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using Facebook.Unity;
//using Facebook.MiniJSON;

//public class FacebookManager : MonoBehaviour
//{

//    public Button FBLoginButton;
//    public RawImage userPic;
//    public Text userName;
//    public string ID;
//    public static FacebookManager Instance;
//    private Queue<string> pictureLoaderQueue;
//    private bool loadNext;
//    private bool publicProfilePermission;
//    private bool userFriendsPermission;

//    void Awake()
//    {
//        publicProfilePermission = false;
//        userFriendsPermission = false;
//        loadNext = true;
//        pictureLoaderQueue = new Queue<string>();

//        if (!Instance)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }

//    }

//    void LoginFail()
//    {
//        Login.Instance.loginPanal.SetActive(true);
//        Login.Instance.loadingPanal.SetActive(false);
//        FBLoginButton.interactable = true;
//    }

//    void HideLoginPanal()
//    {
//        Login.Instance.loginPanal.SetActive(false);
//        Login.Instance.loadingPanal.SetActive(true);
//        FBLoginButton.interactable = false;
//    }

//    public void FBLogin()
//    {
//        //FB.GraphApiVersion = "v3.0";

//        HideLoginPanal();
//        if (!FB.IsInitialized)
//        {
//            FB.Init(this.OnInitComplete, this.OnHideUnity);
//        }
//        else
//        {
//            OnInitComplete();
//        }
//    }

//    public void FBLogOut()
//    {
//        FB.LogOut();
//    }

//    private void OnInitComplete()
//    {
//        string logMessage = string.Format(
//                                "OnInitCompleteCalled IsLoggedIn='{0}' IsInitialized='{1}'",
//                                FB.IsLoggedIn,
//                                FB.IsInitialized);

//        Debug.Log(logMessage);
//        Debug.Log(FB.GraphApiVersion);
//        if (AccessToken.CurrentAccessToken != null)
//        {
//            Debug.Log(AccessToken.CurrentAccessToken.ToString());
//            FB.LogInWithReadPermissions(new List<string>() { "public_profile", "user_age_range", "user_friends" }, this.OnLoginComplete);
//        }
//        else
//        {
//            Debug.Log("Access Token is needed");
//            FB.LogInWithReadPermissions(new List<string>() { "public_profile", "user_age_range", "user_friends" }, this.OnLoginComplete);
//        }
//        StopAllCoroutines();
//        StartCoroutine("picturesLoader");
//    }

//    private void OnHideUnity(bool isGameShown)
//    {
//        Debug.Log("Is game shown: " + isGameShown);
//    }

//    private void OnLoginComplete(IResult result)
//    {
//        if (result == null)
//        {
//            Debug.Log("Null response ");


//        }
//        if (!string.IsNullOrEmpty(result.Error))
//        {
//            Debug.Log("Error response " + result.Error);
//            Debug.Log("Error response " + result.RawResult);
//            LoginFail();


//        }
//        else if (result.Cancelled)
//        {
//            Debug.Log("Cancelled response " + result.RawResult);
//            LoginFail();

//        }
//        else if (!string.IsNullOrEmpty(result.RawResult))
//        {
//            Debug.Log("OK response " + result.RawResult);

//            Dictionary<string, object> response = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
//            //Dictionary<string,object> friends = deserializeResult ["friends"] as Dictionary<string,object>;
//            List<object> granted_permissions = new List<object>();
//            List<object> declined_permissions = new List<object>();
//            //			if (response.ContainsKey ("granted_permissions")) {
//            //				granted_permissions = (List<object>)response ["granted_permissions"];
//            //
//            //			} else 
//            if (response.ContainsKey("declined_permissions"))
//            {
//                if (response["declined_permissions"] is List<object>)
//                {
//                    publicProfilePermission = true;
//                    userFriendsPermission = true;
//                    declined_permissions = (List<object>)response["declined_permissions"];
//                    for (int i = 0; i < declined_permissions.Count; i++)
//                    {
//                        if (((string)declined_permissions[i]) == "public_profile")
//                            publicProfilePermission = false;
//                        if (((string)declined_permissions[i]) == "user_friends")
//                            userFriendsPermission = false;
//                    }
//                }
//                else if (response["declined_permissions"] is string)
//                {
//                    publicProfilePermission = true;
//                    userFriendsPermission = true;
//                    string declined_permissionsString = (string)response["declined_permissions"];
//                    declined_permissions = new List<object>(declined_permissionsString.Split(','));
//                    for (int i = 0; i < declined_permissions.Count; i++)
//                    {
//                        if (((string)declined_permissions[i]) == "public_profile")
//                            publicProfilePermission = false;
//                        if (((string)declined_permissions[i]) == "user_friends")
//                            userFriendsPermission = false;
//                    }
//                }
//            }
//            //			for (int i = 0; i < granted_permissions.Count; i++) {
//            //				if (((string)granted_permissions [i]) == "public_profile")
//            //					publicProfilePermission = true;
//            //				if (((string)granted_permissions [i]) == "user_friends")
//            //					userFriendsPermission = true;
//            //			}




//            PlayerData playerData = GameObject.Find("AllData").GetComponent<PlayerData>();
//            playerData.friends = new List<Friend>();

//            if (publicProfilePermission)
//            {
//                FBGetBasicInfo();
//            }
//            else
//            {
//                Debug.Log("Public Profile Permission must be granted");

//                if (FB.IsLoggedIn)
//                {
//                    FB.LogOut();
//                }
//                OnInitComplete();

//            }

//        }
//        else
//        {
//            Debug.Log("Empty response ");
//            LoginFail();
//        }


//    }

//    private void OnBasicInfo(IResult result)
//    {
//        Debug.Log("OnBasicInfo");

//        if (result == null)
//        {
//            Debug.Log("Null response ");
//            LoginFail();
//        }

//        if (!string.IsNullOrEmpty(result.Error))
//        {
//            Debug.Log("Error response " + result.Error);
//            LoginFail();
//        }
//        else if (result.Cancelled)
//        {
//            Debug.Log("Cancelled response " + result.RawResult);
//            LoginFail();
//        }
//        else if (!string.IsNullOrEmpty(result.RawResult))
//        {
//            Debug.Log("OK response " + result.RawResult);
//            foreach (var item in result.ResultDictionary)
//            {
//                Debug.Log(item.Key + ":" + item.Value);
//                if (item.Key.Equals("id"))
//                {
//                    ID = item.Value.ToString();

//                    SaveLoad.SetString("FBID", ID);


//                }

//                if (item.Key.Equals("name"))
//                {
//                    PlayerData playerData = GameObject.Find("AllData").GetComponent<PlayerData>();
//                    playerData.FBName = item.Value.ToString();
//                    //userName.text = "Wellcome " + 
//                }
//            }
//            if (publicProfilePermission)
//            {
//                FB.API("/me/picture", HttpMethod.GET, OnUserPicture);
//            }
//            else
//            {
//                Debug.Log("Public Profile Permission must be granted");

//                if (FB.IsLoggedIn)
//                {
//                    FB.LogOut();
//                }
//                OnInitComplete();
//            }

//        }
//        else
//        {
//            LoginFail();
//            Debug.Log("Empty response ");
//        }
//    }

//    private void OnUserPicture(IGraphResult result)
//    {
//        Debug.Log("OnUserPicture");
//        if (string.IsNullOrEmpty(result.Error) && result.Texture != null)
//        {
//            Debug.Log("OK response");
//            PlayerData playerData = GameObject.Find("AllData").GetComponent<PlayerData>();
//            playerData.playerPic = result.Texture;

//            PlayerPrefs.SetString(SaveLoad.GetString("FBID"), System.Convert.ToBase64String(ImageConversion.EncodeToJPG(result.Texture)));
//            if (userFriendsPermission)
//            {
//                FBGetFriendsInfo();
//            }
//            else
//            {
//                Debug.Log("User Friends Permission must be granted");

//                if (FB.IsLoggedIn)
//                {
//                    FB.LogOut();
//                }
//                OnInitComplete();
//            }

//        }
//        else if (result.Cancelled)
//        {
//            LoginFail();
//            Debug.Log("Cancelled response " + result.RawResult);
//        }
//        else if (!string.IsNullOrEmpty(result.RawResult))
//        {
//            Debug.Log("OK response " + result.RawResult);
//            foreach (var item in result.ResultDictionary)
//            {
//                Debug.Log(item.Key + ":" + item.Value);
//            }
//            if (userFriendsPermission)
//            {
//                FBGetFriendsInfo();

//            }
//            else
//            {
//                Debug.Log("User Friends Permission must be granted");

//                if (FB.IsLoggedIn)
//                {
//                    FB.LogOut();
//                }
//                OnInitComplete();
//            }

//        }
//        else
//        {
//            LoginFail();
//            Debug.Log("Empty response ");
//        }



//    }

//    public void FBGetFriendsInfo()
//    {
//        FB.API("/me/friends", HttpMethod.GET, OnFriendsInfo);
//    }

//    private void GetFBUserPic(string id)
//    {
//        FB.API("/" + id + "/picture", HttpMethod.GET, OnFBUserPic);
//    }

//    void OnFBUserPic(IGraphResult result)
//    {
//        Debug.Log("OnFBUserPic");
//        if (string.IsNullOrEmpty(result.Error) && result.Texture != null)
//        {
//            Debug.Log("OK response");

//            PlayerPrefs.SetString(pictureLoaderQueue.Dequeue(), System.Convert.ToBase64String(ImageConversion.EncodeToJPG(result.Texture)));
//            loadNext = true;


//        }
//        else if (result.Cancelled)
//        {
//            Debug.Log("Cancelled response " + result.RawResult);
//        }
//        else if (!string.IsNullOrEmpty(result.RawResult))
//        {
//            Debug.Log("OK response " + result.RawResult);
//            foreach (var item in result.ResultDictionary)
//            {
//                Debug.Log(item.Key + ":" + item.Value);
//            }
//        }
//        else
//        {
//            Debug.Log("Empty response ");
//        }


//    }

//    public void RegisterIDForPictureLoad(string id)
//    {
//        pictureLoaderQueue.Enqueue(id);
//    }

//    IEnumerator picturesLoader()
//    {
//        yield return new WaitForEndOfFrame();

//        while (true)
//        {

//            if (pictureLoaderQueue.Count > 0 && loadNext)
//            {
//                string id = pictureLoaderQueue.Peek();
//                if (!PlayerPrefs.HasKey(id))
//                {
//                    loadNext = false;
//                    GetFBUserPic(pictureLoaderQueue.Peek());
//                }
//                else
//                {
//                    pictureLoaderQueue.Dequeue();
//                    loadNext = true;
//                }
//            }

//            yield return new WaitForEndOfFrame();

//        }

//    }

//    private void OnFriendsInfo(IResult result)
//    {
//        Debug.Log("OnFriendsInfo");

//        if (result == null)
//        {
//            Debug.Log("Null response ");
//            LoginFail();

//        }

//        if (!string.IsNullOrEmpty(result.Error))
//        {
//            Debug.Log("Error response " + result.Error);
//            LoginFail();

//        }
//        else if (result.Cancelled)
//        {
//            Debug.Log("Cancelled response " + result.RawResult);
//            LoginFail();

//        }
//        else if (!string.IsNullOrEmpty(result.RawResult))
//        {
//            Debug.Log("OK response " + result.RawResult);
//            foreach (var item in result.ResultDictionary)
//            {
//                //Debug.Log (item.Key + ":"	 + item.Value);
//                Dictionary<string, object> friends = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
//                //Dictionary<string,object> friends = deserializeResult ["friends"] as Dictionary<string,object>;
//                List<object> data = (List<object>)friends["data"];
//                PlayerData playerData = GameObject.Find("AllData").GetComponent<PlayerData>();
//                playerData.friends = new List<Friend>();

//                for (int i = 0; i < data.Count; i++)
//                {
//                    Dictionary<string, object> friend = data[i] as Dictionary<string, object>;
//                    Friend f = new Friend();
//                    f.id = (string)friend["id"];
//                    f.name = (string)friend["name"];
//                    playerData.friends.Add(f);
//                }
//                Debug.Log(data.Count);
//                Debug.Log(playerData.friends);



//            }
//            if (!SaveLoad.GetBool("Registered"))
//                Login.Instance.FBRegister();
//            else
//                Login.Instance.Init();

//        }
//        else
//        {
//            LoginFail();

//            Debug.Log("Empty response ");
//        }
//    }

//    public void FBGetBasicInfo()
//    {
//        FB.API("/me?fields,name", HttpMethod.GET, OnBasicInfo);

//    }

//    public void Invite()
//    {
//        FB.AppRequest(
//            "Come play this great game!",
//            null, new List<object>() { "app_non_users" }, null, null, null, null,
//            delegate (IAppRequestResult result)
//            {
//                Debug.Log(result.RawResult);
//            }
//        );
//    }

//    public void Gift(string id)
//    {
//        FB.AppRequest(
//            "Here is a 25 coins gift!",
//            new List<string>() { id }, null, null, null, "GIFT25", "Free Gifts!!",
//            delegate (IAppRequestResult result)
//            {
//                Debug.Log(result.RawResult);
//            }
//        );
//    }

//    public void Gift()
//    {
//        FB.AppRequest(
//            "Here is a 25 coins gift!",
//            null, new List<object>() { "app_users" }, null, null, "GIFT25", "Free Gifts!!",
//            delegate (IAppRequestResult result)
//            {
//                Debug.Log(result.RawResult);
//            }
//        );
//    }

//}
