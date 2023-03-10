using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;


public class FriendsData
{
    public string friendStatus;

    public static FriendsData FromJson(string json)
    {
        return JsonUtility.FromJson<FriendsData>(json);
    }

}

public class InviteData
{
    public string inviteStatus;
    public string roomKey;

    public static InviteData FromJson(string json)
    {
        return JsonUtility.FromJson<InviteData>(json);
    }

}

public class FirebasePlayer
{
    public string fullName;
    public string uid;
    private Dictionary<string, FriendsData> friends = new Dictionary<string, FriendsData>(0);
    public string location;
    public string photoURL;
    public int score;
    public int wins;

    public FirebasePlayer(string fullName, string uid, string location, string photoURL, int score, int wins)
    {
        this.fullName = fullName;
        this.uid = uid;
        this.location = location;
        this.photoURL = photoURL;
        this.score = score;
        this.wins = wins;
    }

    public FirebasePlayer()
    {
    }

    public string GetNameFormated() {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fullName.ToLower());
    }


    public string ToJson() {
        return JsonUtility.ToJson(this);
    }

    public static FirebasePlayer FromJson(string json)
    {
        return JsonUtility.FromJson<FirebasePlayer>(json);
    }

    public Dictionary<string, FriendsData> getFriends()
    {
        return friends;
    }

    public bool IsFriendOrMe(FirebasePlayer result)
    {
        bool isFriend = false;
        bool isMe = false;
        FriendsData friend = null;
        //is he a friend
        isFriend = friends.TryGetValue(result.uid, out friend);
        //is it me
        isMe = result.uid.Equals(uid);

        return isMe || isFriend;
    }

    public bool IsBlocked(string remotePlayer)
    {
        FriendsData friend = null;
        
        if(friends.TryGetValue(remotePlayer, out friend))
            return friend.friendStatus == Constants.blocked;
        return false;
    }
}
