using PlayFab;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text studentName;
    public GameObject ModeSelectPanel;


    void Start()
    {
        GetStudentName();
    }

    public void GetStudentName()
    {
        if (UserSession.Instance && UserSession.Instance.IsLogin())
        {
            studentName.text = UserSession.Instance.FullName;

            if (UserSession.Instance.Role == 1)
            {
                ModeSelectPanel.SetActive(true);
            }
        }
        else
        {
            studentName.text = "Dev";
            ModeSelectPanel.SetActive(true);

        }
    }
}
