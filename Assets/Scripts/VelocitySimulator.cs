using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class VelocitySimulator : MonoBehaviour
{
    [SerializeField] private TMP_InputField velocityInput;
    [SerializeField] private TMP_InputField distanceInput;
    [SerializeField] private Button simulateButton;
    [SerializeField] private TextMeshProUGUI timeResultText;
    [SerializeField] private Transform ball;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    private bool isSimulating = false;

    private void Start()
    {
        if (simulateButton != null)
        {
            simulateButton.onClick.AddListener(OnSimulateClick);
        }
    }

    private void OnSimulateClick()
    {
        if (isSimulating) return;

        if (!float.TryParse(velocityInput.text, out float velocity) || velocity <= 0)
        {
            Debug.LogError("Invalid velocity input. Please enter a positive number.");
            timeResultText.text = "Vận tốc không hợp lệ!";
            return;
        }

        if (!float.TryParse(distanceInput.text, out float distance) || distance <= 0)
        {
            Debug.LogError("Invalid distance input. Please enter a positive number.");
            timeResultText.text = "Khoảng cách không hợp lệ!";
            return;
        }

        if (ball == null || startPoint == null || endPoint == null)
        {
            Debug.LogError("Ball, StartPoint, or EndPoint is not assigned!");
            return;
        }

        float time = distance / velocity;
        velocityInput.text += "km/h";

        StartCoroutine(MoveBall(time));
    }

    private IEnumerator MoveBall(float duration)
    {
        isSimulating = true;
        float elapsedTime = 0f;
        Vector3 startPos = startPoint.position + Vector3.up * 0.5f;
        Vector3 endPos = endPoint.position + Vector3.up * 0.5f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            ball.position = Vector3.Lerp(startPos, endPos, t);
            timeResultText.text = $"Thời gian: {elapsedTime:F2}h";
            yield return null;
        }

        ball.position = endPos;
        timeResultText.text = $"Thời gian: {elapsedTime:F2}h";
        isSimulating = false;
    }
}
