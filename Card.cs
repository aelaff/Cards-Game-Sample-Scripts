using System;
using System.Collections;
using UnityEngine;

public class Card : MonoBehaviour
{
    public CardMetaData cardMetaData;
    private AudioSource cardFlip;
    public bool cardOpen = true;
    public Sprite cardEnable;
    public Sprite cardDisable;
    public int cardZone;

    public Vector3 IntialLocation
    {
        set
        {
            pos = value;
            StopAllCoroutines();
            StartCoroutine("LerpToPosition");
        }
        get { return pos; }
    }

    void Start()
    {
        cardFlip = GetComponent<AudioSource>();
    }

    public void PlayFlipSound(float delay)
    {
        if (cardFlip != null)
        {
            cardFlip.pitch = UnityEngine.Random.Range(.75f, 1.75f);
            cardFlip.PlayDelayed(delay);
        }
    }

    public bool isDraggable = false;
    public int sortingLayer;

    public Vector3 pos;

    public IEnumerator LerpToPosition()
    {
        while (Vector3.Distance(transform.position, pos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, pos, 0.5f);
            yield return new WaitForEndOfFrame();
        }

        transform.position = pos;
        yield return new WaitForEndOfFrame();
    }

    public void InitValues(CardMetaData cardMetaData, Sprite cardEnabledSprite, bool cardOpen)
    {
        this.cardMetaData = cardMetaData;
        this.cardEnable = cardEnabledSprite;
        this.cardOpen = cardOpen;
        IntialLocation = transform.parent.position;
    }

    private void Update()
    {
        //GetComponent<Collider>().enabled = cardOpen;
        if (cardOpen)
        {
            GetComponent<SpriteRenderer>().sprite = cardEnable;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = cardDisable;
        }
    }



}

[System.Serializable]
public class CardMetaData
{
    public byte cardNo;
    public byte cardType;
    public byte cardSpriteIndex;

    public CardMetaData(byte cardNo, byte cardType, byte cardSpriteIndex)
    {
        this.cardNo = cardNo;
        this.cardType = cardType;
        this.cardSpriteIndex = cardSpriteIndex;
    }
}