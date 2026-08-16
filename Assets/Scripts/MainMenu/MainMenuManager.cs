using PlayFab;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text studentName;

    void Start()
    {
        GetStudentName();
    }

    public void GetStudentName()
    {
        Debug.Log(PlayFabClientAPI.IsClientLoggedIn());
        if(PlayFabClientAPI.IsClientLoggedIn())
        {
            if(studentName.text == "")
            {
                Debug.Log("get username");
                PlayFabClientAPI.GetUserData(
                    new PlayFab.ClientModels.GetUserDataRequest(),
                    result =>
                    {
                        if(result.Data != null && result.Data.ContainsKey("FullName"))
                        {
                            studentName.text = result.Data["FullName"].Value;
                            Debug.Log(result.Data["FullName"].Value);
                        }
                        else
                        {
                            Debug.Log("unknow");
                        }
                        
                    },
                    error =>
                    {
                        Debug.Log(error.GenerateErrorReport());
                    }
                );
            }
        }
    }
}
