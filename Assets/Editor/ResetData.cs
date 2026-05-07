using UnityEngine;
using UnityEditor; //에디터 기능

public class ResetData
{
    [MenuItem("Tools/데이터 전체 초기화")]
    public static void ResetAllData()
    {
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();
    }
}