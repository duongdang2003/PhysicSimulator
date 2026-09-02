using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuestionManager : MonoBehaviour
{
    private const int QuestionsPerRound = 10;

    [SerializeField] private E_Topic topic;
    [SerializeField] private QuestionSO[] questionSOs;
    [SerializeField] private FillTheBlankQuestion fillTheBlankPrefab;
    [SerializeField] private MultiChoiceQuestion multiChoicePrefab;
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button doAgainButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI topicTitleText;
    [SerializeField] private TextMeshProUGUI correctAnswerText;
    [SerializeField] private string titleText;
    [SerializeField] private FirebaseQuestionRepository firebaseQuestionRepository;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject quizPanel;

    private Question[] questionInstances;
    private bool isLoading;

    private void Start()
    {
        if (submitButton != null) submitButton.onClick.AddListener(OnSubmit);
        if (startButton != null) startButton.onClick.AddListener(StartQuiz);
        if (doAgainButton != null)
        {
            doAgainButton.onClick.AddListener(DoAgain);
        }
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoToMainMenu);
        }
        if (topicTitleText != null) topicTitleText.text = titleText;
        if (startPanel != null) startPanel.SetActive(true);
        if (quizPanel != null) quizPanel.SetActive(false);
        if (submitButton != null) submitButton.interactable = false;
    }

    private void OnDestroy()
    {
        if (submitButton != null) submitButton.onClick.RemoveListener(OnSubmit);
        if (startButton != null) startButton.onClick.RemoveListener(StartQuiz);
        if (doAgainButton != null) doAgainButton.onClick.RemoveListener(DoAgain);
        if (backButton != null) backButton.onClick.RemoveListener(GoToMainMenu);
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

    public void StartQuiz()
    {
        if (isLoading) return;

        if (firebaseQuestionRepository == null)
        {
            Debug.LogWarning("FirebaseQuestionRepository chưa được gán. Dùng dữ liệu Resources làm fallback.");
            LoadQuestionsByTopic(topic);
            ShowQuizPanel();
            return;
        }

        isLoading = true;
        if (startButton != null) startButton.interactable = false;
        if (doAgainButton != null) doAgainButton.interactable = false;

        firebaseQuestionRepository.LoadRandomQuestionSet(topic, (questions, error) =>
        {
            isLoading = false;
            if (startButton != null) startButton.interactable = true;
            if (doAgainButton != null) doAgainButton.interactable = true;

            if (!string.IsNullOrEmpty(error) || questions == null || questions.Count == 0)
            {
                Debug.LogError("Không thể load question set: " + error);
                return;
            }

            LoadQuestionData(questions);
            ShowQuizPanel();
        });
    }

    private void LoadQuestionData(System.Collections.Generic.List<QuestionData> questionDataList)
    {
        Transform parent = scrollView != null ? scrollView.content : null;
        if (parent == null) return;

        foreach (Transform child in parent) Destroy(child.gameObject);
        questionInstances = new Question[questionDataList.Count];

        for (int i = 0; i < questionDataList.Count; i++)
        {
            Question questionInstance = null;
            if (questionDataList[i].QuestionType == E_QuestionType.FillTheBlank)
                questionInstance = Instantiate(fillTheBlankPrefab, parent);
            else if (questionDataList[i].QuestionType == E_QuestionType.MultiChoices)
                questionInstance = Instantiate(multiChoicePrefab, parent);

            if (questionInstance != null)
            {
                questionInstance.DisplayQuestion(questionDataList[i]);
                questionInstances[i] = questionInstance;
            }
        }

        if (scrollView != null) scrollView.verticalNormalizedPosition = 1f;
    }

    private void ShowQuizPanel()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (quizPanel != null) quizPanel.SetActive(true);
        if (submitButton != null) submitButton.interactable = true;
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

        ShuffleArray(topicQuestions);

        int questionCount = Mathf.Min(QuestionsPerRound, topicQuestions.Length);
        questionInstances = new Question[questionCount];
        for (int i = 0; i < questionCount; i++)
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
        if (correctAnswerText != null)
        {
            correctAnswerText.text = string.Empty;
        }

        StartQuiz();
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
        SceneManager.LoadScene(1);
    }
}

