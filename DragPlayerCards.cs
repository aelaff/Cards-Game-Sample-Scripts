
using UnityEngine;

public class DragPlayerCards : MonoBehaviour
{



    bool collided = false;
    public GameObject player1Cards;
    public GameObject player2Cards;
    //public bool IsOwnedByLocal = false;
    Transform collisionGO;
    private Vector3 selfPos;
    public static bool pickupCardsBlocked;
    public bool isMouseDownPerformed;
    public bool isMyTurn;

    private void Start()
    {
        pickupCardsBlocked = false;
        isMouseDownPerformed = false;
    }

    void OnMouseDown()
    {
        if (transform.parent.name == "remotePlayerCards" || !isMyTurn)
        {
            return;
        }
        Debug.Log("OnMouseDown");
        if (GetComponent<Card>().cardOpen && !pickupCardsBlocked)
        {
            isMouseDownPerformed = true;
            pickupCardsBlocked = true;
            MoveCardDown();
        }
        Debug.Log(GetComponent<Card>().IntialLocation);
    }
    void OnMouseDrag()
    {
        if (transform.parent.name == "remotePlayerCards" || !isMouseDownPerformed || !isMyTurn)
        {
            return;
        }
        Debug.Log("OnMouseDrag");

        if (GetComponent<Card>().cardOpen)
        {
            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 currentPositionFixed = new Vector3(currentPosition.x, currentPosition.y, 0f);
            transform.position = currentPositionFixed;
            GetComponent<SpriteRenderer>().sortingOrder = 24;
            GetComponent<Card>().isDraggable = true;
        }
    }
    private void OnMouseUp()
    {
        if (transform.parent.name == "remotePlayerCards" || !isMouseDownPerformed || !isMyTurn)
        {
            return;
        }
        Debug.Log("OnMouseUp");

        if (GetComponent<Card>().cardOpen && pickupCardsBlocked)
        {
            MoveCardUp();
            pickupCardsBlocked = false;
            isMouseDownPerformed = false;
        }
    }

