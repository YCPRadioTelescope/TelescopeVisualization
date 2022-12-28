using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneChange : MonoBehaviour
{
    public bool vr;
    void OnEnable()
    {
        Debug.Log("HIT ACTIVATION");
        // Only specifying the sceneName or sceneBuildIndex will load the Scene with the Single mode
        if(vr)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(1);
        }

    }
}