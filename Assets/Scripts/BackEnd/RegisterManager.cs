using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField userName;
    public TMP_InputField password;
    public TMP_InputField fullName;
    public TMP_InputField studentClass;
    public TMP_InputField schoolName;
    public TMP_Dropdown role;
    public Button registerBtn;
    public Button backToLogin;
    public GameObject loginForm;
    [SerializeField] private TMP_Text notifyText;

    private void OnEnable()
    {
        registerBtn.onClick.AddListener(Register);
        backToLogin.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            loginForm.SetActive(true);
        });
    }

    public void Register()
    {
        SetNotification(string.Empty);

        var request = new RegisterPlayFabUserRequest
        {
            Username = userName.text,
            Password = password.text,
            RequireBothUsernameAndEmail = false,
            DisplayName = fullName.text,
        };

        PlayFabClientAPI.RegisterPlayFabUser(
            request,
            OnRegisterSuccess,
            OnRegisterFailed
        );
    }

    public void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Register success!");
        Debug.Log($"PlayFab ID: {result.PlayFabId}");
        Debug.Log($"Username: {result.Username}");

        SaveStudentValue();
    }

    public void OnRegisterFailed(PlayFabError result)
    {
        SetNotification("Đăng ký thất bại");
        Debug.LogError(result.GenerateErrorReport());
    }

    private void SetNotification(string message)
    {
        if (notifyText != null) notifyText.text = message;
    }

    private void SaveStudentValue()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"FullName", fullName.text},
                {"School", schoolName.text},
                {"Class", studentClass.text},
                {"Role", role.value.ToString()}
            }
        };

        PlayFabClientAPI.UpdateUserData(
            request,
            result => Debug.Log("Student information saved."),
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    private void OnDisable()
    {
        registerBtn.onClick.RemoveListener(Register);

    }
}
