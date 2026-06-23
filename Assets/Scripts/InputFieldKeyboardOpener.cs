using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class InputFieldKeyboardOpener :
    MonoBehaviour,
    IPointerClickHandler
{
    public VRKeyboard keyboard;

    private TMP_InputField input;

    void Start()
    {
        input = GetComponent<TMP_InputField>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        keyboard.OpenKeyboard(input);
    }
}