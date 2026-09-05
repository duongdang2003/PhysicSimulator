using System.Collections.Generic;
using System.IO;
using System.Collections;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>Editor câu hỏi chạy trong UI game. Kéo các reference vào Inspector.</summary>
public class QuestionEditorUI : MonoBehaviour
{
    [Header("Màn hình chọn chủ đề")]
    [SerializeField] private GameObject topicSelectionPanel;
    [SerializeField] private TMP_Dropdown topicSelectionDropdown;
    [SerializeField] private Button confirmTopicButton;
    [SerializeField] private Button backToTopicSelectionButton;
    [SerializeField] private GameObject editorPanel;

    [Header("UI chung")]
    [SerializeField] private TMP_Dropdown questionTypeDropdown;
    [SerializeField] private TMP_Dropdown topicDropdown;
    [SerializeField] private TMP_Text selectedTopicText;
    [SerializeField] private Button createButton;
    [SerializeField] private Button importExcelButton;
    [SerializeField] private Button doneButton;

    [Header("Điền vào chỗ trống")]
    [SerializeField] private GameObject fillTheBlankEditor;
    [SerializeField] private TMP_InputField fillQuestionInput;
    [SerializeField] private TMP_InputField fillAnswerInput;

    [Header("Trắc nghiệm")]
    [SerializeField] private GameObject multiChoicesEditor;
    [SerializeField] private TMP_InputField multiQuestionInput;
    [SerializeField] private TMP_InputField[] choiceInputs;
    [SerializeField] private TMP_Dropdown correctAnswerDropdown;

    [Header("Danh sách QuestionSlot")]
    [SerializeField] private Transform questionSlotContainer;
    [SerializeField] private QuestionSlotUI questionSlotPrefab;

    [Header("Lưu Firebase")]
    [SerializeField] private FirebaseQuestionRepository firebaseRepository;

    private readonly List<QuestionData> questions = new List<QuestionData>();
    private QuestionData selectedQuestion;
    private bool isSaving;

