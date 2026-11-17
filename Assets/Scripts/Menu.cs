using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class Menu : MonoBehaviour {

	#region Variables
	#endregion

	#region Unity Methods
	private void LoadLevel(int index)
	{
#if UNITY_EDITOR
		Selection.activeObject = null;
#endif
		SceneManager.LoadScene(index);
	}

	public void PlayGame()
	{
		LoadLevel(1);
	}
	public void PlayGame2()
	{
		LoadLevel(2);
	}
	public void PlayGame3()
	{
		LoadLevel(3);
	}
	public void PlayGame4()
	{
		LoadLevel(4);
	}
	public void PlayGame5()
	{
		LoadLevel(5);
	}
	public void PlayGame6()
	{
		LoadLevel(6);
	}
	public void PlayGame7()
	{
		LoadLevel(7);
	}
	public void PlayGame8()
	{
		LoadLevel(8);
	}
	#endregion
}
