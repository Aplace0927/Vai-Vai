using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// DragSelectSystem에서 정의한 ISelectable 인터페이스를 구현합니다

public class IntegratedVisualFeedback : MonoBehaviour, ISelectable
{
    private MeshRenderer meshRenderer;
    private XRBaseInteractable interactable;
    private Material materialInstance;

    // URP/HDRP Standard Material에 사용되는 기본 색상 속성 ID
    // Built-in RP Standard Shader를 사용한다면 "_Color"로 변경하세요.
    private static readonly int ColorPropertyID = Shader.PropertyToID("_BaseColor");

    [Header("Color Feedback Settings")]
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.red;

    private bool isSelected = false; // 선택 상태 추적
    private Color originalColor;
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        interactable = GetComponent<XRBaseInteractable>();

        if (meshRenderer == null || !meshRenderer.material.HasProperty(ColorPropertyID))
        {
            Debug.LogError(gameObject.name + ": MeshRenderer 또는 유효한 Material/Color 속성(_BaseColor)이 없습니다. 스크립트 비활성화.");
            enabled = false;
            return;
        }

        // Material 인스턴스를 한 번만 가져와 저장합니다.
        materialInstance = meshRenderer.material;
        originalColor = materialInstance.GetColor(ColorPropertyID);

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }
    }

    private void SetMaterialColor(Color color)
    {
        if (materialInstance != null)
        {
            materialInstance.SetColor(ColorPropertyID, color);
        }
    }

    // ⭐ OnHoverEntered: Select 상태가 아닐 때만 Hover 색상 적용
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (!isSelected)
        {
            SetMaterialColor(hoverColor);
        }
    }

    // ⭐ OnHoverExited: Select 상태가 아닐 때만 원래 색상으로 복원
    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (!isSelected)
        {
            SetMaterialColor(originalColor);
        }
    }

    // ISelectable 구현: Select 상태가 됨
    public void OnSelect()
    {
        isSelected = true;
        // Select 시: Hover 상태이든 아니든 선택 색상으로 덮어씁니다.
        SetMaterialColor(selectedColor);
    }

    // ISelectable 구현: Deselect 상태가 됨
    public void OnDeselect()
    {
        isSelected = false;

        // Deselect 시: 무조건 원래 색상으로 복원합니다.
        // 이 시점에서 Hover 상태 검사를 제거하여 불필요한 재반응을 막습니다.
        SetMaterialColor(originalColor);
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }

        // 메모리 정리
        if (materialInstance != null)
        {
            // Material 인스턴스 파괴 전 원래 색상으로 복원
            materialInstance.SetColor(ColorPropertyID, originalColor);
            Destroy(materialInstance);
        }
    }
}