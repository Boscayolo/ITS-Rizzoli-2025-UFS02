using UnityEngine;

public class Utilities
{
    public static void SetCursorLocked(bool nCursorLocked)
    {
        if (nCursorLocked == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (nCursorLocked == false)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
