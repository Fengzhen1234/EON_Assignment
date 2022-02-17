using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuizApp : MonoBehaviour
{
    [SerializeField]
    // Main
    public Transform quizContainer;
    public Transform quizEntry;

    // New Quiz
    public Transform quizNameInput;

    // Quiz Details
    public Transform qnContainer;
    public Transform qnEntry;

    // Question Details
    public Transform qnDescription;
    public Transform opContainer;
    public Transform opToggle;

    // Preview
    public Transform previewQn;
    public Transform previewLabel;
    public Transform previewOption;

    // Result
    public Transform result;
    public Button submitBtn;
    public Button nextBtn;
    public Transform previewScore;

    
    public bool correct = false;
    public int qnDone = 0; // count of questions done
    public int score;

    public Quiz quiz;
    public QuizSummary quizOverall;
    public List<Quiz> quizList;
    public List<Transform> quizEntryTransformList;
    public List<Transform> qnEntryTransformList;
    public List<Question> questionList;
    public List<Option> optionList;

    private void Awake()
    {
        quizOverall = new QuizSummary();
        quizContainer = transform.Find("MainPage").transform.Find("QuizList");
        quizEntry = quizContainer.transform.Find("Row");

        quizNameInput = transform.Find("New_Quiz").transform.Find("QnName").transform.Find("Text");

        quizEntry.gameObject.SetActive(false);
        
        quizEntryTransformList = new List<Transform>();
        foreach (Quiz quizEntry in quizList)
        {
            PopulateQuiz(quizEntry, quizContainer, quizEntryTransformList);
        }

    }

    private void PopulateQuiz(Quiz quiz, Transform container, List<Transform> transformList)
    {
        float entryHeight = 60f;
        Transform entryTransform = Instantiate(quizEntry, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -entryHeight * (transformList.Count-1));

        int quizCount = transformList.Count + 1;
        string quizTitle = quiz.QuizName;

        quizEntry.Find("Num").GetComponent<Text>().text = quizCount.ToString();
        quizEntry.Find("Quiz").GetComponent<Text>().text = quizTitle;

        quizEntry.gameObject.SetActive(true);
        transformList.Add(entryTransform);
    }

    private void PopulateQuestion(Question question, Transform container, List<Transform> transformList)
    {
        float rowHeight = 60f;
        Transform qnTransform = Instantiate(qnEntry, container);
        RectTransform qnRectTransform = qnTransform.GetComponent<RectTransform>();
        qnRectTransform.anchoredPosition = new Vector2(0, -rowHeight * (transformList.Count - 1));

        int qnCount = transformList.Count + 1;
        string qnTitle = question.QnName;

        qnEntry.Find("Num").GetComponent<Text>().text = qnCount.ToString();
        qnEntry.Find("Question").GetComponent<Text>().text = qnTitle;
        qnEntry.Find("Option").GetComponent<Text>().text = question.OptionList.Count.ToString();

        qnEntry.gameObject.SetActive(true);
        transformList.Add(qnTransform);
    }
   
    public void CreateQuiz()
    {
        quiz = new Quiz();
        string inputName = quizNameInput.GetComponent<Text>().text;
        
        quiz.QuizName = inputName;
        quizList.Add(quiz);   // add quiz to list

        // populate question list
        qnContainer = transform.Find("Quiz_Details").transform.Find("Scroll View");
        qnEntry = qnContainer.transform.Find("Row");

        qnEntry.gameObject.SetActive(false);

        qnEntryTransformList = new List<Transform>();
        foreach (Question qnEntry in questionList)
        {
            PopulateQuestion(qnEntry, qnContainer, qnEntryTransformList);
        }
    }

    public void AddQuestion()
    {
        qnDescription = transform.Find("Question_Details").transform.Find("Qn_Desc").transform.Find("Text");
        opContainer = transform.Find("Question_Details").transform.Find("Op_Desc");
        opToggle = transform.Find("Question_Details").transform.Find("Toggles");
    }
    public void CreateQuestion()
    {
        string inputQn = qnDescription.GetComponent<Text>().text;
        Question tempQuestion = new Question();
        List<Option> tempOptionList = new List<Option>();
        
        tempQuestion.QnName = inputQn;

        for (int i = 0; i < 3; i++)
        {
            Option tempOption = new Option();
            string opGameObject = "Op" + (i + 1).ToString();
            string toggleGameObject = "Toggle" + (i + 1).ToString();
            string inputOp = opContainer.Find(opGameObject).Find("Text").GetComponent<Text>().text;
            bool selectedOp = opToggle.Find(toggleGameObject).GetComponent<Toggle>().isOn;
            tempOption.OpDetail = inputOp;
            tempOption.isAnswer = selectedOp;
            tempOptionList.Add(tempOption);
        }
        tempQuestion.OptionList = tempOptionList;
        questionList.Add(tempQuestion); // append to questionList
        quiz.QuestionList = questionList; // update question list of the quiz

        // populate question list before returning to Quiz_Details
        qnContainer = transform.Find("Quiz_Details").transform.Find("Scroll View");
        qnEntry = qnContainer.transform.Find("Row");

        qnEntry.gameObject.SetActive(false);

        qnEntryTransformList = new List<Transform>();
        foreach (Question qnEntry in questionList)
        {
            PopulateQuestion(qnEntry, qnContainer, qnEntryTransformList);
        }
    }

    public void Next()
    {
        if (qnDone >= questionList.Count)
        {
            // change button UI to submit
            submitBtn = transform.Find("Preview").transform.Find("Submit").GetComponent<Button>();
            nextBtn = transform.Find("Preview").transform.Find("Next").GetComponent<Button>();
            nextBtn.gameObject.SetActive(false);
            submitBtn.gameObject.SetActive(true);
            // reset count of questions done
            qnDone = 0;
            print("run complete function");
            CompletePreview();
        }
        else
        {
            print("qn done: "+qnDone);
            PreviewQuiz();
        }
    }

    public void CheckAns()
    {
        // check current button on click
        bool clicked = true;
        // find name of this gameobject
        string nameBtn = EventSystem.current.currentSelectedGameObject.name; // button1 or button2 or button3
        string lastChar = nameBtn.Substring(nameBtn.Length-1);
        int qnNum = int.Parse(lastChar);
        if (clicked == questionList[qnDone-1].OptionList[qnNum-1].isAnswer)
        {
            correct = true;
            questionList[qnDone-1].isCorrect = true;
        }

    }

    public int PreviewQuiz()
    {
        previewLabel = transform.Find("Preview").transform.Find("Labels").transform.Find("Label");
        previewQn = transform.Find("Preview").transform.Find("Qn_Desc").transform.Find("Text");
        previewOption = transform.Find("Preview").transform.Find("OpList");

        string questionCount = $"Question {qnDone+1} / {questionList.Count}";
        previewLabel.GetComponent<Text>().text = questionCount;
        previewQn.GetComponent<Text>().text = questionList[qnDone].QnName;
        for (int i = 0; i < 3; i++)     // looping through the options
        {
            string opGameObject = "Button" + (i + 1).ToString();
            previewOption.Find(opGameObject).Find("Text").GetComponent<Text>().text = questionList[qnDone].OptionList[i].OpDetail;
        }
        qnDone +=1; // num of times PreviewQuiz() is called
        return qnDone;
    }

    public void CompletePreview()
    {
        // to get score, sum all correctly answered questions
        foreach (Quiz qz in quizList)
        {
            foreach (Question qn in qz.QuestionList)
            {
                if (qn.isCorrect)
                {
                    quiz.Score += 1;
                }
            }
        }
        previewScore = transform.Find("Quiz_Result").transform.Find("Score");
        previewScore.GetComponent<Text>().text = "Your Score: " + quiz.Score.ToString() + " / " + questionList.Count;

        quizOverall.QuizList = quizList;
        Save(quizOverall);
    }

    private void Save(QuizSummary quizSummary)
    {
        string json = JsonUtility.ToJson(quizSummary, true);
        File.WriteAllText(Application.dataPath  + "/save.txt", json);
        
    }
}



[Serializable]
public class Option
{
    [SerializeField] public string OpDetail;
    [SerializeField] public bool isAnswer;
}
[Serializable]
public class Question
{
    [SerializeField] public string QnName;
    [SerializeField] public List<Option> OptionList;
    [SerializeField] public bool isCorrect = false;
}
[Serializable]
public class Quiz
{
    [SerializeField] public string QuizName;
    [SerializeField] public int Score = 0;
    [SerializeField] public List<Question> QuestionList;
}
[Serializable]
public class QuizSummary
{
    [SerializeField] public List<Quiz> QuizList;
}