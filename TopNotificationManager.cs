using UnityEngine;

public class TopNotificationManager : MonoBehaviour {

    public float notificationTimeout = 4.5f;
    public GameObject topNotificationPrefab;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Show(FriendInvitePrefab friendInvite)
    {
        GameObject obj = Instantiate(topNotificationPrefab, gameObject.transform);
        FriendInvitePrefab friendInvite1 = obj.GetComponent<FriendInvitePrefab>();
        friendInvite1.Uid = friendInvite.Uid;
        friendInvite1.inviteState = friendInvite.inviteState;
        Destroy(obj, notificationTimeout);
    }
}
