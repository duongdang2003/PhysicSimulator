using System;
using System.Collections.Generic;
using System.Text;
#if !UNITY_WEBGL
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
#endif
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Lưu và load question set bằng Firebase Unity SDK.
/// Cấu trúc:
/// questionSets/{topic}/{ownerId}/{setId}/metadata
/// questionSets/{topic}/{ownerId}/{setId}/questions/{questionId}
/// </summary>
#if UNITY_WEBGL
/// <summary>
/// Firebase Unity SDK does not support WebGL. This keeps scene references and
/// callers valid while the WebGL build uses the local Resources fallback.
/// </summary>
public class FirebaseQuestionRepository : MonoBehaviour
{
    private const string DatabaseUrl = "https://physic-simulator-bd7a4-default-rtdb.asia-southeast1.firebasedatabase.app";

    public void SaveQuestions(IList<QuestionData> questions, Action<bool, string> completed = null)
    {
        if (questions == null || questions.Count == 0)
        {
            completed?.Invoke(false, "Question list is empty.");
            return;
        }

        string ownerId = UserSession.Instance == null ? string.Empty : UserSession.Instance.UserID;
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            completed?.Invoke(false, "User chưa đăng nhập PlayFab.");
            return;
        }

        E_Topic topic = questions[0].Topic;
        for (int i = 0; i < questions.Count; i++)
        {
            if (questions[i].Topic != topic)
            {
                completed?.Invoke(false, "Một question set không được chứa nhiều chủ đề.");
                return;
            }
        }

