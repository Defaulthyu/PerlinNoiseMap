// PlayerRecipe.cs (플레이어에게 붙임)
using System.Collections.Generic;
using UnityEngine;

public class PlayerRecipe : MonoBehaviour
{
    // 내가 배운 레시피 이름 목록 (예: "IronPickaxe")
    public List<string> knownRecipes = new List<string>();

    public void LearnRecipe(string recipeName)
    {
        if (!knownRecipes.Contains(recipeName))
        {
            knownRecipes.Add(recipeName);
        }
    }

    public bool HasRecipe(string recipeName)
    {
        return knownRecipes.Contains(recipeName);
    }
}