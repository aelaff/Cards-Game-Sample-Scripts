using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reStructureChildren : MonoBehaviour
{

    public void RenameCardsAndSortLayers()
    {

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).name = i + "";
            transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = i + 1;
            transform.GetChild(i).GetComponent<Card>().sortingLayer = i + 1;
        }
    }


    public void RearrangeCards()
    {
        Card[] children = transform.GetComponentsInChildren<Card>();
        Array.Sort(children, delegate (Card c1, Card c2)
        {
            return c2.cardMetaData.cardNo.CompareTo(c1.cardMetaData.cardNo);
        });

        Vector3 nextPos = Vector3.zero;

        for (int i = 0; i < transform.childCount; i++)
        {
            children[i].transform.SetSiblingIndex(i);
            //children[i].transform.localPosition = nextPos;
            children[i].IntialLocation = children[i].transform.parent.TransformPoint(nextPos);
            children[i].GetComponent<SpriteRenderer>().sortingOrder = i + 1;
            children[i].sortingLayer = i + 1;
            nextPos = new Vector3(0, nextPos.y - 0.2f, 0);

        }
    }

    public void RearrangeTopCards()
    {
        Card[] children = transform.GetComponentsInChildren<Card>();
        Array.Sort(children, delegate (Card c1, Card c2)
        {
            return c2.cardMetaData.cardNo.CompareTo(c1.cardMetaData.cardNo);
        });


        for (int i = 0; i < transform.childCount; i++)
        {
            children[i].transform.SetSiblingIndex(i);
            children[i].IntialLocation = children[i].transform.TransformVector(Vector3.zero);
            children[i].GetComponent<SpriteRenderer>().sortingOrder = i + 1;
            children[i].sortingLayer = i + 1;
        }
    }

}
