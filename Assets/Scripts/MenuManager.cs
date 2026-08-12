using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button backButton;

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoToMainMenu);
        }
    }

    public void GoToMainMenu()
    {
        LoadScene(0);
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void QuitApp()
    {
        Application.Quit();
    }

}