﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UserAvatar : MonoBehaviour
{
    public static Vector3 user_position;
    public static float user_localRotation;
    public static Vector3 user_forward;

    public Text testing;
    [Header("Auto Move")]
    public bool auto_move;              //if the user should dmeo the environment and walk automatically
    public Slider auto_speed;           //speed to walk when demoing
    public Slider auto_rotation;
    public WaypointManager waypointmanager;        //hold the list of waypoints, move to them in order
    Waypoint[] waypoints;
    int curwaypointindex = 0;

    // Use this for initialization
    void Start()
    {
        user_position = new Vector3();
        if (waypointmanager) {
            waypoints = waypointmanager.waypointPath;
        }
        //if(SystemInfo.deviceType == DeviceType.Handheld)
        //{
        //    auto_move = false;
        //}

        StartCoroutine("UpdatePositionAndRotation");
    }

    Vector3 nextposition;
    IEnumerator UpdatePositionAndRotation()
    {
        while (true)
        {
            //auto move the player through the environment (used for testing)
            if (auto_move)
            {
                nextposition = waypoints[curwaypointindex].transform.position;
                //move to the next target
                while ((transform.position - nextposition).sqrMagnitude > .2f)
                {
                    //rotate towards the next target

                    float angle = Vector3.SignedAngle((nextposition - transform.position).normalized, transform.forward, Vector3.up);
                    if ( angle > 1f)
                    {
                        FindObjectOfType<Notification_Voice>().Speak(VoiceDirection.Left);
                    }
                    else if (angle < -1f)
                    {
                        FindObjectOfType<Notification_Voice>().Speak(VoiceDirection.Right);
                    }


                    while (Vector3.SignedAngle((nextposition - transform.position).normalized, transform.forward, Vector3.up) > 1)
                    {
                        transform.Rotate(0, -auto_rotation.value * Time.deltaTime * 20, 0);
                        SetMyValues();
                        yield return null;
                    }
                    while (Vector3.SignedAngle((nextposition - transform.position).normalized, transform.forward, Vector3.up) < -1)
                    {
                        transform.Rotate(0, auto_rotation.value * Time.deltaTime * 20, 0);
                        SetMyValues();
                        yield return null;
                    }
                    transform.LookAt(nextposition);

                    transform.Translate((nextposition - transform.position).normalized * auto_speed.value * Time.deltaTime, Space.World);
                    SetMyValues();
                    yield return null;
                }
                transform.position = nextposition;

                //increase index to move to next
                curwaypointindex++;
                if (curwaypointindex >= waypoints.Length) {
                    curwaypointindex = 0;
                }
            }
            //use estimote and phone rotation to move (used for deployment)
            else
            {
                //get position from IndoorManager
                user_position.x = IndoorManager.UserX;
                user_position.z = IndoorManager.UserY;
                transform.position = user_position;

                //get the local rotation (in terms of the room) and set the rotation of character
                user_localRotation = UserRotation.GetRotation();
                testing.text = string.Format("Rot: {0:0.00}", user_localRotation);
                transform.rotation = Quaternion.Euler(0, user_localRotation, 0);

                //store where the user is facing
                user_forward = transform.forward;
            }
            yield return null;
        }
    }

    void SetMyValues()
    {
        user_position.x = transform.position.x;
        user_position.z = transform.position.z;
        user_localRotation = transform.rotation.eulerAngles.y;
        user_forward = transform.forward;
    }

    public void SetAutoWalk(bool walkauto)
    {
        auto_move = walkauto;
    }
}
