using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestionSlotUI : MonoBehaviour, IDeselectHandler
{
    [SerializeField] private Image _questionType;
    [SerializeField] private Sprite _multiChoicesIcon;
    [SerializeField] private Sprite _fillTheBlankIcon;
    [SerializeField] private Button _deleteButton;

    private Button _btn;
    private QuestionData _questionData;
    private Action<QuestionData> _onSelected;
    private Action<QuestionData> _onDeleted;
    private Action _onDeselected;

    private void Awake() => _btn = GetComponent<Button>();

    public void InitQuestionSlot(QuestionData data, Action<QuestionData> onSelected = null, Action onDeselected = null, Action<QuestionData> onDeleted = null)
    {
        _questionData = data;
        _onSelected = onSelected;
        _onDeselected = onDeselected;
        _onDeleted = onDeleted;
        if (_btn == null) _btn = GetComponent<Button>();
        _btn.onClick.RemoveListener(ShowDataOnQuestionEditor);
        _btn.onClick.AddListener(ShowDataOnQuestionEditor);

        if (_deleteButton == null)
            _deleteButton = transform.Find("CloseBtn")?.GetComponent<Button>();
        if (_deleteButton != null)
        {
            _deleteButton.onClick.RemoveListener(DeleteQuestion);
            _deleteButton.onClick.AddListener(DeleteQuestion);
        }

        if (_questionType != null && data != null)
            _questionType.sprite = data.QuestionType == E_QuestionType.MultiChoices ? _multiChoicesIcon : _fillTheBlankIcon;
    }

    public void ShowDataOnQuestionEditor() => _onSelected?.Invoke(_questionData);
    public void DeleteQuestion() => _onDeleted?.Invoke(_questionData);
    public void SetQuestionData(QuestionData data) => _questionData = data;
    public bool HasQuestionData(QuestionData data) => ReferenceEquals(_questionData, data);

    public void OnDeselect(BaseEventData eventData) => _onDeselected?.Invoke();

    private void OnDestroy()
    {
        if (_btn != null) _btn.onClick.RemoveListener(ShowDataOnQuestionEditor);
        if (_deleteButton != null) _deleteButton.onClick.RemoveListener(DeleteQuestion);
    }
}
