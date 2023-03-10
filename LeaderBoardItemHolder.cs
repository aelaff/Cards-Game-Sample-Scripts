using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaderBoardItemHolder : MonoBehaviour
{

	public RawImage playerPhoto;
	public Image badge;
	public Text playername, score;
	public string url;

	private void OnEnable()
	{
		Debug.Log("OnEnable");
		if (!string.IsNullOrEmpty(url))
			StartCoroutine("GetPlayerImageAsync", url);
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
