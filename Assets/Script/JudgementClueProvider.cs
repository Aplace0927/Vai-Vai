using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class JudgementClueProvider : MonoBehaviour
{
    public bool isSelected=true; // 지금 눌려져 있는가?
    public double lastEnterSelectTime=0; // 마지막으로 누른 시각
    public double lastExitSelectTime=0; 

    XRBaseInteractable interactable;

      void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
    }


    void OnEnable()
    {
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
    }

    void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEnter);
        interactable.hoverExited.RemoveListener(OnHoverExit);
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        isSelected = true;
        lastEnterSelectTime = Time.timeAsDouble;
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        isSelected = false;
        lastExitSelectTime = Time.timeAsDouble;
    }
}
