using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

/// <summary>Đọc workbook .xlsx theo format question template.xlsx.</summary>
public static class ExcelQuestionImporter
{
    public static bool TryImport(string filePath, out E_Topic topic, out List<QuestionData> questions, out string error)
    {
        topic = default;
        questions = new List<QuestionData>();
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            error = "Không tìm thấy file Excel.";
            return false;
        }

        string extension = Path.GetExtension(filePath);
        if (string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return TryImport(File.ReadAllBytes(filePath), Path.GetFileName(filePath), out topic, out questions, out error);
            }
            catch (Exception exception)
            {
                error = "Không thể đọc file CSV: " + exception.Message;
                return false;
            }
        }

        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            error = "Chỉ hỗ trợ file Excel .xlsx hoặc .csv.";
            return false;
        }

        try
        {
            // OneDrive/Excel có thể giữ file với shared read/write. Mở bằng FileStream
            // với FileShare đầy đủ để không bị sharing violation khi import file đồng bộ.
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                List<string> sharedStrings = ReadSharedStrings(archive);
                ZipArchiveEntry sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
                if (sheetEntry == null)
                {
                    error = "Không tìm thấy worksheet đầu tiên trong file Excel.";
                    return false;
                }

                List<List<string>> rows = ReadRows(sheetEntry, sharedStrings);
                if (rows.Count == 0 || rows[0].Count == 0 || string.IsNullOrWhiteSpace(rows[0][0]))
                {
                    error = "Dòng đầu tiên phải chứa chủ đề.";
                    return false;
                }

                if (!Enum.TryParse(rows[0][0].Trim(), true, out topic))
                {
                    error = $"Chủ đề '{rows[0][0].Trim()}' không tồn tại trong E_Topic.";
                    return false;
                }

                for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
                {
                    List<string> row = rows[rowIndex];
                    if (IsEmpty(row)) continue;

                    string type = GetCell(row, 0);
                    string question = GetCell(row, 1);
                    if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(question))
                    {
                        error = $"Dòng {rowIndex + 1}: thiếu loại câu hỏi hoặc nội dung câu hỏi.";
                        return false;
                    }

                    if (type.Equals("Multi choices", StringComparison.OrdinalIgnoreCase))
                    {
                        if (row.Count < 7)
                        {
                            error = $"Dòng {rowIndex + 1}: câu Multi choices phải có 4 đáp án và đáp án đúng.";
                            return false;
                        }

                        string[] choices = new string[4];
                        for (int choiceIndex = 0; choiceIndex < choices.Length; choiceIndex++)
                        {
                            choices[choiceIndex] = GetCell(row, choiceIndex + 2);
                            if (string.IsNullOrWhiteSpace(choices[choiceIndex]))
                            {
                                error = $"Dòng {rowIndex + 1}: thiếu đáp án {(char)('A' + choiceIndex)}.";
                                return false;
                            }
                        }

                        string correctCell = GetCell(row, 6);
                        int correctIndex = ParseAnswerIndex(correctCell);
                        if (correctIndex < 0)
                        {
                            error = $"Dòng {rowIndex + 1}: đáp án đúng phải là A, B, C hoặc D.";
                            return false;
                        }

                        questions.Add(new MultiChoicesData
                        {
                            Question = question.Trim(),
                            Topic = topic,
                            Choices = choices,
                            CorrectOption = (E_AnswerOption)correctIndex,
                            Answer = choices[correctIndex]
                        });
                    }
                    else if (type.Equals("Fill the blank", StringComparison.OrdinalIgnoreCase))
                    {
                        string answer = GetCell(row, 2);
                        if (string.IsNullOrWhiteSpace(answer))
                        {
                            error = $"Dòng {rowIndex + 1}: thiếu đáp án điền vào chỗ trống.";
                            return false;
                        }

                        questions.Add(new FillTheBlankData
                        {
                            Question = question.Trim(),
                            Topic = topic,
                            Answer = answer.Trim()
                        });
                    }
                    else
                    {
                        error = $"Dòng {rowIndex + 1}: loại câu hỏi '{type}' không được hỗ trợ.";
                        return false;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            error = "Không thể đọc file Excel: " + exception.Message;
            questions.Clear();
            return false;
        }

        if (questions.Count == 0)
        {
            error = "File Excel không chứa câu hỏi nào.";
            return false;
        }

        return true;
    }

    public static bool TryImport(byte[] fileBytes, string fileName, out E_Topic topic, out List<QuestionData> questions, out string error)
    {
        topic = default;
        questions = new List<QuestionData>();
        error = string.Empty;

        if (fileBytes == null || fileBytes.Length == 0)
        {
            error = "File được chọn đang trống.";
            return false;
        }

        // WebGL receives the selected file as bytes.
        string extension = Path.GetExtension(fileName);
        if (string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
        {
            return TryImportCsv(fileBytes, out topic, out questions, out error);
        }

        string temporaryPath = Path.Combine(UnityEngine.Application.persistentDataPath, "question-import.xlsx");
        try
        {
            File.WriteAllBytes(temporaryPath, fileBytes);
            return TryImport(temporaryPath, out topic, out questions, out error);
        }
        catch (Exception exception)
        {
            error = "Không thể lưu file import tạm thời: " + exception.Message;
            return false;
        }
    }

    private static bool TryImportCsv(byte[] fileBytes, out E_Topic topic, out List<QuestionData> questions, out string error)
    {
        topic = default;
        questions = new List<QuestionData>();
        error = string.Empty;

        try
        {
            string csv = Encoding.UTF8.GetString(fileBytes).TrimStart('\uFEFF');
            List<List<string>> rows = ReadCsvRows(csv);
            if (rows.Count == 0 || rows[0].Count == 0 || string.IsNullOrWhiteSpace(rows[0][0]))
            {
                error = "Dòng đầu tiên phải chứa chủ đề.";
                return false;
            }

            if (!Enum.TryParse(rows[0][0].Trim(), true, out topic))
            {
                error = $"Chủ đề '{rows[0][0].Trim()}' không tồn tại trong E_Topic.";
                return false;
            }

            for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                List<string> row = rows[rowIndex];
                if (IsEmpty(row)) continue;

                string type = GetCell(row, 0);
                string question = GetCell(row, 1);
                if (type.Equals("Multi choices", StringComparison.OrdinalIgnoreCase))
                {
                    if (row.Count < 7)
                    {
                        error = $"Dòng {rowIndex + 1}: câu Multi choices phải có 4 đáp án và đáp án đúng.";
                        return false;
                    }

                    string[] choices = new string[4];
                    for (int choiceIndex = 0; choiceIndex < choices.Length; choiceIndex++)
                        choices[choiceIndex] = GetCell(row, choiceIndex + 2);
                    int correctIndex = ParseAnswerIndex(GetCell(row, 6));
                    bool hasEmptyChoice = false;
                    for (int choiceIndex = 0; choiceIndex < choices.Length; choiceIndex++)
                        if (string.IsNullOrWhiteSpace(choices[choiceIndex])) hasEmptyChoice = true;
                    if (string.IsNullOrWhiteSpace(question) || correctIndex < 0 || hasEmptyChoice)
                    {
                        error = $"Dòng {rowIndex + 1}: dữ liệu câu hỏi không hợp lệ.";
                        return false;
                    }

                    questions.Add(new MultiChoicesData
                    {
                        Question = question,
                        Topic = topic,
                        Choices = choices,
                        CorrectOption = (E_AnswerOption)correctIndex,
                        Answer = choices[correctIndex]
                    });
                }
                else if (type.Equals("Fill the blank", StringComparison.OrdinalIgnoreCase))
                {
                    string answer = GetCell(row, 2);
                    if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
                    {
                        error = $"Dòng {rowIndex + 1}: thiếu nội dung câu hỏi hoặc đáp án.";
                        return false;
                    }

                    questions.Add(new FillTheBlankData { Question = question, Topic = topic, Answer = answer });
                }
                else
                {
                    error = $"Dòng {rowIndex + 1}: loại câu hỏi '{type}' không được hỗ trợ.";
                    return false;
                }
            }
        }
        catch (Exception exception)
        {
            error = "Không thể đọc file CSV: " + exception.Message;
            questions.Clear();
            return false;
        }

        if (questions.Count == 0) error = "File CSV không chứa câu hỏi nào.";
        return questions.Count > 0;
    }

    private static List<List<string>> ReadCsvRows(string csv)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var value = new StringBuilder();
        bool quoted = false;

        for (int i = 0; i < csv.Length; i++)
        {
            char character = csv[i];
            if (character == '"')
            {
                if (quoted && i + 1 < csv.Length && csv[i + 1] == '"') { value.Append('"'); i++; }
                else quoted = !quoted;
            }
            else if (character == ',' && !quoted)
            {
                row.Add(value.ToString().Trim());
                value.Clear();
            }
            else if ((character == '\n' || character == '\r') && !quoted)
            {
                if (character == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
                row.Add(value.ToString().Trim());
                value.Clear();
                rows.Add(row);
                row = new List<string>();
            }
            else value.Append(character);
        }

        if (value.Length > 0 || row.Count > 0)
        {
            row.Add(value.ToString().Trim());
            rows.Add(row);
        }
        return rows;
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var result = new List<string>();
        ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry == null) return result;

        var document = new XmlDocument();
        using (Stream stream = entry.Open()) document.Load(stream);
        foreach (XmlNode stringNode in document.GetElementsByTagName("si"))
        {
            var builder = new StringBuilder();
            foreach (XmlNode textNode in stringNode.SelectNodes(".//*[local-name()='t']"))
                builder.Append(textNode.InnerText);
            result.Add(builder.ToString());
        }
        return result;
    }

    private static List<List<string>> ReadRows(ZipArchiveEntry entry, List<string> sharedStrings)
    {
        var rows = new List<List<string>>();
        var document = new XmlDocument();
        using (Stream stream = entry.Open()) document.Load(stream);

        foreach (XmlNode rowNode in document.GetElementsByTagName("row"))
        {
            var values = new List<string>();
            int nextColumn = 0;
            foreach (XmlNode cellNode in rowNode.ChildNodes)
            {
                if (!string.Equals(cellNode.LocalName, "c", StringComparison.Ordinal)) continue;
                string reference = cellNode.Attributes?["r"]?.Value;
                int column = GetColumnIndex(reference);
                while (nextColumn < column) { values.Add(string.Empty); nextColumn++; }
                values.Add(ReadCellValue(cellNode, sharedStrings));
                nextColumn = column + 1;
            }
            rows.Add(values);
        }
        return rows;
    }

    private static string ReadCellValue(XmlNode cellNode, List<string> sharedStrings)
    {
        string type = cellNode.Attributes?["t"]?.Value;
        if (type == "inlineStr")
        {
            var builder = new StringBuilder();
            foreach (XmlNode textNode in cellNode.SelectNodes(".//*[local-name()='t']")) builder.Append(textNode.InnerText);
            return builder.ToString();
        }

        XmlNode valueNode = cellNode.SelectSingleNode("./*[local-name()='v']");
        string value = valueNode?.InnerText ?? string.Empty;
        if (type == "s" && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index) && index >= 0 && index < sharedStrings.Count)
            return sharedStrings[index];
        return value;
    }

    private static int GetColumnIndex(string reference)
    {
        if (string.IsNullOrEmpty(reference)) return 0;
        int index = 0;
        for (int i = 0; i < reference.Length && char.IsLetter(reference[i]); i++)
            index = index * 26 + (char.ToUpperInvariant(reference[i]) - 'A' + 1);
        return Math.Max(0, index - 1);
    }

    private static int ParseAnswerIndex(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return -1;
        string answer = value.Trim().ToUpperInvariant();
        if (answer.Length == 1 && answer[0] >= 'A' && answer[0] <= 'D') return answer[0] - 'A';
        return int.TryParse(answer, NumberStyles.Integer, CultureInfo.InvariantCulture, out int numeric) && numeric >= 0 && numeric <= 3 ? numeric : -1;
    }

    private static string GetCell(List<string> row, int index) => index >= 0 && index < row.Count ? row[index]?.Trim() ?? string.Empty : string.Empty;

    private static bool IsEmpty(List<string> row)
    {
        for (int i = 0; i < row.Count; i++) if (!string.IsNullOrWhiteSpace(row[i])) return false;
        return true;
    }
}
