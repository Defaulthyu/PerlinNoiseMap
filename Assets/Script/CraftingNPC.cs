using UnityEngine;

public class CraftingNPC : MonoBehaviour
{
    // 제작 UI 연결
    public CraftingPanel craftingPanel;

    // 플레이어가 클릭했을 때 호출할 함수
    public void OpenCrafting()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetOpen(true);
        }
    }

    public void CloseCrafting()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetOpen(false);
        }
    }
}