    private void Awake()
    {
        EnsureImportButton();
        SetupQuestionTypeDropdown();
        SetupTopicSelectionDropdown();
        SetupCorrectAnswerDropdown();
        ShowTopicSelection();
        if (confirmTopicButton != null) confirmTopicButton.onClick.AddListener(OpenEditorForSelectedTopic);
        if (backToTopicSelectionButton != null) backToTopicSelectionButton.onClick.AddListener(ReturnToTopicSelection);
        if (questionTypeDropdown != null) questionTypeDropdown.onValueChanged.AddListener(OnQuestionTypeChanged);
        if (topicDropdown != null) topicDropdown.onValueChanged.AddListener(_ => ApplyEditorToSelectedQuestion());
        if (fillQuestionInput != null) fillQuestionInput.onEndEdit.AddListener(_ => ApplyEditorToSelectedQuestion());
        if (multiQuestionInput != null) multiQuestionInput.onEndEdit.AddListener(_ => ApplyEditorToSelectedQuestion());
        if (fillAnswerInput != null) fillAnswerInput.onEndEdit.AddListener(_ => ApplyEditorToSelectedQuestion());
        if (choiceInputs != null)
            foreach (TMP_InputField input in choiceInputs)
                if (input != null) input.onEndEdit.AddListener(_ => ApplyEditorToSelectedQuestion());
        if (correctAnswerDropdown != null) correctAnswerDropdown.onValueChanged.AddListener(_ => ApplyEditorToSelectedQuestion());
        if (createButton != null) createButton.onClick.AddListener(CreateQuestion);
        if (importExcelButton != null)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLFileUploadButton uploadButton = importExcelButton.GetComponent<WebGLFileUploadButton>();
            if (uploadButton == null) uploadButton = importExcelButton.gameObject.AddComponent<WebGLFileUploadButton>();
            uploadButton.FileUploaded = ImportWebGLFile;
#else
            importExcelButton.onClick.AddListener(ImportExcelFile);
#endif
        }
        if (doneButton != null) doneButton.onClick.AddListener(FinishQuestionSet);
        RefreshTypeEditor();
    }

    private void EnsureImportButton()
    {
        if (importExcelButton != null || createButton == null || editorPanel == null) return;

        GameObject buttonObject = Instantiate(createButton.gameObject, editorPanel.transform);
        buttonObject.name = "ImportExcelBtn";
        importExcelButton = buttonObject.GetComponent<Button>();
        if (importExcelButton == null) return;

        RectTransform sourceRect = createButton.GetComponent<RectTransform>();
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        if (sourceRect != null && buttonRect != null)
        {
            buttonRect.anchorMin = sourceRect.anchorMin;
            buttonRect.anchorMax = sourceRect.anchorMax;
            buttonRect.pivot = sourceRect.pivot;
            buttonRect.sizeDelta = sourceRect.sizeDelta;
            buttonRect.anchoredPosition = sourceRect.anchoredPosition + new Vector2(0f, 95f);
        }

        TMP_Text label = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = "Import CSV / Excel";
    }

    private void SetupQuestionTypeDropdown()
    {
        if (questionTypeDropdown == null) return;
        questionTypeDropdown.ClearOptions();
        questionTypeDropdown.AddOptions(new List<string> { "Trắc nghiệm", "Điền vào chỗ trống" });
        questionTypeDropdown.SetValueWithoutNotify((int)E_QuestionType.FillTheBlank);
    }

    private void SetupTopicSelectionDropdown()
    {
        if (topicSelectionDropdown == null) return;
        topicSelectionDropdown.ClearOptions();
        var options = new List<string>();
        foreach (E_Topic topic in System.Enum.GetValues(typeof(E_Topic)))
            options.Add(topic.ToString());
        topicSelectionDropdown.AddOptions(options);
        topicSelectionDropdown.SetValueWithoutNotify(0);
    }

    private void ShowTopicSelection()
    {
        if (topicSelectionPanel != null) topicSelectionPanel.SetActive(true);
        if (editorPanel != null) editorPanel.SetActive(false);
    }

    public void OpenEditorForSelectedTopic()
    {
        int topicIndex = topicSelectionDropdown == null ? 0 : topicSelectionDropdown.value;
        if (topicDropdown != null) topicDropdown.SetValueWithoutNotify(topicIndex);
        if (selectedTopicText != null) selectedTopicText.text = ((E_Topic)topicIndex).ToString();
        if (questionTypeDropdown != null) questionTypeDropdown.SetValueWithoutNotify((int)E_QuestionType.FillTheBlank);
        if (topicSelectionPanel != null) topicSelectionPanel.SetActive(false);
        if (editorPanel != null) editorPanel.SetActive(true);
        RefreshTypeEditor();
    }

    public void ReturnToTopicSelection()
    {
        ResetQuestionView();
        ShowTopicSelection();
    }

    private void SetupCorrectAnswerDropdown()
    {
        if (correctAnswerDropdown == null) return;
        correctAnswerDropdown.ClearOptions();
        correctAnswerDropdown.AddOptions(new List<string> { "A", "B", "C", "D" });
        correctAnswerDropdown.SetValueWithoutNotify(0);
    }

    private void OnDestroy()
    {
        if (confirmTopicButton != null) confirmTopicButton.onClick.RemoveListener(OpenEditorForSelectedTopic);
        if (backToTopicSelectionButton != null) backToTopicSelectionButton.onClick.RemoveListener(ReturnToTopicSelection);
        if (questionTypeDropdown != null) questionTypeDropdown.onValueChanged.RemoveAllListeners();
        if (topicDropdown != null) topicDropdown.onValueChanged.RemoveAllListeners();
        if (fillQuestionInput != null) fillQuestionInput.onEndEdit.RemoveAllListeners();
        if (multiQuestionInput != null) multiQuestionInput.onEndEdit.RemoveAllListeners();
        if (fillAnswerInput != null) fillAnswerInput.onEndEdit.RemoveAllListeners();
        if (choiceInputs != null)
            foreach (TMP_InputField input in choiceInputs) if (input != null) input.onEndEdit.RemoveAllListeners();
        if (correctAnswerDropdown != null) correctAnswerDropdown.onValueChanged.RemoveAllListeners();
        if (createButton != null) createButton.onClick.RemoveListener(CreateQuestion);
        if (importExcelButton != null)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLFileUploadButton uploadButton = importExcelButton.GetComponent<WebGLFileUploadButton>();
            if (uploadButton != null) uploadButton.FileUploaded = null;
#else
            importExcelButton.onClick.RemoveListener(ImportExcelFile);
#endif
        }
        if (doneButton != null) doneButton.onClick.RemoveListener(FinishQuestionSet);
    }

    /// <summary>Mở hộp chọn file và thêm các câu hỏi từ file .xlsx hoặc .csv.</summary>
    public void ImportExcelFile()
    {
        ExtensionFilter[] extensions =
        {
            new ExtensionFilter("Question files", "xlsx", "csv")
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Import câu hỏi từ Excel",
            string.Empty,
            extensions,
            false);

        if (paths != null && paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            ImportQuestionsFromExcel(paths[0]);
    }

    private void ImportWebGLFile(string url)
    {
        if (!string.IsNullOrEmpty(url)) StartCoroutine(ImportWebGLFileRoutine(url));
    }

    private IEnumerator ImportWebGLFileRoutine(string url)
    {
        string[] fileParts = url.Split(new[] { '|' }, 2);
        string fileName = fileParts.Length > 0 && !string.IsNullOrEmpty(fileParts[0])
            ? System.Uri.UnescapeDataString(fileParts[0])
            : "uploaded.xlsx";
        string fileUrl = fileParts.Length > 1 ? fileParts[1] : url;

        using (UnityWebRequest request = UnityWebRequest.Get(fileUrl))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Không thể đọc file đã chọn: " + request.error);
                yield break;
            }

            if (!ExcelQuestionImporter.TryImport(request.downloadHandler.data, fileName, out E_Topic importedTopic, out List<QuestionData> importedQuestions, out string error))
            {
                Debug.LogWarning(error);
                yield break;
            }

            if (topicDropdown != null) topicDropdown.SetValueWithoutNotify((int)importedTopic);
            if (selectedTopicText != null) selectedTopicText.text = importedTopic.ToString();
            foreach (QuestionData question in importedQuestions) FinishCreateQuestion(question);
            Debug.Log($"Đã import {importedQuestions.Count} câu hỏi từ file được chọn trên WebGL.");
        }
    }

    /// <summary>API dùng cho UI file picker ở bản build và cũng thuận tiện cho test.</summary>
    public bool ImportQuestionsFromExcel(string filePath)
    {
        if (!ExcelQuestionImporter.TryImport(filePath, out E_Topic importedTopic, out List<QuestionData> importedQuestions, out string error))
        {
            Debug.LogWarning(error);
            return false;
        }

        if (topicDropdown != null) topicDropdown.SetValueWithoutNotify((int)importedTopic);
        if (selectedTopicText != null) selectedTopicText.text = importedTopic.ToString();
        foreach (QuestionData question in importedQuestions) FinishCreateQuestion(question);
        Debug.Log($"Đã import {importedQuestions.Count} câu hỏi từ '{Path.GetFileName(filePath)}'.");
        return true;
    }

    public void RefreshTypeEditor()
    {
        bool multi = GetSelectedType() == E_QuestionType.MultiChoices;
        if (fillTheBlankEditor != null) fillTheBlankEditor.SetActive(!multi);
        if (multiChoicesEditor != null) multiChoicesEditor.SetActive(multi);
    }

    public void CreateQuestion()
    {
        if (isSaving) return;
        QuestionData data = ReadEditorData();
        if (data == null) return;
        FinishCreateQuestion(data);
    }

    private void FinishCreateQuestion(QuestionData data)
    {
        questions.Add(data);
        CreateSlot(data);
        ClearEditor();
        selectedQuestion = null;
    }

    public void FinishQuestionSet()
    {
        if (isSaving || questions.Count == 0) return;

        string validationError = ValidateQuestions();
        if (!string.IsNullOrEmpty(validationError))
        {
            Debug.LogWarning("Không thể lưu bộ câu hỏi: " + validationError);
            return;
        }

        if (firebaseRepository == null)
        {
            Debug.LogWarning("FirebaseQuestionRepository chưa được gán.");
            return;
        }

        isSaving = true;
        if (createButton != null) createButton.interactable = false;
        if (doneButton != null) doneButton.interactable = false;
        firebaseRepository.SaveQuestions(questions, (success, message) =>
        {
            isSaving = false;
            if (createButton != null) createButton.interactable = true;
            if (doneButton != null) doneButton.interactable = true;
            if (!success)
            {
                Debug.LogError("Không thể lưu bộ câu hỏi lên Firebase: " + message);
                return;
            }

            Debug.Log("Đã lưu bộ câu hỏi lên Firebase.");
            ResetQuestionView();
            ShowTopicSelection();
        });
    }

    private string ValidateQuestions()
    {
        for (int i = 0; i < questions.Count; i++)
        {
            QuestionData question = questions[i];
            if (string.IsNullOrWhiteSpace(question.Question)) return $"Câu {i + 1} chưa có nội dung.";
            if (question.QuestionType == E_QuestionType.FillTheBlank)
            {
                if (string.IsNullOrWhiteSpace(question.Answer)) return $"Câu {i + 1} chưa có đáp án.";
                continue;
            }

            MultiChoicesData multi = question as MultiChoicesData;
            if (multi == null || multi.Choices == null || multi.Choices.Length != 4)
                return $"Câu {i + 1} phải có đủ 4 lựa chọn.";
            for (int j = 0; j < multi.Choices.Length; j++)
                if (string.IsNullOrWhiteSpace(multi.Choices[j])) return $"Câu {i + 1} thiếu lựa chọn {(char)('A' + j)}.";
            if (string.IsNullOrWhiteSpace(multi.Answer)) return $"Câu {i + 1} chưa chọn đáp án đúng.";
        }
        return string.Empty;
    }

    public void SelectQuestion(QuestionData data)
    {
        selectedQuestion = data;
        WriteDataToEditor(data);
    }

    private void OnQuestionTypeChanged(int value)
    {
        RefreshTypeEditor();
        ApplyEditorToSelectedQuestion();
    }

    private void CreateSlot(QuestionData data)
    {
        if (questionSlotContainer == null || questionSlotPrefab == null) return;
        QuestionSlotUI slot = Instantiate(questionSlotPrefab, questionSlotContainer);
        slot.InitQuestionSlot(data, SelectQuestion, ClearViewedQuestion, DeleteQuestion);
    }

    private void DeleteQuestion(QuestionData data)
    {
        if (data == null) return;

        bool wasSelected = ReferenceEquals(selectedQuestion, data);
        questions.Remove(data);
        if (wasSelected)
        {
            selectedQuestion = null;
            ClearEditor();
        }

        if (questionSlotContainer != null)
        {
            QuestionSlotUI[] slots = questionSlotContainer.GetComponentsInChildren<QuestionSlotUI>(true);
            foreach (QuestionSlotUI slot in slots)
            {
                if (!slot.HasQuestionData(data)) continue;
                Destroy(slot.gameObject);
                break;
            }
        }
    }

    private void ResetQuestionView()
    {
        questions.Clear();
        selectedQuestion = null;
        ClearEditor();

        if (questionSlotContainer != null)
        {
            for (int i = questionSlotContainer.childCount - 1; i >= 0; i--)
                Destroy(questionSlotContainer.GetChild(i).gameObject);
        }

        if (questionTypeDropdown != null)
            questionTypeDropdown.SetValueWithoutNotify((int)E_QuestionType.FillTheBlank);
        RefreshTypeEditor();
    }

    private void RebuildSlots()
    {
        if (questionSlotContainer == null || questionSlotPrefab == null) return;
        for (int i = questionSlotContainer.childCount - 1; i >= 0; i--)
            Destroy(questionSlotContainer.GetChild(i).gameObject);
        for (int i = 0; i < questions.Count; i++) CreateSlot(questions[i]);
    }

    private QuestionData ReadEditorData()
    {
        E_QuestionType type = GetSelectedType();
        TMP_InputField questionField = type == E_QuestionType.FillTheBlank ? fillQuestionInput : multiQuestionInput;
        string text = questionField == null ? string.Empty : questionField.text.Trim();
        if (string.IsNullOrEmpty(text)) return null;
        E_Topic topic = topicDropdown == null ? default : (E_Topic)topicDropdown.value;

        if (type == E_QuestionType.FillTheBlank)
            return new FillTheBlankData { Question = text, Topic = topic, Answer = fillAnswerInput == null ? string.Empty : fillAnswerInput.text.Trim() };

        int count = choiceInputs == null ? 0 : choiceInputs.Length;
        string[] choices = new string[count];
        int correct = correctAnswerDropdown == null ? 0 : Mathf.Clamp(correctAnswerDropdown.value, 0, 3);
        for (int i = 0; i < count; i++)
            choices[i] = choiceInputs[i] == null ? string.Empty : choiceInputs[i].text.Trim();
        E_AnswerOption correctOption = (E_AnswerOption)correct;
        return new MultiChoicesData { Question = text, Topic = topic, Choices = choices, CorrectOption = correctOption, Answer = correct < choices.Length ? choices[correct] : string.Empty };
    }

    private void WriteDataToEditor(QuestionData data)
    {
        if (topicDropdown != null) topicDropdown.SetValueWithoutNotify((int)data.Topic);
        // Khi review, editor phải chuyển đúng loại của câu hỏi đang chọn.
        if (questionTypeDropdown != null)
            questionTypeDropdown.SetValueWithoutNotify((int)data.QuestionType);
        RefreshTypeEditor();

        if (data.QuestionType == E_QuestionType.FillTheBlank)
        {
            if (fillQuestionInput != null) fillQuestionInput.text = data.Question;
            if (fillAnswerInput != null) fillAnswerInput.text = data.Answer;
            return;
        }
        MultiChoicesData multi = data as MultiChoicesData;
        if (multiQuestionInput != null) multiQuestionInput.text = data.Question;
        for (int i = 0; choiceInputs != null && i < choiceInputs.Length; i++)
            if (choiceInputs[i] != null) choiceInputs[i].text = multi != null && multi.Choices != null && i < multi.Choices.Length ? multi.Choices[i] : string.Empty;
        if (correctAnswerDropdown != null)
        {
            int correctIndex = multi == null ? 0 : (int)multi.CorrectOption;
            if (multi != null && multi.Choices != null && !string.IsNullOrEmpty(data.Answer))
                for (int i = 0; i < multi.Choices.Length; i++) if (multi.Choices[i] == data.Answer) correctIndex = i;
            correctAnswerDropdown.SetValueWithoutNotify(Mathf.Clamp(correctIndex, 0, 3));
        }
    }

    private void ClearEditor()
    {
        if (fillQuestionInput != null) fillQuestionInput.text = string.Empty;
        if (multiQuestionInput != null) multiQuestionInput.text = string.Empty;
        if (fillAnswerInput != null) fillAnswerInput.text = string.Empty;
        if (choiceInputs != null) foreach (TMP_InputField input in choiceInputs) if (input != null) input.text = string.Empty;
        if (correctAnswerDropdown != null) correctAnswerDropdown.SetValueWithoutNotify(0);
    }

    private void ClearViewedQuestion()
    {
        if (selectedQuestion == null) return;
        selectedQuestion = null;
        ClearEditor();
    }

    private void ApplyEditorToSelectedQuestion()
    {
        if (selectedQuestion == null) return;
        QuestionData updated = ReadEditorData();
        if (updated == null) return;
        int index = questions.IndexOf(selectedQuestion);
        if (index < 0) return;
        QuestionData oldData = selectedQuestion;
        questions[index] = updated;
        selectedQuestion = updated;
        if (questionSlotContainer != null)
        {
            QuestionSlotUI[] slots = questionSlotContainer.GetComponentsInChildren<QuestionSlotUI>(true);
            foreach (QuestionSlotUI slot in slots)
                if (slot.HasQuestionData(oldData)) slot.SetQuestionData(updated);
        }
    }

    private E_QuestionType GetSelectedType() => questionTypeDropdown == null ? E_QuestionType.FillTheBlank : (E_QuestionType)questionTypeDropdown.value;
}
