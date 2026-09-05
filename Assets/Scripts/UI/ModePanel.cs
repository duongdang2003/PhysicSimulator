using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModePanel : MonoBehaviour
{
    [SerializeField] private Button _createTopicGroupBtn;
    [SerializeField] private Button _chooseTopicGroupBtn;
    [SerializeField] private GameObject _createTopic;
    [SerializeField] private GameObject _chooseTopic;
    [SerializeField] private Button _logoutBtn;

    [SerializeField] private Button _backBtn;

    private void OnEnable()
    {
        _createTopicGroupBtn.onClick.AddListener(() =>
        {
            _createTopic.SetActive(true);
            _backBtn.gameObject.SetActive(true);
        });

        _chooseTopicGroupBtn.onClick.AddListener(() =>
        {
            _chooseTopic.SetActive(true);
            _backBtn.gameObject.SetActive(true);
        });

        _backBtn.onClick.AddListener(() =>
        {
            _createTopic.SetActive(false);
            _chooseTopic.SetActive(false);
            _backBtn.gameObject.SetActive(false);
        });

        _logoutBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex - 1);
        });
    }

    private void OnDisable()
    {
        _createTopicGroupBtn.onClick.RemoveAllListeners();
        _chooseTopicGroupBtn.onClick.RemoveAllListeners();
        _backBtn.onClick.RemoveAllListeners();
        _logoutBtn.onClick.RemoveAllListeners();
    }

    public void ShowChooseTopicForStudent()
    {
        _createTopicGroupBtn.gameObject.SetActive(false);
        _chooseTopicGroupBtn.gameObject.SetActive(false);
        _createTopic.SetActive(false);
        _chooseTopic.SetActive(true);
        _backBtn.gameObject.SetActive(false);
    }

    public void ToggleBackBtn(bool toggle) => _backBtn.gameObject.SetActive(toggle);
}
