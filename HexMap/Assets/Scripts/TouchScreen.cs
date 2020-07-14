using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchScreen : MonoBehaviour
{
    [SerializeField] HexMapCamera hexMapCamera;
    Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
    Vector2 lastMousePos;
    float deltaRotation = 0.6f;
    float minReactDistance = 6;

    private void Start()
    {
        lastMousePos = screenCenter;
    }

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float centerXOffset = Input.mousePosition.x - screenCenter.x;
            float centerYOffset = Input.mousePosition.y - screenCenter.y;

            lastMousePos.x = centerXOffset;
            lastMousePos.y = centerYOffset;
        }

        if (Input.GetMouseButton(0))
        {
            if (Input.touchCount > 1)
            {
                return;
            }
            Vector2 crtMousePos = new Vector2(Input.mousePosition.x - screenCenter.x, Input.mousePosition.y - screenCenter.y);
            Vector2 posVector = crtMousePos - lastMousePos;
            if (posVector.magnitude > minReactDistance)
            {
                //horizontal
                if (Mathf.Abs(posVector.x) > Mathf.Abs(posVector.y))
                {
                    //right
                    if (posVector.x > 0)
                    {
                        //up
                        if (lastMousePos.y > 0)
                        {
                            hexMapCamera.AdjustRotation(-deltaRotation);
                        }
                        else//down
                        {
                            hexMapCamera.AdjustRotation(deltaRotation);
                        }
                    }
                    else//left
                    {
                        //up
                        if (lastMousePos.y > 0)
                        {
                            hexMapCamera.AdjustRotation(deltaRotation);
                        }
                        else//down
                        {
                            hexMapCamera.AdjustRotation(-deltaRotation);
                        }
                    }
                }
                else//vertical
                {
                    //up
                    if (posVector.y > 0)
                    {
                        //right
                        if (lastMousePos.x > 0)
                        {
                            hexMapCamera.AdjustRotation(deltaRotation);
                        }
                        else//left
                        {
                            hexMapCamera.AdjustRotation(-deltaRotation);
                        }
                    }
                    else//down
                    {
                        //right
                        if (lastMousePos.x > 0)
                        {
                            hexMapCamera.AdjustRotation(-deltaRotation);
                        }
                        else//left
                        {
                            hexMapCamera.AdjustRotation(deltaRotation);
                        }
                    }
                }
            }
            lastMousePos = crtMousePos;
        }
    }

}
