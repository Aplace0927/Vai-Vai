using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayClickLogger : MonoBehaviour
{
    private XRRayInteractor rayInteractor;

    void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
    }

    void OnEnable()
    {
        rayInteractor.selectEntered.AddListener(OnSelect);
    }

    void OnDisable()
    {
        rayInteractor.selectEntered.RemoveListener(OnSelect);
    }

    void OnSelect(SelectEnterEventArgs args)
    {
        // 선택된 오브젝트
        var interactable = args.interactableObject.transform;

        Debug.Log("Clicked Object: " + interactable.name);
    }
}
