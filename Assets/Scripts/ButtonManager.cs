using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{

    public void OnClick()
    {
        Debug.Log("Button Clicked");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
