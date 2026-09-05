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

            if (UserSession.Instance.Role == (int)E_Roles.Student)
            {
                ModeSelectPanel.SetActive(true);
                ModePanel modePanel = ModeSelectPanel.GetComponent<ModePanel>();
                if (modePanel != null) modePanel.ShowChooseTopicForStudent();
            }
            else
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
