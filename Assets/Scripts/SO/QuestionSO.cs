using UnityEngine;

[CreateAssetMenu(fileName = "QuestionSO", menuName = "Scriptable Objects/QuestionSO")]
public class QuestionSO : ScriptableObject
{
    public E_QuestionType QuestionType;
    public E_Topic Topic;

    public string Question;
    // question answer
    public string Answer;

    // for multi choices
    [Header("Multi choices")]
    public string[] Choices;

}
