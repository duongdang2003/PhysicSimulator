using UnityEngine;
using UnityEngine.UI;

public class Question : MonoBehaviour
{
    [SerializeField] protected Image statusImage;

    public virtual void DisplayQuestion(QuestionSO questionSO)
    {
    }

    public virtual void ResetQuestion()
    {
    }

    protected void SetStatusColor(Color color)
    {
        if (statusImage != null)
        {
            statusImage.color = color;
        }
    }

    protected void ResetStatusColor()
    {
        SetStatusColor(Color.white);
    }
}
