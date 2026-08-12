using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MultiChoiceQuestion : Question
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons = new Button[4];
    [SerializeField] private TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[4];
    [SerializeField] private TextMeshProUGUI feedbackText;

    private string correctAnswer;
    private string selectedAnswer;
    private Color originalButtonColor;

    private void Awake()
    {
        originalButtonColor = answerButtons[0].GetComponent<Image>().color;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            answerButtons[i].onClick.AddListener(() => SelectAnswer(index));
        }
    }

    public override void DisplayQuestion(QuestionSO questionSO)
    {
        if (questionSO == null || questionSO.QuestionType != E_QuestionType.MultiChoices)
            return;

        questionText.text = questionSO.Question;
        feedbackText.text = "";
        selectedAnswer = "";
        ResetStatusColor();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < questionSO.Choices.Length)
            {
                answerTexts[i].text = questionSO.Choices[i];
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].GetComponent<Image>().color = originalButtonColor;
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        correctAnswer = questionSO.Answer;
    }

    private void SelectAnswer(int buttonIndex)
    {
        selectedAnswer = answerTexts[buttonIndex].text;
        MarkAsChosen();

        Image buttonImage = answerButtons[buttonIndex].GetComponent<Image>();
        buttonImage.color = new Color(0.5f, 0.5f, 0.5f);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i != buttonIndex)
            {
                Image img = answerButtons[i].GetComponent<Image>();
                img.color = originalButtonColor;
            }
        }
    }

    public string GetUserAnswer()
    {
        return selectedAnswer;
    }

    public string GetCorrectAnswer()
    {
        return correctAnswer;
    }

    public bool IsAnswerCorrect()
    {
        return selectedAnswer.Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase);
    }

    public void ShowFeedback(bool isCorrect)
    {
        if (isCorrect)
        {
            feedbackText.text = "Đúng rồi!";
            feedbackText.color = Color.green;
            SetStatusColor(Color.green);
        }
        else
        {
            feedbackText.text = $"Sai! Đáp án đúng là: {correctAnswer}";
            feedbackText.color = Color.red;
            SetStatusColor(Color.red);
        }
    }

    public void MarkAsChosen()
    {
        SetStatusColor(new Color(0.5f, 0.5f, 0.5f));
    }

    public override void ResetQuestion()
    {
        selectedAnswer = "";
        feedbackText.text = "";
        ResetStatusColor();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponent<Image>().color = originalButtonColor;
        }
    }
}
