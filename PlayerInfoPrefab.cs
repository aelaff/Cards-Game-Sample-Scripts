using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerInfoPrefab : MonoBehaviour {

    public FirebasePlayer playerData {
        set {
            playerPhoto.texture = null;
            playerName.text = value.GetNameFormated();
            playerLocation.text = value.location ?? "Unknown";
            uid = value.uid;
            url = value.photoURL;
            StopAllCoroutines();
            if (gameObject.activeInHierarchy)
                StartCoroutine("GetPlayerImageAsync", url);
            addFriendButton.onClick.RemoveAllListeners();
            //Add on click invite listener
        }
        
    }
    private string url;

    private string uid;
    public RawImage playerPhoto;
    public Text playerName,playerLocation;
    public Button addFriendButton;

    private void OnDisable()
    {
      StopAllCoroutines();
      addFriendButton.onClick.RemoveAllListeners();

    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        if (!string.IsNullOrEmpty(url))
            StartCoroutine("GetPlayerImageAsync", url);

    }

    public void AddFriend() {
        FirebaseManager.Instance.SendFriendRequest(uid);
        FriendsData friendsData = new FriendsData();
        friendsData.friendStatus = Constants.request;
        FirebaseManager.Instance.player.getFriends()[uid] = friendsData;
        Hide();
    }

    public void Hide() {
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
