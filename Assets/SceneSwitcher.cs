using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void SwitchtoSavedPositionsScene()
    {
        SceneManager.LoadScene("SavedPositionsScene");
    }
}
