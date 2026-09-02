using System;

[Serializable]
public class MultiChoicesData : QuestionData
{
    public string[] Choices;
    public E_AnswerOption CorrectOption;

    public MultiChoicesData()
    {
        QuestionType = E_QuestionType.MultiChoices;
        Choices = new string[4];
        CorrectOption = E_AnswerOption.A;
    }
}
