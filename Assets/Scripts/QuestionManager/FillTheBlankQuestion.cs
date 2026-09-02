using UnityEngine;
using TMPro;

public class FillTheBlankQuestion : Question
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TMP_InputField answerInputField;
    [SerializeField] private TextMeshProUGUI feedbackText;

    private string correctAnswer;

    public override void DisplayQuestion(QuestionSO questionSO)
    {
        if (questionSO == null || questionSO.QuestionType != E_QuestionType.FillTheBlank)
            return;

        questionText.text = questionSO.Question;
        correctAnswer = questionSO.Answer;
        answerInputField.text = "";
        feedbackText.text = "";
        ResetStatusColor();
    }

    public override void DisplayQuestion(QuestionData questionData)
    {
        if (questionData == null || questionData.QuestionType != E_QuestionType.FillTheBlank)
            return;

        questionText.text = questionData.Question;
        correctAnswer = questionData.Answer;
        answerInputField.text = "";
        feedbackText.text = "";
        ResetStatusColor();
    }

    public string GetUserAnswer()
    {
        return answerInputField.text.Trim();
    }

    public string GetCorrectAnswer()
    {
        return correctAnswer;
    }

    public bool IsAnswerCorrect()
    {
        return GetUserAnswer().Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase);
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
        answerInputField.text = "";
        feedbackText.text = "";
        ResetStatusColor();
    }
}
