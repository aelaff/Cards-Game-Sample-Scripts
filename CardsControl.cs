using UnityEngine;

public class CardsControl : MonoBehaviour
{
    public bool ownedByLocal;
    public Transform player1OpenCardArea;
    Transform playerCloseCardArea;
    public int cardToBeOpenIndex = 0;
    public int length = 0;
    public bool canOpenNewOne = false;

    private void Awake()
    {
        playerCloseCardArea = transform;
    }

   
    private void OnMouseUpAsButton()
    {
        if (ownedByLocal && GameManager.Instance.isMyTurn && canOpenNewOne)
        {
            GameManager.Instance.photonView.RPC("OpenCard", PhotonTargets.AllBufferedViaServer);
        }
    }

    public void Open()
    {
        length = transform.childCount;

        FixCards();

        if (cardToBeOpenIndex < length)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<Collider2D>().enabled = false;
            }

            Transform openedCard = transform.GetChild(cardToBeOpenIndex);
            //openedCard.position = player1OpenCardArea.position;
            openedCard.GetComponent<Card>().IntialLocation = player1OpenCardArea.position;
            openedCard.GetComponent<Card>().PlayFlipSound(0);
            openedCard.GetComponent<Card>().cardOpen = true;
            openedCard.GetComponent<Collider2D>().enabled = true;
            canOpenNewOne = false;
            cardToBeOpenIndex++;
        }
        else if (cardToBeOpenIndex >= length)
        {
            cardToBeOpenIndex = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform closedCard = transform.GetChild(i);
                closedCard.GetComponent<Card>().cardOpen = false;
                closedCard.GetComponent<Collider2D>().enabled = false;
                //closedCard.position = transform.position;
                closedCard.GetComponent<Card>().IntialLocation = transform.position;
                closedCard.GetComponent<Card>().PlayFlipSound((i / transform.childCount) / 2f);
            }
        }
    }

    public bool CheckLastOpenedCardMovementEligibility(bool openerIsLocal)
    {
        bool foundOneOpened = false;
        bool foundOneEligible = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform cardTransfrom = transform.GetChild(i);

            if (cardTransfrom.GetComponent<Collider2D>().enabled)
            {
                foundOneOpened = true;
                Card card = cardTransfrom.GetComponent<Card>();


                //topCards check

                if (card.cardMetaData.cardNo == 0)
                {
                    foundOneEligible = true;
                    Debug.Log("Can Be Placed on TOP CARDS");
                }

                Transform topCardsParent = GameManager.Instance.topCardsParent;
                for (int j = 0; j < topCardsParent.childCount; j++)
                {
                    Transform group = topCardsParent.GetChild(j);
                    if (group.childCount > 0)
                    {
                        Transform lastCardTransform = group.GetChild(group.childCount - 1);
                        Card lastCard = lastCardTransform.GetComponent<Card>();
                        if (lastCard.cardMetaData.cardType == card.cardMetaData.cardType &&
                            (lastCard.cardMetaData.cardNo + 1) == card.cardMetaData.cardNo)
                        {
                            foundOneEligible = true;
                            Debug.Log("Can Be Placed on TOP CARDS");
                        }
                    }
                }

                //middleCards
                Transform middleCardsParent = GameManager.Instance.middleCardsParent;
                for (int j = 0; j < middleCardsParent.childCount; j++)
                {
                    Transform group = middleCardsParent.GetChild(j);
                    if (group.childCount > 0)
                    {
                        Transform lastCardTransform = group.GetChild(group.childCount - 1);
                        Card lastCard = lastCardTransform.GetComponent<Card>();
                        if (lastCard.cardMetaData.cardType != card.cardMetaData.cardType &&
                            (card.cardMetaData.cardNo + 1) == lastCard.cardMetaData.cardNo &&
                            (card.cardMetaData.cardType + lastCard.cardMetaData.cardType) % 2 != 0)
                        {
                            foundOneEligible = true;
                            Debug.Log("Can Be Placed on MIDDLE CARDS");
                        }
                    }
                }

                //emptyMiddleCards
                for (int j = 0; j < middleCardsParent.childCount; j++)
                {
                    Transform group = middleCardsParent.GetChild(j);
                    if (group.childCount == 0 && card.cardMetaData.cardNo > 1)
                    {
                        foundOneEligible = true;
                        Debug.Log("Can Be Placed on EMPTY MIDDLE CARDS");
                    }
                }

                Transform openerPlayerCards;
                if (openerIsLocal)
                {
                    //check remotePlayerCards
                    openerPlayerCards = GameManager.Instance.RemotePlayerCardsParent;
                }
                else
                {
                    //check localPlayerCards
                    openerPlayerCards = GameManager.Instance.localPlayerCardsParent;
                }

                for (int x = 0; x < openerPlayerCards.childCount; x++)
                {
                    cardTransfrom = openerPlayerCards.GetChild(x);

                    if (cardTransfrom.GetComponent<Collider2D>().enabled)
                    {
                        Card lastCard = cardTransfrom.GetComponent<Card>();
                        if (lastCard.cardMetaData.cardType != card.cardMetaData.cardType &&
                            ((card.cardMetaData.cardNo + 1) == lastCard.cardMetaData.cardNo || (card.cardMetaData.cardNo - 1) == lastCard.cardMetaData.cardNo) &&
                            (card.cardMetaData.cardType + lastCard.cardMetaData.cardType) % 2 != 0)
                        {
                            foundOneEligible = true;
                            Debug.Log("Can Be Placed on REMOTE MIDDLE CARDS");
                        }
                    }
                }
            }
        }

        if (!foundOneOpened)
        {
            return true;
        }

        if (foundOneEligible)
        {
            return true;
        }
        else
        {
            Debug.Log("No Moves Switching turns");
            return false;
        }
    }

    public void FixCards()
    {
        int topOneIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform cardTransfrom = transform.GetChild(i);
            cardTransfrom.GetComponent<SpriteRenderer>().sortingOrder = i;
            cardTransfrom.SetSiblingIndex(i);
            Card card = cardTransfrom.GetComponent<Card>();
            card.sortingLayer = i;
            if (card.cardOpen)
            {
                //cardTransfrom.position = player1OpenCardArea.position;
                card.IntialLocation = player1OpenCardArea.position;
                cardToBeOpenIndex = i + 1;
                topOneIndex = i;
            }
            else
            {
                //cardTransfrom.position = playerCloseCardArea.position;
                card.IntialLocation = playerCloseCardArea.position;
            }

            cardTransfrom.GetComponent<Collider2D>().enabled = false;
        }

        if (topOneIndex < transform.childCount)
        {
            //enable top one only
            transform.GetChild(topOneIndex).GetComponent<Collider2D>().enabled = true;
        }
    }
}