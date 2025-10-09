using UnityEngine;
using System.Collections.Generic;

public class QuestionLoader : MonoBehaviour
{
    private GameManager gameManager;
    [Header("Question File Paths")]
    public string standardQuestionsPath = "questions";
    public string playerQuestionsPath = "playerqs";
    public string pictureQuestionsPath = "picqs";
    public string bonusQuestionsPath = "bonusquestions";

    void Awake()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("QuestionLoader could not find an active GameManager. Questions will not be loaded.");
            return;
        }

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
            gameManager.standardQuestions = wrapper.questions;
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
        wrapper.questions = ParsePlayerQuestionArray(jsonFile.text);

        if (wrapper.questions != null && wrapper.questions.Count > 0)
        {
            gameManager.playerQuestions = wrapper.questions;
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
            gameManager.pictureQuestions = wrapper.questions;
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

        // Wrap the array in an object: {"bonusQuestions": [...]}
        string wrappedJson = "{\"bonusQuestions\":" + jsonFile.text + "}";

        RawBonusQuestionArrayWrapper wrapper = JsonUtility.FromJson<RawBonusQuestionArrayWrapper>(wrappedJson);

        if (wrapper != null && wrapper.bonusQuestions != null)
        {
            // Convert to BonusQuestion format
            BonusQuestion bonusData = new BonusQuestion();
            bonusData.miniQuestions = new List<string>();

            foreach (RawBonusQuestionData raw in wrapper.bonusQuestions)
            {
                bonusData.miniQuestions.Add(raw.question);
            }

            gameManager.bonusQuestions = bonusData;
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
        // Wrap it in an object so Unity's JsonUtility can parse it

        List<Question> questions = new List<Question>();

        try
        {
            // Wrap the array in a JSON object: {"questions": [...]}
            string wrappedJson = "{\"questions\":" + json + "}";

            // Parse using wrapper class
            RawQuestionArrayWrapper wrapper = JsonUtility.FromJson<RawQuestionArrayWrapper>(wrappedJson);

            if (wrapper != null && wrapper.questions != null)
            {
                foreach (RawQuestionData raw in wrapper.questions)
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

    List<Question> ParsePlayerQuestionArray(string json)
    {
        // Player questions use different field names: "Question", "Right Answer", "Robot Answer"
        // Unity's JsonUtility doesn't handle spaces in field names, so we'll replace them
        List<Question> questions = new List<Question>();

        try
        {
            // Replace field names with spaces to use underscores
            string fixedJson = json.Replace("\"Right Answer\"", "\"Right_Answer\"");
            fixedJson = fixedJson.Replace("\"Robot Answer\"", "\"Robot_Answer\"");

            // Wrap the array in a JSON object: {"questions": [...]}
            string wrappedJson = "{\"questions\":" + fixedJson + "}";

            // Parse using player question wrapper class
            RawPlayerQuestionArrayWrapper wrapper = JsonUtility.FromJson<RawPlayerQuestionArrayWrapper>(wrappedJson);

            if (wrapper != null && wrapper.questions != null)
            {
                foreach (RawPlayerQuestionData raw in wrapper.questions)
                {
                    Question q = new Question();
                    q.questionText = raw.Question;
                    q.correctAnswer = raw.Right_Answer;
                    q.robotAnswer = raw.Robot_Answer;
                    q.robotAnecdote = ""; // Not in current data structure
                    q.questionType = "player";
                    q.imageURL = "";

                    questions.Add(q);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing player question array: " + e.Message);
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
public class RawQuestionArrayWrapper
{
    public List<RawQuestionData> questions;
}

[System.Serializable]
public class RawPlayerQuestionData
{
    public string Question;
    public string Right_Answer;
    public string Robot_Answer;
}

[System.Serializable]
public class RawPlayerQuestionArrayWrapper
{
    public List<RawPlayerQuestionData> questions;
}

[System.Serializable]
public class RawBonusQuestionData
{
    public int id;
    public string question;
}

[System.Serializable]
public class RawBonusQuestionArrayWrapper
{
    public List<RawBonusQuestionData> bonusQuestions;
}

[System.Serializable]
public class QuestionArrayWrapper
{
    public List<Question> questions;
}
