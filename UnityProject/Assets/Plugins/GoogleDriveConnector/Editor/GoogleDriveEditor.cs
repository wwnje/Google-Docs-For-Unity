#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace GDFU
{
	public class GoogleDriveEditor : Editor
	{
		private static readonly GoogleDriveConfig Config =
			AssetDatabase.LoadAssetAtPath<GoogleDriveConfig>("Assets/Settings/GoogleDriveConfig.asset");

		private const string LoadingSingleTableById = "Tools/Google Drive Connector/Loading single Sheet id";
		private const string LoadingSingleTableByName = "Tools/Google Drive Connector/Loading single Sheet";
		
		[MenuItem("Tools/Google Drive Connector/Loading all Sheets")]
		private static void LoadingAllTables()
		{
			if (!EditorUtility.DisplayDialog("Loading all Sheets?", "Loading all Sheets", "ok", "cancel")) return;
			EditorCoroutineRunner.StopAllCoroutine();
			EditorCoroutineRunner.StartEditorCoroutine(LoadingAllSheets());
		}
		
		[MenuItem(LoadingSingleTableById)]
		public static void LoadingSingleTable()
		{
			EditorCoroutineRunner.StopAllCoroutine();
			EditorCoroutineRunner.StartEditorCoroutine(GoogleDriveConnectorCore.LoadingSingleSheet());
		}
		
		[MenuItem(LoadingSingleTableByName)]
		static void LoadingSingleTableFromName()
		{
			Debug.Log(LoadingSingleTableByName);
			EditorCoroutineRunner.StopAllCoroutine();
			EditorCoroutineRunner.StartEditorCoroutine(GoogleDriveConnectorCore.LoadingSingleSheetFromName());
		}

		[MenuItem("Tools/Google Drive Connector/Stop all operating")]
		public static void StopAllOperating()
		{
			EditorCoroutineRunner.StopAllCoroutine();
			Debug.Log("Stop all");
		}

		static IEnumerator LoadingAllSheets()
		{
			yield return EditorCoroutineRunner.StartEditorCoroutine(GoogleDriveConnectorCore.GetAllSheets());
			Debug.LogError("TODO");
//			ExcelParser.ExportExcels();
		}

		
		[MenuItem(LoadingSingleTableByName, true)]
		static bool ValidateTest() => Config.isDevMode;


//		[MenuItem("Tools/Google Drive Connector/Test")]
//		static void Test()
//		{
//			EditorCoroutineRunner.StopAllCoroutine();
//			EditorCoroutineRunner.StartEditorCoroutine(GoogleDriveConnectorCore.LoadingTest());		
//		}

	}
}
#endif
