using UnityEngine;
using TMPro;

public class VRKeyboard : MonoBehaviour
{
    public TMP_InputField currentInput;
    public GameObject keyboard;

    void Start()
    {
        keyboard.SetActive(false); // siempre oculto al iniciar
    }

    public void OpenKeyboard(TMP_InputField input)
    {
        currentInput = input;
        keyboard.SetActive(true);
    }

    public void AddLetter(string letter)
    {
    if (currentInput == null)
        return;

    // respetar límite
    if (
        currentInput.characterLimit > 0 &&
        currentInput.text.Length >= currentInput.characterLimit
    )
        return;

    // respetar validación numérica del TMP_InputField
    if (
        currentInput.characterValidation ==
        TMP_InputField.CharacterValidation.Integer
    )
    {
        if (!char.IsDigit(letter[0]))
            return;
    }

    currentInput.text += letter;
    }

    public void Space()
    {
        if (currentInput == null) return;
        currentInput.text += " ";
    }

    public void Backspace()
    {
        if (currentInput == null) return;

        if (currentInput.text.Length > 0)
        {
            currentInput.text =
                currentInput.text.Substring(0, currentInput.text.Length - 1);
        }
    }

    public void Confirm()
    {
        // ✔ cerrar teclado al presionar OK
        keyboard.SetActive(false);
        currentInput = null;
    }

    // (opcional pero recomendado)
    public void CloseKeyboard()
    {
        keyboard.SetActive(false);
        currentInput = null;
    }
}