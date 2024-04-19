using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlashingCursor : MonoBehaviour
{
    public InputField inputField;
    private Coroutine flashingCoroutine;
    private Graphic cursorGraphic;

    void Start()
    {
        cursorGraphic = inputField.GetComponentInChildren<Graphic>(); // Assuming the cursor is a child graphic element
    }

    public void OnInputFieldSelected()
    {
        flashingCoroutine = StartCoroutine(FlashCursor());
    }

    public void OnInputFieldDeselected()
    {
        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
            // Reset cursor appearance here
        }
    }

    IEnumerator FlashCursor()
    {
        while (true)
        {
            // Toggle cursor visibility here by modifying alpha value
            yield return new WaitForSeconds(0.5f); // Adjust flashing speed as needed
        }
    }
}
