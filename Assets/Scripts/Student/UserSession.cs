using PlayFab;
using UnityEngine;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance { get; private set; }

    public string UserID { get; private set; }
    public string FullName { get; private set; }
    public string Class { get; private set; }
    public string School { get; private set; }
    public int Role { get; private set; }

    public bool IsLogin() => PlayFabClientAPI.IsClientLoggedIn();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Role = 1;
        DontDestroyOnLoad(gameObject);
    }

    public void SetData(
        string playerID,
        string fullName,
        string studentClass,
        string school,
        int role)
    {
        UserID = playerID;
        FullName = fullName;
        Class = studentClass;
        School = school;
        Role = role;
    }
}