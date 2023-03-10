using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FriendInfoPrefab : MonoBehaviour
{



    public FirebasePlayer playerData
    {
        set
        {
            playerPhoto.texture = null;
            playerName.text = value.GetNameFormated();
            playerLocation.text = value.location ?? "Unknown";
            //uid = value.uid;
            StopAllCoroutines();
            url = value.photoURL;
            if (gameObject.activeInHierarchy)
                StartCoroutine("GetPlayerImageAsync", url);
            //Add on click invite listener
        }

    }
    public string uid
    {
        set
        {
            Uid = value;
            RequestFriendData(value);
        }
    }

    public string Uid;
    private string inviteState;
    private string url;


    private void OnEnable()
    {
        Debug.Log("OnEnable");
        if (!string.IsNullOrEmpty(url))
            StartCoroutine("GetPlayerImageAsync", url);
        if (!string.IsNullOrEmpty(inviteState) && gameObject.activeInHierarchy)
            animator.Play(inviteState);
        

    }



    private void RequestFriendData(string uid)
    {
        FirebaseManager.Instance.root.Child(Constants.playersNode).Child(uid).GetValueAsync().ContinueWith(task =>
        {
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
    public Animator animator;

    public Text playerName, playerLocation;

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void InviteFriend()
    {
        string roomKey = Uid + FirebaseManager.Instance.player.uid + (UnityEngine.Random.value * 10);
        FirebaseManager.Instance.SendInviteRequest(Uid,roomKey);
        LoadJoinGameScene(roomKey);
    }

    public void LoadJoinGameScene(string roomKey)
    {
        PlayerPrefs.SetString(Constants.INVITED,roomKey);
        SceneManager.LoadScene(Constants.joinGameScene);
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

    public void SetInvitable()
    {
        this.inviteState = "InvitableFriend";
        if (!string.IsNullOrEmpty(inviteState) && gameObject.activeInHierarchy)
            animator.Play(inviteState);
    }

    public void SetInvited()
    {
        this.inviteState = "InvitedFriend";
        if (!string.IsNullOrEmpty(inviteState) && gameObject.activeInHierarchy)
            animator.Play(inviteState);
    }
}
