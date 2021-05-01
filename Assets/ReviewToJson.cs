using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

using PlayFab;
using PlayFab.ClientModels;


//A class to store the review, this will be used to parse reviews as json objects
public class Review
{
    public List<string> reviewText = new List<string>{};
    public List<float> starAmount = new List<float>{};
}

public class ReviewToJson : MonoBehaviour
{
    //This is where the review rating is inputted by the user
    [SerializeField]private InputField m_reviewRating;
    //This is where the review is inputted by the user
    [SerializeField]private InputField m_reviewText;

    //store the review written by the user 
    private Review m_reviews = new Review();
    
    void Awake() 
    {
        Login();
    }

    void Start() 
    {
        //Make sure we have the most recent version of all the reviews
        Invoke("GetReview", 0.2f);
    }


    //Backend with playfab
    #region backend
    void Login()
    {
        //Create a request to login with a new ID
        var request = new LoginWithCustomIDRequest{
            CustomId = "ReviewStorage",
            //CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError);

    }

    public void SaveReview()
    {
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string>{
                {"Reviews", getCurrentReviewData()}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);
    }

    public void GetReview()
    {
        //Retrieves all the user data stored in the currently logged in user
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataGet, OnError);
    }

    #region error and success messages
    void OnDataSend(UpdateUserDataResult result)
    {
        Debug.Log("Data was successfully updated");
    }

    void OnSuccess(LoginResult result)
    {
        Debug.Log("The login was successful");
    }

    void OnDataGet(GetUserDataResult result)
    {
        //If there is data stored
        if(result.Data!=null && result.Data.ContainsKey("Reviews"))
        { 
            //Updated the local json file with the values from the serverside player data
            Debug.Log("Data successfully retrieved");
            File.WriteAllText(Application.dataPath+"\\FileName.json", result.Data["Reviews"].Value);

        }
    }

    void OnError(PlayFabError error)
    {
        Debug.Log("There was an error: ");
        Debug.Log(error.GenerateErrorReport());
    }
    #endregion
    #endregion

    public void SaveCurrentReview()
    {
        //Make sure that the current reviews are up to date
        GetReview();
        m_reviews = JsonUtility.FromJson<Review>(File.ReadAllText(Application.dataPath+"\\Filename.json"));

        //Add new values to the review
        m_reviews.reviewText.Add(m_reviewText.text);
        m_reviews.starAmount.Add(float.Parse(m_reviewRating.text));

        //Write it to the json file
        File.WriteAllText(Application.dataPath+"\\FileName.json", JsonUtility.ToJson(m_reviews));

        //Update the server database with the new json file data
        SaveReview();
    }


    public void GetRandomReview()
    {
        //Make sure that the reviews are up to date
        GetReview();
        m_reviews = JsonUtility.FromJson<Review>(File.ReadAllText(Application.dataPath+"\\Filename.json"));


        //Then select a random review
        int randomPos = Random.Range(0, m_reviews.reviewText.Count);

        Debug.Log("Random review: "+m_reviews.reviewText[randomPos]);
        Debug.Log("Random Review rating: "+m_reviews.starAmount[randomPos]);
    }

    public string getCurrentReviewData()
    {
        return File.ReadAllText(Application.dataPath+"\\FileName.json");
    }
}
