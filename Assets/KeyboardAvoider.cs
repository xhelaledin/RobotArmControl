using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if TMP_PRESENT
using TMPro;
#endif

[RequireComponent(
#if TMP_PRESENT
    typeof(TMP_InputField)
#else
    typeof(InputField)
#endif
)]
public class KeyboardAvoider : MonoBehaviour
, ISelectHandler
, IDeselectHandler
{
    [Header("Assign the container you want to shift")]
    public RectTransform panelToMove;

    [Tooltip("Extra pixels above keyboard")]
    public float offset = 10f;

    Vector2 _originalPos;
    TouchScreenKeyboard _keyboard;
    bool _isSelected;

#if TMP_PRESENT
    TMP_InputField _tmpInput;
#else
    InputField _uIInput;
#endif

    void Awake()
    {
#if TMP_PRESENT
        _tmpInput = GetComponent<TMP_InputField>();
#else
        _uIInput  = GetComponent<InputField>();
#endif

        if (panelToMove == null)
            panelToMove = transform.parent.GetComponent<RectTransform>();

        _originalPos = panelToMove.anchoredPosition;
    }

    void Update()
    {
        if (_isSelected && TouchScreenKeyboard.visible)
        {
            // Get keyboard height in screen pixels
            Rect kbRect = TouchScreenKeyboard.area;
            float kbHeight = kbRect.height;

            // Convert to canvas units
            float canvasHeight = panelToMove.GetComponentInParent<Canvas>()
                                            .GetComponent<RectTransform>()
                                            .sizeDelta.y;
            float screenHeight = Screen.height;
            float heightInCanvas = kbHeight * (canvasHeight / screenHeight);

            panelToMove.anchoredPosition =
                _originalPos + Vector2.up * (heightInCanvas + offset);
        }
        else if (!_isSelected)
        {
            panelToMove.anchoredPosition = _originalPos;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = true;

        // If you want to manually open keyboard:
        // _keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isSelected = false;
    }
}
