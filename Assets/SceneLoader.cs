using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환을 위한 간단한 헬퍼 클래스.
/// UI 버튼 등에서 호출될 수 있도록 public 함수를 제공합니다.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// 지정된 이름의 씬을 로드합니다. 로드 전에 데이터를 저장합니다.
    /// </summary>
    /// <param name="sceneName">로드할 씬의 이름</param>
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("로드할 씬 이름이 지정되지 않았습니다!");
            return;
        }

        Debug.Log($"씬 로드 요청: {sceneName} (데이터 저장 중...)");
        // 데이터 저장 시도
        DataManager.Instance?.SaveGameData();

        // 씬 로드
        SceneManager.LoadScene(sceneName);
    }

    // 필요하다면 게임 종료 함수 등 다른 공용 함수 추가 가능
    // public void QuitGame() { Application.Quit(); }
}