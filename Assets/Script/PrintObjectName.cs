using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PrintObjectName : MonoBehaviour
{
    private XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelected);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelected);
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        // 클릭된 오브젝트 이름 출력
        Debug.Log("Clicked Object: " + gameObject.name);
    }
}
