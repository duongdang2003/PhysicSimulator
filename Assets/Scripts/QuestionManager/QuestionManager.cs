using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private E_Topic topic;
    [SerializeField] private QuestionSO[] questionSOs;
    [SerializeField] private FillTheBlankQuestion fillTheBlankPrefab;
    [SerializeField] private MultiChoiceQuestion multiChoicePrefab;
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button doAgainButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI topicTitleText;
    [SerializeField] private TextMeshProUGUI correctAnswerText;
    [SerializeField] private string titleText;

    private Question[] questionInstances;

    private void Start()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmit);
        }
        if (doAgainButton != null)
        {
            doAgainButton.onClick.AddListener(DoAgain);
        }
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoToMainMenu);
        }
        LoadQuestions();

        topicTitleText.text = titleText;

        LoadQuestionsByTopic(topic);
    }

    public void LoadQuestions()
    {
        if (questionSOs == null || questionSOs.Length == 0) return;

        Transform parent = scrollView != null ? scrollView.content : null;
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        questionInstances = new Question[questionSOs.Length];
        for (int i = 0; i < questionSOs.Length; i++)
        {
            Question questionInstance = null;

            if (questionSOs[i].QuestionType == E_QuestionType.FillTheBlank)
            {
                questionInstance = Instantiate(fillTheBlankPrefab, parent);
            }
            else if (questionSOs[i].QuestionType == E_QuestionType.MultiChoices)
            {
                questionInstance = Instantiate(multiChoicePrefab, parent);
            }

            if (questionInstance != null)
            {
                questionInstance.DisplayQuestion(questionSOs[i]);
                questionInstances[i] = questionInstance;
            }
        }
    }

    public void LoadQuestion(QuestionSO questionSO)
    {
        if (questionSO == null) return;

        Transform parent = scrollView != null ? scrollView.content : null;
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        Question questionInstance = null;

        if (questionSO.QuestionType == E_QuestionType.FillTheBlank)
        {
            questionInstance = Instantiate(fillTheBlankPrefab, parent);
        }
        else if (questionSO.QuestionType == E_QuestionType.MultiChoices)
        {
            questionInstance = Instantiate(multiChoicePrefab, parent);
        }

        if (questionInstance != null)
        {
            questionInstance.DisplayQuestion(questionSO);
            questionInstances = new Question[] { questionInstance };
        }
    }

    public void LoadQuestionsByTopic(E_Topic topic)
    {
        string topicFolderPath = topic.ToString();
        QuestionSO[] topicQuestions = Resources.LoadAll<QuestionSO>($"Questions/{topicFolderPath}");
        Debug.Log($"[LoadQuestionsByTopic] Loading from folder: Resources/Questions/{topicFolderPath}");
        Debug.Log($"[LoadQuestionsByTopic] Found {topicQuestions.Length} questions with topic: {topic}");

        if (topicQuestions.Length == 0)
        {
            Debug.LogWarning($"No questions found in Resources/{topicFolderPath}. Check folder structure and file locations");
            return;
        }

        Transform parent = scrollView != null ? scrollView.content : null;
        if (parent == null)
        {
            Debug.LogError("ScrollView.content is NULL! Assign ScrollView in inspector");
            return;
        }

        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        questionInstances = new Question[topicQuestions.Length];
        for (int i = 0; i < topicQuestions.Length; i++)
        {
            Question questionInstance = null;

            if (topicQuestions[i].QuestionType == E_QuestionType.FillTheBlank)
            {
                questionInstance = Instantiate(fillTheBlankPrefab, parent);
            }
            else if (topicQuestions[i].QuestionType == E_QuestionType.MultiChoices)
            {
                questionInstance = Instantiate(multiChoicePrefab, parent);
            }

            if (questionInstance != null)
            {
                questionInstance.DisplayQuestion(topicQuestions[i]);
                questionInstances[i] = questionInstance;
            }
        }
    }

    private void OnSubmit()
    {
        if (questionInstances == null || questionInstances.Length == 0) return;

        int correctCount = 0;
        foreach (Question question in questionInstances)
        {
            if (question is FillTheBlankQuestion fillTheBlank)
            {
                bool isCorrect = fillTheBlank.IsAnswerCorrect();
                fillTheBlank.ShowFeedback(isCorrect);
                if (isCorrect) correctCount++;
            }
            else if (question is MultiChoiceQuestion multiChoice)
            {
                bool isCorrect = multiChoice.IsAnswerCorrect();
                multiChoice.ShowFeedback(isCorrect);
                if (isCorrect) correctCount++;
            }
        }

        if (correctAnswerText != null)
        {
            correctAnswerText.text = $"Điểm: {correctCount}/{questionInstances.Length}";
        }
    }

    private void DoAgain()
    {
        if (questionInstances == null || questionInstances.Length == 0) return;

        ShuffleArray(questionInstances);

        foreach (Question question in questionInstances)
        {
            if (question is FillTheBlankQuestion fillTheBlank)
            {
                fillTheBlank.ResetQuestion();
            }
            else if (question is MultiChoiceQuestion multiChoice)
            {
                multiChoice.ResetQuestion();
            }
        }
    }

    private void ShuffleArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}

