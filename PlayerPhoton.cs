using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhoton : Photon.MonoBehaviour {
    //public PhotonView photonView;
    private void Start()
    {

    }
    void Update () {
        if (PhotonNetwork.connected) {
            //if (photonView.isMine)
            //{
            //    Debug.Log("Me : "+PhotonNetwork.playerName);
               
            //}
            //else {
            //    Debug.Log("Other : "+PhotonNetwork.playerName);

            //}
        }
    }

    //void OnPhotonSerializeView(PhotonStream stream,PhotonMessageInfo info) {
    //    if (stream.isWriting)
    //    {
    //        stream.SendNext(selfPosions);
    //    }
    //    else {
    //        selfPosions=(Vector3[])stream.ReceiveNext();
    //    }
    //}
}
