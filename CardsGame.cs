using System.Collections.Generic;
using UnityEngine;

public class CardsGame : Photon.PunBehaviour
{
    public List<Sprite> allCards;
    public List<GameObject> cardsGO;
    public List<Card> myCards;
    public List<Card> tmpCards;

    public List<Card> scenceCards;

    public static bool sent = true;

    public static CardsGame instance;


    private void Start()
    {
        instance = this;
    }

    public void InitGame() {
        sent = false;
    }

    List<CardMetaData> Shuffle(List<CardMetaData> orignialCards)
    {
        for (int t = 0; t < orignialCards.Count; t++)
        {
            CardMetaData tmp = orignialCards[t];
            int r = UnityEngine.Random.Range(t, orignialCards.Count);
            orignialCards[t] = orignialCards[r];
            orignialCards[r] = tmp;
        }
        return orignialCards;
    }


    public List<CardMetaData> GetOrignial()
    {

        List<CardMetaData> originals = new List<CardMetaData>();
        myCards = new List<Card>();
        for (int j = 0; j < 52; j++)
        {
            CardMetaData cardData = new CardMetaData((byte)(j % 13), (byte)(j / 13), (byte)j);
            originals.Add(cardData);
        }
        return originals;
    }
    public string GenerateCards()
    {

        List<CardMetaData> orignialCards = GetOrignial();

        ShuffledCards cards = new ShuffledCards();
        cards.shuffledCards = Shuffle(orignialCards);

        return JsonUtility.ToJson(cards);

    }

    [System.Serializable]
    class ShuffledCards {
        public List<CardMetaData> shuffledCards;
    }



    void PutCardsInTheirPositions(string json)
    {
        ShuffledCards cardsContainer = JsonUtility.FromJson<ShuffledCards>(json);
        List<CardMetaData> cards = cardsContainer.shuffledCards;




        if (cards != null && cards.Count > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<Card>().InitValues(cards[i], allCards[cards[i].cardSpriteIndex], true);
            }
            //myCards.RemoveRange(0, 3);
            for (int i = 4; i < 27; i++)
            {
                transform.GetChild(2).GetChild(i - 4).GetComponent<Card>().InitValues(cards[i], allCards[cards[i].cardSpriteIndex], false);
            }
            //myCards.RemoveRange(0, 12);
            for (int i = 27; i < 51; i++)
            {
                //init player 2 cards and give him the ownership
                transform.GetChild(3).GetChild(i - 27).GetComponent<Card>().InitValues(cards[i], allCards[cards[i].cardSpriteIndex], false);
               // transform.GetChild(3).GetChild(i - 27).GetComponent<Card>().photonView.TransferOwnership(PhotonNetwork.otherPlayers[0].ID);
            }
            //transform.GetChild(3).GetComponent<PhotonView>().photonView.TransferOwnership(PhotonNetwork.otherPlayers[0].ID);
        }
    }


    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {

            if (PhotonNetwork.player.IsMasterClient && !sent)
            {
                Debug.Log("Stream, CardsGame : isWriting Begin");

                string jsonData = GenerateCards();

                //PutCardsInTheirPositions(cardNo, cardType);
                Debug.Log("Send Stream, CardsGame :" + jsonData);

                stream.Serialize(ref jsonData);
                PutCardsInTheirPositions(jsonData);
                sent = true;
                Debug.Log("Stream, CardsGame : isWriting End");

            }

        }
        else if (stream.isReading)
        {
            Debug.Log("Stream, CardsGame : isReading Begin");

            string jsonData = null;
            stream.Serialize(ref jsonData);
            Debug.Log("Stream, CardsGame :" + jsonData);
            
            PutCardsInTheirPositions(jsonData);
            Debug.Log("Stream, CardsGame : isReading End");

        }
    }

}
