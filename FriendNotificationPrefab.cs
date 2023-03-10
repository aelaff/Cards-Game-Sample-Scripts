using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class FriendNotificationPrefab : MonoBehaviour {

    

    public FirebasePlayer playerData
    {
        set
        {
            playerPhoto.texture = null;
            playerName.text = value.GetNameFormated();
            //uid = value.uid;
            url = value.photoURL;
            StopAllCoroutines();
            if (gameObject.activeInHierarchy)
                StartCoroutine("GetPlayerImageAsync", url);
            //Add on click invite listener

        }

    }
    private string url;

    public string friendState;

    public GameObject resetScript;
    private string uid;

    public string Uid
    {
        set
        {
            uid = value;
            RequestFriendData(value);
        }
        get {
            return uid;
        }
    }

    private void RequestFriendData(string uid)
    {
        FirebaseManager.Instance.root.Child(Constants.playersNode).Child(uid).GetValueAsync().ContinueWith(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Request Friend Data Is Canceled or Faulted");
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("Request Friend Data Is Completed");
                Debug.Log(task.Result.GetRawJsonValue());
                playerData = FirebasePlayer.FromJson(task.Result.GetRawJsonValue());
            }

        });
    }

    public RawImage playerPhoto;
    public Text playerName;
    public Animator animator;

    private void OnDisable()
    {
        StopAllCoroutines();

    }

    private void OnEnable()
    {
        animator.Play(friendState);
        Debug.Log("OnEnable");
        if (!string.IsNullOrEmpty(url))
            StartCoroutine("GetPlayerImageAsync", url);

    }

    public void AcceptFriend()
    {
        FirebaseManager.Instance.AcceptFriendRequest(uid);
        FriendsData friendsData = new FriendsData();
        friendsData.friendStatus = Constants.friends;
        FirebaseManager.Instance.player.getFriends()[uid] = friendsData;
        Hide();
    }

    
    public void RejectOrCancel() {
        FirebaseManager.Instance.RejectOrCancelFriendRequest(uid);
        FirebaseManager.Instance.player.getFriends().Remove(uid);
        Hide();
    }

    public void Hide()
    {
        //Hide animation
        
        gameObject.SetActive(false);
    }

    IEnumerator GetPlayerImageAsync(string url)
    {
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
}