        string setId = Guid.NewGuid().ToString("N");
        string path = $"questionSets/{EscapePath(topic.ToString())}/{EscapePath(ownerId)}/{setId}.json";
        string json = BuildQuestionSetJson(questions, ownerId, topic, setId);
        StartCoroutine(PutQuestionSet(path, json, setId, completed));
    }

    public void LoadRandomQuestionSet(E_Topic topic, Action<List<QuestionData>, string> completed)
    {
        completed?.Invoke(null, "Firebase is not supported in WebGL builds.");
    }

    private System.Collections.IEnumerator PutQuestionSet(string path, string json, string setId, Action<bool, string> completed)
    {
        using (UnityWebRequest request = UnityWebRequest.Put($"{DatabaseUrl}/{path}", Encoding.UTF8.GetBytes(json)))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                completed?.Invoke(false, request.error);
                yield break;
            }

            completed?.Invoke(true, setId);
        }
    }

    private static string BuildQuestionSetJson(IList<QuestionData> questions, string ownerId, E_Topic topic, string setId)
    {
        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var builder = new StringBuilder();
        builder.Append("{\"metadata\":{")
            .Append("\"ownerId\":").Append(JsonString(ownerId)).Append(',')
            .Append("\"topic\":").Append(JsonString(topic.ToString())).Append(',')
            .Append("\"setId\":").Append(JsonString(setId)).Append(',')
            .Append("\"questionCount\":").Append(questions.Count).Append(',')
            .Append("\"createdAt\":").Append(createdAt)
            .Append("},\"questions\":{");

        for (int i = 0; i < questions.Count; i++)
        {
            if (i > 0) builder.Append(',');
            builder.Append(JsonString($"q_{i + 1:000}")).Append(':');
            AppendQuestionJson(builder, questions[i], ownerId, topic, createdAt);
        }

        return builder.Append("}}")
            .ToString();
    }

    private static void AppendQuestionJson(StringBuilder builder, QuestionData data, string ownerId, E_Topic topic, long createdAt)
    {
        MultiChoicesData multi = data as MultiChoicesData;
        builder.Append('{')
            .Append("\"ownerId\":").Append(JsonString(ownerId)).Append(',')
            .Append("\"topic\":").Append(JsonString(topic.ToString())).Append(',')
            .Append("\"questionType\":").Append(JsonString(data.QuestionType.ToString())).Append(',')
            .Append("\"question\":").Append(JsonString(data.Question)).Append(',')
            .Append("\"answer\":").Append(JsonString(data.Answer)).Append(',')
            .Append("\"choices\":[");

        if (multi != null && multi.Choices != null)
        {
            for (int i = 0; i < multi.Choices.Length; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(JsonString(multi.Choices[i]));
            }
        }

        builder.Append("],\"correctOption\":")
            .Append(JsonString(multi == null ? string.Empty : multi.CorrectOption.ToString()))
            .Append(",\"createdAt\":")
            .Append(createdAt)
            .Append('}');
    }

    private static string JsonString(string value)
    {
        if (value == null) return "null";
        return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"")
            .Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t") + "\"";
    }

    private static string EscapePath(string value) => Uri.EscapeDataString(value ?? string.Empty);
}
#else
public class FirebaseQuestionRepository : MonoBehaviour
{
    private FirebaseDatabase database;
    private bool isInitialized;
    private string initializationError;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                initializationError = task.Exception == null ? "Firebase initialization failed." : task.Exception.GetBaseException().Message;
                Debug.LogError(initializationError);
                return;
            }

            if (task.Result != DependencyStatus.Available)
            {
                initializationError = "Firebase dependencies unavailable: " + task.Result;
                Debug.LogError(initializationError);
                return;
            }

            database = FirebaseDatabase.DefaultInstance;
            isInitialized = true;
        });
    }

    /// <summary>Lưu toàn bộ danh sách thành một bộ đề riêng.</summary>
    public void SaveQuestions(IList<QuestionData> questions, Action<bool, string> completed = null)
    {
        if (questions == null || questions.Count == 0)
        {
            completed?.Invoke(false, "Question list is empty.");
            return;
        }
        if (!isInitialized)
        {
            completed?.Invoke(false, string.IsNullOrEmpty(initializationError) ? "Firebase is still initializing." : initializationError);
            return;
        }

        string ownerId = UserSession.Instance == null ? string.Empty : UserSession.Instance.UserID;
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            completed?.Invoke(false, "User chưa đăng nhập PlayFab.");
            return;
        }

        E_Topic topic = questions[0].Topic;
        string topicKey = topic.ToString();
        string setId = database.RootReference.Child("questionSets").Child(topicKey).Child(ownerId).Push().Key;
        long createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var updates = new Dictionary<string, object>
        {
            [$"questionSets/{topicKey}/{ownerId}/{setId}/metadata"] = new Dictionary<string, object>
            {
                { "ownerId", ownerId },
                { "topic", topicKey },
                { "setId", setId },
                { "questionCount", questions.Count },
                { "createdAt", createdAt }
            }
        };

        for (int i = 0; i < questions.Count; i++)
        {
            QuestionData question = questions[i];
            if (question.Topic != topic)
            {
                completed?.Invoke(false, "Một question set không được chứa nhiều chủ đề.");
                return;
            }

            string questionId = $"q_{i + 1:000}";
            FirebaseQuestionRecord record = FirebaseQuestionRecord.From(question, ownerId, topicKey, createdAt);
            updates[$"questionSets/{topicKey}/{ownerId}/{setId}/questions/{questionId}"] = record.ToDictionary();
        }

        database.RootReference.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                string error = task.Exception == null ? "Firebase question set write failed." : task.Exception.GetBaseException().Message;
                Debug.LogError(error);
                completed?.Invoke(false, error);
                return;
            }
            completed?.Invoke(true, setId);
        });
    }

    /// <summary>
    /// Chọn ngẫu nhiên một owner trong topic, sau đó chọn ngẫu nhiên một set của owner đó.
    /// </summary>
    public void LoadRandomQuestionSet(E_Topic topic, Action<List<QuestionData>, string> completed)
    {
        if (!isInitialized)
        {
            completed?.Invoke(null, string.IsNullOrEmpty(initializationError) ? "Firebase is still initializing." : initializationError);
            return;
        }

        database.RootReference.Child("questionSets").Child(topic.ToString()).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    completed?.Invoke(null, task.Exception == null ? "Cannot load question set index." : task.Exception.GetBaseException().Message);
                    return;
                }

                var owners = new List<DataSnapshot>();
                foreach (DataSnapshot owner in task.Result.Children) owners.Add(owner);
                if (owners.Count == 0)
                {
                    completed?.Invoke(null, "Chủ đề chưa có question set.");
                    return;
                }

                DataSnapshot selectedOwner = owners[UnityEngine.Random.Range(0, owners.Count)];
                var sets = new List<DataSnapshot>();
                foreach (DataSnapshot set in selectedOwner.Children) sets.Add(set);
                if (sets.Count == 0)
                {
                    completed?.Invoke(null, "User này chưa có question set.");
                    return;
                }

                string ownerId = selectedOwner.Key;
                string setId = sets[UnityEngine.Random.Range(0, sets.Count)].Key;
                LoadQuestionSet(topic, ownerId, setId, completed);
            });
    }

    private void LoadQuestionSet(E_Topic topic, string ownerId, string setId, Action<List<QuestionData>, string> completed)
    {
        database.RootReference.Child("questionSets").Child(topic.ToString()).Child(ownerId).Child(setId).Child("questions").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    completed?.Invoke(null, task.Exception == null ? "Cannot load questions." : task.Exception.GetBaseException().Message);
                    return;
                }

                var questions = new List<QuestionData>();
                foreach (DataSnapshot snapshot in task.Result.Children)
                {
                    QuestionData question = ParseQuestion(snapshot, topic);
                    if (question != null) questions.Add(question);
                }
                completed?.Invoke(questions, questions.Count == 0 ? "Question set is empty." : string.Empty);
            });
    }

    private static QuestionData ParseQuestion(DataSnapshot snapshot, E_Topic topic)
    {
        string type = snapshot.Child("questionType").Value == null ? string.Empty : snapshot.Child("questionType").Value.ToString();
        string text = snapshot.Child("question").Value == null ? string.Empty : snapshot.Child("question").Value.ToString();
        string answer = snapshot.Child("answer").Value == null ? string.Empty : snapshot.Child("answer").Value.ToString();
        if (type == E_QuestionType.FillTheBlank.ToString())
            return new FillTheBlankData { Question = text, Answer = answer, Topic = topic };

        var choices = new List<string>();
        foreach (DataSnapshot choice in snapshot.Child("choices").Children)
            choices.Add(choice.Value == null ? string.Empty : choice.Value.ToString());
        E_AnswerOption correctOption = E_AnswerOption.A;
        string correct = snapshot.Child("correctOption").Value == null ? string.Empty : snapshot.Child("correctOption").Value.ToString();
        Enum.TryParse(correct, out correctOption);
        return new MultiChoicesData { Question = text, Answer = answer, Choices = choices.ToArray(), CorrectOption = correctOption, Topic = topic };
    }

    [Serializable]
    private class FirebaseQuestionRecord
    {
        public string ownerId;
        public string topic;
        public string questionType;
        public string question;
        public string answer;
        public string[] choices;
        public string correctOption;
        public long createdAt;

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "ownerId", ownerId },
                { "topic", topic },
                { "questionType", questionType },
                { "question", question },
                { "answer", answer },
                { "choices", choices == null ? new List<string>() : new List<string>(choices) },
                { "correctOption", correctOption },
                { "createdAt", createdAt }
            };
        }

        public static FirebaseQuestionRecord From(QuestionData data, string ownerId, string topic, long createdAt)
        {
            MultiChoicesData multi = data as MultiChoicesData;
            return new FirebaseQuestionRecord
            {
                ownerId = ownerId,
                topic = topic,
                questionType = data.QuestionType.ToString(),
                question = data.Question,
                answer = data.Answer,
                choices = multi == null ? new string[0] : multi.Choices,
                correctOption = multi == null ? string.Empty : multi.CorrectOption.ToString(),
                createdAt = createdAt
            };
        }
    }
}
#endif
