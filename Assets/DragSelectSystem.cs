using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections.Generic;

// 선택 가능한 오브젝트에 구현할 인터페이스
public interface ISelectable
{
    void OnSelect();
    void OnDeselect();
}

public class DragSelectSystem : MonoBehaviour
{
    [Header("XR Settings")]
    public XRRayInteractor rayInteractor;
    
    [Header("Input Settings")]
    public InputActionProperty selectAction; // Select 버튼 (트리거)
    
    private GameObject currentSelectedObject;
    private bool isDragging = false;
    
    void Start()
    {
        if (rayInteractor == null)
        {
            rayInteractor = GetComponent<XRRayInteractor>();
        }
        selectAction.action.Enable();
    }
    
    void Update()
    {
        bool isPressed = selectAction.action.IsPressed();
        
        if (isPressed && !isDragging)
        {
            isDragging = true;
        }
        
        if (isDragging && isPressed)
        {
            CheckRaycastHit();
        }
        else
        {
            if (isDragging)
            {
                DeselectCurrent();
            }
            isDragging = false;
        }
    }
    
    void CheckRaycastHit()
    {
        if (rayInteractor == null)
        {
            DeselectCurrent();
            return;
        }
        
        bool isHitting = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
        
        if (isHitting && hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // ISelectable 컴포넌트가 있는지 확인합니다.
            if (hitObject.GetComponent<ISelectable>() == null)
            {
                return; // 선택 불가능한 오브젝트는 무시
            }
            
            // 다른 오브젝트를 가리키거나 처음 선택하는 경우
            if (hitObject != currentSelectedObject)
            {
                DeselectCurrent();
                SelectObject(hitObject);
            }
        }
    }
    
    void SelectObject(GameObject obj)
    {
        if (obj == null) return;
        
        currentSelectedObject = obj;
        
        // ISelectable 인터페이스 호출 (선택된 오브젝트가 시각적 피드백을 처리)
        ISelectable selectable = obj.GetComponent<ISelectable>();
        selectable?.OnSelect();
        
        Debug.Log($"Selected: {obj.name}");
    }
    
    void DeselectCurrent()
    {
        if (currentSelectedObject == null) return;
        
        // ISelectable 인터페이스 호출
        ISelectable selectable = currentSelectedObject.GetComponent<ISelectable>();
        selectable?.OnDeselect();
        
        Debug.Log($"Deselected: {currentSelectedObject.name}");
        
        currentSelectedObject = null;
    }
    
    public GameObject GetSelectedObject()
    {
        return currentSelectedObject;
    }

    void OnDestroy()
    {
        selectAction.action.Disable();
        DeselectCurrent();
    }
}