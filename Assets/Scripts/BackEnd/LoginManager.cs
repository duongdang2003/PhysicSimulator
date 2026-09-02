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
    [SerializeField] private TMP_Text notifyText;

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
        SetNotification(string.Empty);

        var request = new LoginWithPlayFabRequest
        {
            Username = userName.text,
            Password = passWord.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserData = true
            }
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

        var data = result.InfoResultPayload.UserData;

        UserSession.Instance.SetData(
           result.PlayFabId,
           data["FullName"].Value,
           data["Class"].Value,
           data["School"].Value,
           int.Parse(data["Role"].Value)
       );

        Debug.Log("Login successful!");
        Debug.Log($"PlayFab ID: {result.PlayFabId}");
        Debug.Log($"Fullname: {data["FullName"].Value}");

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnLoginFailed(PlayFabError error)
    {
        SetNotification("Đăng nhập thất bại");
        Debug.LogError(error.GenerateErrorReport());
    }

    private void SetNotification(string message)
    {
        if (notifyText != null) notifyText.text = message;
    }

    private void OnDisable()
    {
        loginBtn.onClick.RemoveListener(Login);
        backToRegister.onClick.RemoveAllListeners();

    }
}
