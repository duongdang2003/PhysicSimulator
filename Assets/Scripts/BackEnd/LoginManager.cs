using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField userName;
    public TMP_InputField passWord;
    public Button loginBtn;
    public Button backToRegister;
    public GameObject registerForm;

    private void OnEnable()
    {
        loginBtn.onClick.AddListener(Login);
        backToRegister.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            registerForm.SetActive(true);
        });
    }

    public void Login()
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = userName.text,
            Password = passWord.text
        };

        PlayFabClientAPI.LoginWithPlayFab
        (
            request,
            OnLoginSuccess,
            OnLoginFailed
        );
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");
        Debug.Log($"PlayFab ID: {result.PlayFabId}");

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnLoginFailed(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    private void OnDisable()
    {
        loginBtn.onClick.RemoveListener(Login);
        backToRegister.onClick.RemoveAllListeners();

    }
}
