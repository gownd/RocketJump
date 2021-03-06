using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class SlowMotionHandler : MonoBehaviour
{
    [SerializeField] MMFeedbacks slowMotionFeedback = null;

    private void Start() 
    {
        Time.fixedDeltaTime = 0.02f;    
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            StartSlowMotion();
        }

        if(Input.GetMouseButtonUp(1))
        {
            EndSlowMotion();
        }
    }

    public void StartSlowMotion()
    {
        slowMotionFeedback.PlayFeedbacks();
    }

    public void EndSlowMotion()
    {
        slowMotionFeedback.StopFeedbacks();
    }
}
