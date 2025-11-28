using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ActivateClickRay : MonoBehaviour
{
    public GameObject leftClickRay;
    public GameObject rightClickRay;

    public XRDirectInteractor leftDirectClick;
    public XRDirectInteractor rightDirectClick;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        leftClickRay.SetActive(leftDirectClick.interactablesSelected.Count == 0);
        rightClickRay.SetActive(rightDirectClick.interactablesSelected.Count == 0);
    }
}
