using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    private bool proceed = false;

    private void Awake()
    {
        Audio.CacheAll();
    }


    private void Update()
    {
        if (this.proceed)
            return;

        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("TutorialScene");
            this.proceed = true;
        }
    }
}
