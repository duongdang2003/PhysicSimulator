using System;

[Serializable]
public abstract class QuestionData
{
    public E_QuestionType QuestionType;
    public E_Topic Topic;

    public string Question;
    public string Answer;
}