    public void MoveCardUp()
    {
        if (GetComponent<Card>().cardOpen)
        {
            if (!collided)
            {
                UndoMove();
                UndoGrouping();
                Debug.Log("Did not collid");

            }
            else
            {
                Debug.Log("collided with " + collisionGO.tag);

                if (collisionGO.tag == "topCards")
                {
                    Card thisCard = GetComponent<Card>();
                    if (thisCard.cardMetaData.cardType + "" == collisionGO.name)
                    {
                        if ((collisionGO.childCount == 0 && thisCard.cardMetaData.cardNo == 0 && transform.childCount == 0) || (collisionGO.childCount > 0 && transform.childCount == 0 && thisCard.cardMetaData.cardNo == collisionGO.GetChild(collisionGO.childCount - 1).GetComponent<Card>().cardMetaData.cardNo + 1))
                        {
                            GameManager.Instance.photonView.RPC("AddCardToTopCards", PhotonTargets.AllBufferedViaServer, thisCard.cardMetaData.cardType, thisCard.cardMetaData.cardNo);
                            Debug.Log("Add " + thisCard.cardMetaData.cardNo + " to " + collisionGO.name);
                        }                   
                        else
                        {
                            SoundManager.Instance.PlayError();
                            UndoMove();
                            UndoGrouping();
                        }
                    }
                    else
                    {
                        SoundManager.Instance.PlayError();
                        UndoMove();
                        UndoGrouping();
                    }
                    return;
                }
                else if (collisionGO.tag == "middleCards" && collisionGO.parent != transform.parent)
                {
                    Transform lastCardTransform = collisionGO.parent.GetChild(collisionGO.parent.childCount - 1);

                    Card thisCard = GetComponent<Card>();
                    Card lastCard = lastCardTransform.GetComponent<Card>();
                    if (thisCard.cardMetaData.cardNo + 1 == lastCard.cardMetaData.cardNo &&
                            thisCard.cardMetaData.cardType != lastCard.cardMetaData.cardType &&
                            (thisCard.cardMetaData.cardType + lastCard.cardMetaData.cardType) % 2 != 0)
                    {
                        UndoGrouping();
                        //GameManager.Instance.EndTurn();
                        GameManager.Instance.photonView.RPC("AddCardToMiddleCards", PhotonTargets.AllBufferedViaServer, thisCard.cardMetaData.cardType, thisCard.cardMetaData.cardNo, lastCard.cardMetaData.cardType, lastCard.cardMetaData.cardNo);

                    }
                    else
                    {
                        SoundManager.Instance.PlayError();
                        UndoMove();
                        UndoGrouping();
                    }
                }
                else if (collisionGO.tag == "emptyMiddleCard" && collisionGO.transform != transform.parent)
                {
                    Card thisCard = GetComponent<Card>();
                    if (thisCard.cardMetaData.cardNo > 1)
                    {
                        byte emptyMiddleCardIndex = (byte)collisionGO.transform.GetSiblingIndex();

                        UndoGrouping();
                        //GameManager.Instance.EndTurn();
                        GameManager.Instance.photonView.RPC("AddCardToEmptyMiddleCard", PhotonTargets.AllBufferedViaServer, thisCard.cardMetaData.cardType, thisCard.cardMetaData.cardNo, emptyMiddleCardIndex);

                    }
                    else
                    {
                        SoundManager.Instance.PlayError();
                        UndoMove();
                        UndoGrouping();
                    }
                }
                else if (collisionGO.tag == "remotePlayerCards" && transform.childCount == 0 && transform.tag == "playerCards")
                {
                    Transform lastCardTransform = collisionGO.transform;

                    Card thisCard = GetComponent<Card>();
                    Card lastCard = lastCardTransform.GetComponent<Card>();
                    if ((thisCard.cardMetaData.cardNo + 1 == lastCard.cardMetaData.cardNo || thisCard.cardMetaData.cardNo - 1 == lastCard.cardMetaData.cardNo) &&
                            thisCard.cardMetaData.cardType != lastCard.cardMetaData.cardType &&
                            (thisCard.cardMetaData.cardType + lastCard.cardMetaData.cardType) % 2 != 0)
                    {
                        UndoGrouping();
                        //GameManager.Instance.EndTurn();
                        GameManager.Instance.photonView.RPC("AddCardToRemotePlayer", PhotonTargets.AllBufferedViaServer, thisCard.cardMetaData.cardType, thisCard.cardMetaData.cardNo, lastCard.cardMetaData.cardType, lastCard.cardMetaData.cardNo);
                    }
                    else
                    {
                        SoundManager.Instance.PlayError();
                        UndoMove();
                        UndoGrouping();
                    }
                }
                else
                {
                    UndoMove();
                    UndoGrouping();
                }
            }
        }
    }

    public void UndoMove()
    {

        transform.GetComponent<Card>().IntialLocation = transform.GetComponent<Card>().IntialLocation;
        transform.GetComponent<SpriteRenderer>().sortingOrder = transform.GetSiblingIndex();
    }

    public void UndoGrouping()
    {
        if (transform.childCount > 0)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform temp = transform.GetChild(i);
                temp.parent = transform.parent;

            }
        }
        reStructureChildren reStructureChildrens = transform.parent.GetComponent<reStructureChildren>();
        if (reStructureChildrens != null)
        {
            reStructureChildrens.RearrangeCards();
            reStructureChildrens.RenameCardsAndSortLayers();
        }
    }

    public void MoveCardDown()
    {
        if (GetComponent<Card>().cardOpen)
        {

            GetComponent<Card>().IntialLocation = transform.position;
            GetComponent<Card>().sortingLayer = GetComponent<SpriteRenderer>().sortingOrder;

            if (!IsLast(transform))
            {
                if (tag == "middleCards")
                {
                    int thisIndex = transform.GetSiblingIndex();
                    int lastIndex = transform.parent.childCount - 1;
                    Debug.Log("thisIndex " + thisIndex + ", lastIndex" + lastIndex);
                    for (int i = lastIndex; i > thisIndex; i--)
                    {
                        transform.parent.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 24 + i;
                        transform.parent.GetChild(i).parent = transform;
                    }
                }
            }
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.gameObject.tag == "middleCards" || collision.gameObject.tag == "topCards" || collision.gameObject.tag == "emptyMiddleCard" || collision.gameObject.tag == "remotePlayerCards")
        {
            collisionGO = collision.transform;
            collided = true;
        }

    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {

        collided = false;
    }
    
    private bool IsLast(Transform child)
    {
        return child.parent.GetChild(child.parent.childCount - 1) == child;

    }

}
