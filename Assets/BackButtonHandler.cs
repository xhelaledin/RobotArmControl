using UnityEngine;
using UnityEngine.InputSystem;

public class BackButtonHandler : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (PanelManager.Instance.HasActivePanels())
            {
                PanelManager.Instance.HideTopActivePanel();
            }
            else
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // Instead of quitting, move app to background
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    activity.Call<bool>("moveTaskToBack", true);
                }
#else
                // In editor or other platforms, just quit or log
                Application.Quit();
#endif
            }
        }
    }
}

