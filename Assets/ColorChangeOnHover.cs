using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ColorChangeOnHover : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;

    [Header("Colors")]
    public Color hoverColor = Color.yellow;
    public Color selectColor = Color.red;

    private XRSimpleInteractable interactable;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        interactable = GetComponent<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
        interactable.selectEntered.AddListener(OnSelectEnter);
        interactable.selectExited.AddListener(OnSelectExit);
    }

    void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEnter);
        interactable.hoverExited.RemoveListener(OnHoverExit);
        interactable.selectEntered.RemoveListener(OnSelectEnter);
        interactable.selectExited.RemoveListener(OnSelectExit);
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        rend.material.color = hoverColor;
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        rend.material.color = originalColor;
    }

    void OnSelectEnter(SelectEnterEventArgs args)
    {
        rend.material.color = selectColor;
    }

    void OnSelectExit(SelectExitEventArgs args)
    {
        rend.material.color = originalColor;
    }
}
