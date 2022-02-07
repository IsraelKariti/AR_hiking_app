using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
enum Status
{
    SCALE,
    POSITION
}
//m
public class AdditionScript : MonoBehaviour
{
    //public Text t;
    public GameObject additionPrefab;
    private GameObject tmp;
    private Status pinchStatus;
    private int touchID0 = -1;
    private int touchID1 = -1;
    private float R = 0.1f;
    public void AddCube()
    {
        tmp = Instantiate(additionPrefab);
        tmp.transform.position = new Vector3(0, 0, 4);

    }
    private void Update()
    {
        if (Input.touchCount == 0)
        {
            touchID0 = -1;
            touchID1 = -1;
        }
        else if (Input.touchCount == 1)
        {
            //t.text += "\nphase: " + Input.GetTouch(0).phase;
            Vector2 d = Input.GetTouch(0).deltaPosition;
            float valX = d.x * R;
            float valY = d.y * R;
            if (Mathf.Abs(d.y) > Mathf.Abs(d.x))
            {
                tmp.transform.position += new Vector3(0, valY, 0);
            }
            else
            {
                tmp.transform.position += new Vector3(valX, 0, 0);
            }

        }
        else if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0CurrPos = touch0.position;
            Vector2 touch1CurrPos = touch1.position;

            Vector2 touch0delta = touch0.deltaPosition;
            Vector2 touch1delta = touch1.deltaPosition;

            Vector2 touch0PrevPos = touch0.position - touch0delta;
            Vector2 touch1PrevPos = touch1.position - touch1delta;

            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currMagnitude = (touch0.position - touch1.position).magnitude;

            float diff = currMagnitude - prevMagnitude;
            float move = diff * R;
            // check if the fingers are not too close to each other
            // being too close is too hard to determine the status of the pinch (scale or position)
            if (currMagnitude > 300)
            {
                // check if this is a new pinch
                if (touchID0 == -1 && touchID1 == -1)
                {

                    if (isVerticallySpaced(touch0CurrPos, touch1CurrPos))// change position
                    {
                        touchID0 = touch0.fingerId;
                        touchID1 = touch1.fingerId;

                        pinchStatus = Status.POSITION;
                        tmp.transform.localPosition += new Vector3(0, 0, -move);
                    }
                    //check if the fingers are diagonal to each other
                    else if (isHorizontalySpacedSpaced(touch0CurrPos, touch1CurrPos))
                    {
                        touchID0 = touch0.fingerId;
                        touchID1 = touch1.fingerId;

                        pinchStatus = Status.SCALE;
                        if (isScaleableByDiff(tmp.transform.localScale, diff))
                            tmp.transform.localScale += new Vector3(move, move, 0);
                    }

                }
                else// if this is an ongoing pinch
                {
                    if (pinchStatus == Status.POSITION)
                    {
                        tmp.transform.localPosition += new Vector3(0, 0, -move);
                    }
                    else
                    {
                        if (isScaleableByDiff(tmp.transform.localScale, diff))
                            tmp.transform.localScale += new Vector3(move, move, 0);
                    }
                }
            }

        }
    }

    private bool isScaleableByDiff(Vector3 localScale, float diff)
    {
        if (localScale.x + diff < 0 || localScale.y + diff < 0)
            return false;
        else
            return true;
    }

    private bool isVerticallySpaced(Vector2 touch0, Vector2 touch1)
    {
        if (Mathf.Abs(touch0.y - touch1.y) > 300)
            return true;
        else
            return false;
    }

    private bool isHorizontalySpacedSpaced(Vector2 touch0, Vector2 touch1)
    {
        if (Mathf.Abs(touch0.x - touch1.x) > 300)
            return true;
        else
            return false;
    }
}
