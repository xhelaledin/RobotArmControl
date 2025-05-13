using UnityEngine;
using TMPro;

public class OpenKeyboard : MonoBehaviour
{
    public TMP_InputField firstTMPInputField;
    public TMP_InputField secondTMPInputField;
    public TMP_InputField thirdTMPInputField;


    public void OpenKeyboardForFirst()
    {
        firstTMPInputField.Select();
        firstTMPInputField.ActivateInputField();
    }

    public void OpenKeyboardForSecond()
    {
        secondTMPInputField.Select();
        secondTMPInputField.ActivateInputField();
    }

    public void OpenKeyboardForThird()
    {
        thirdTMPInputField.Select();
        thirdTMPInputField.ActivateInputField();
    }
}