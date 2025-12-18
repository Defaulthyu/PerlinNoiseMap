using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void Stage1()
    {
        SceneManager.LoadScene("Stage1");
    }
    public void Stage2()
    {
        SceneManager.LoadScene("Stage2");
    }
    public void Stage3()
    {
        SceneManager.LoadScene("Stage3");
    }
    public void TItle()
    {
        SceneManager.LoadScene("Title");
    }
    public void Ending()
    {
        SceneManager.LoadScene("Ending");
    }
    public void Quit_()
    {
        Application.Quit();
    }
}
