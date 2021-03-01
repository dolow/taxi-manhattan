using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialScene : MonoBehaviour
{
    private bool proceed = false;

    private void Update()
    {
        if (this.proceed)
            return;

        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("GameScene");
            this.proceed = true;
        }
    }
}
