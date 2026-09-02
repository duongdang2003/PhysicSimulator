using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestionSlotUI : MonoBehaviour, IDeselectHandler
{
    [SerializeField] private Image _questionType;
    [SerializeField] private Sprite _multiChoicesIcon;
    [SerializeField] private Sprite _fillTheBlankIcon;

    private Button _btn;
    private QuestionData _questionData;
    private Action<QuestionData> _onSelected;
    private Action _onDeselected;

    private void Awake() => _btn = GetComponent<Button>();

    public void InitQuestionSlot(QuestionData data, Action<QuestionData> onSelected = null, Action onDeselected = null)
    {
        _questionData = data;
        _onSelected = onSelected;
        _onDeselected = onDeselected;
        if (_btn == null) _btn = GetComponent<Button>();
        _btn.onClick.RemoveListener(ShowDataOnQuestionEditor);
        _btn.onClick.AddListener(ShowDataOnQuestionEditor);

        if (_questionType != null && data != null)
            _questionType.sprite = data.QuestionType == E_QuestionType.MultiChoices ? _multiChoicesIcon : _fillTheBlankIcon;
    }

    public void ShowDataOnQuestionEditor() => _onSelected?.Invoke(_questionData);
    public void SetQuestionData(QuestionData data) => _questionData = data;
    public bool HasQuestionData(QuestionData data) => ReferenceEquals(_questionData, data);

    public void OnDeselect(BaseEventData eventData) => _onDeselected?.Invoke();

    private void OnDestroy()
    {
        if (_btn != null) _btn.onClick.RemoveListener(ShowDataOnQuestionEditor);
    }
}
