using System;

[Serializable]
public class FillTheBlankData : QuestionData
{
    public FillTheBlankData() { QuestionType = E_QuestionType.FillTheBlank; }
}
