using UnityEngine;
using System.Collections.Generic;

public class QuestionLoader : MonoBehaviour
{
    [Header("Question File Paths")]
    public string standardQuestionsPath = "questions";
    public string playerQuestionsPath = "playerqs";
    public string pictureQuestionsPath = "picqs";
    public string bonusQuestionsPath = "bonusquestions";
    
    void Awake()
    {
        LoadAllQuestions();
    }
    
    void LoadAllQuestions()
    {
        // Load standard questions
        LoadStandardQuestions();
        
        // Load player questions
        LoadPlayerQuestions();
        
        // Load picture questions
        LoadPictureQuestions();
        
        // Load bonus questions
        LoadBonusQuestions();
        
        Debug.Log("All questions loaded successfully");
    }
    
    void LoadStandardQuestions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(standardQuestionsPath);
        
        if (jsonFile == null)
        {
            Debug.LogError("Could not find questions.json at Resources/" + standardQuestionsPath);
            return;
        }
        
        // Parse the array format: [{"QUESTION": ..., "CORRECT ANSWER": ..., "ROBOT ANSWER": ...}]
        QuestionArrayWrapper wrapper = new QuestionArrayWrapper();
        wrapper.questions = ParseQuestionArray(jsonFile.text);
        
        if (wrapper.questions != null && wrapper.questions.Count > 0)
        {
            GameManager.Instance.standardQuestions = wrapper.questions;
            Debug.Log("Loaded " + wrapper.questions.Count + " standard questions");
        }
        else
        {
            Debug.LogError("Failed to parse standard questions JSON");
        }
    }
    
    void LoadPlayerQuestions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(playerQuestionsPath);
        
        if (jsonFile == null)
        {
            Debug.LogError("Could not find playerqs.json at Resources/" + playerQuestionsPath);
            return;
        }
        
        QuestionArrayWrapper wrapper = new QuestionArrayWrapper();
        wrapper.questions = ParseQuestionArray(jsonFile.text);
        
        if (wrapper.questions != null && wrapper.questions.Count > 0)
        {
            GameManager.Instance.playerQuestions = wrapper.questions;
            Debug.Log("Loaded " + wrapper.questions.Count + " player questions");
        }
        else
        {
            Debug.LogError("Failed to parse player questions JSON");
        }
    }
    
    void LoadPictureQuestions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(pictureQuestionsPath);
        
        if (jsonFile == null)
        {
            Debug.LogError("Could not find picqs.json at Resources/" + pictureQuestionsPath);
            return;
        }
        
        QuestionArrayWrapper wrapper = new QuestionArrayWrapper();
        wrapper.questions = ParseQuestionArray(jsonFile.text);
        
        if (wrapper.questions != null && wrapper.questions.Count > 0)
        {
            GameManager.Instance.pictureQuestions = wrapper.questions;
            Debug.Log("Loaded " + wrapper.questions.Count + " picture questions");
        }
        else
        {
            Debug.LogError("Failed to parse picture questions JSON");
        }
    }
    
    void LoadBonusQuestions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(bonusQuestionsPath);
        
        if (jsonFile == null)
        {
            Debug.LogError("Could not find bonusquestions.json at Resources/" + bonusQuestionsPath);
            return;
        }
        
        BonusQuestion bonusData = JsonUtility.FromJson<BonusQuestion>(jsonFile.text);
        
        if (bonusData != null && bonusData.miniQuestions != null)
        {
            GameManager.Instance.bonusQuestions = bonusData;
            Debug.Log("Loaded " + bonusData.miniQuestions.Count + " bonus questions");
        }
        else
        {
            Debug.LogError("Failed to parse bonus questions JSON");
        }
    }
    
    List<Question> ParseQuestionArray(string json)
    {
        // The JSON is an array like: [{"QUESTION": ..., "CORRECT ANSWER": ..., "ROBOT ANSWER": ...}, ...]
        // We need to parse it manually since Unity's JsonUtility doesn't handle root arrays
        
        List<Question> questions = new List<Question>();
        
        try
        {
            // Remove outer brackets and split by objects
            json = json.Trim();
            if (json.StartsWith("[")) json = json.Substring(1);
            if (json.EndsWith("]")) json = json.Substring(0, json.Length - 1);
            
            // Split by },{ pattern
            string[] questionObjects = json.Split(new string[] { "},{" }, System.StringSplitOptions.None);
            
            foreach (string objStr in questionObjects)
            {
                // Add back the braces
                string jsonObj = objStr.Trim();
                if (!jsonObj.StartsWith("{")) jsonObj = "{" + jsonObj;
                if (!jsonObj.EndsWith("}")) jsonObj = jsonObj + "}";
                
                // Parse individual question
                RawQuestionData raw = JsonUtility.FromJson<RawQuestionData>(jsonObj);
                
                if (raw != null)
                {
                    Question q = new Question();
                    q.questionText = raw.QUESTION;
                    q.correctAnswer = raw.CORRECT_ANSWER;
                    q.robotAnswer = raw.ROBOT_ANSWER;
                    q.robotAnecdote = ""; // Not in current data structure
                    q.questionType = "standard";
                    q.imageURL = ""; // For picture questions, would be added
                    
                    questions.Add(q);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing question array: " + e.Message);
        }
        
        return questions;
    }
}

// === JSON DATA STRUCTURES ===

[System.Serializable]
public class RawQuestionData
{
    public string QUESTION;
    public string CORRECT_ANSWER;
    public string ROBOT_ANSWER;
}

[System.Serializable]
public class QuestionArrayWrapper
{
    public List<Question> questions;
}
