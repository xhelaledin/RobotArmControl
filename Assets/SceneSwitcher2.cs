using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher2 : MonoBehaviour
{
    public void LoadFirstScene()
    {
        SceneManager.LoadScene("RobotArmScene");
    }
}
