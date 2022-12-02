using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneChange : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log("HIT ACTIVATION");
        // Only specifying the sceneName or sceneBuildIndex will load the Scene with the Single mode
        SceneManager.LoadScene("MK-HIGH(DEREK)", LoadSceneMode.Single);
    }
}