#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Networking;

namespace GDFU
{
	public class GoogleDriveConnector
	{
		public static readonly GoogleDriveConfig Config =
			AssetDatabase.LoadAssetAtPath<GoogleDriveConfig>("Assets/Settings/GoogleDriveConfig.asset");

		public static string FilePath => Environment.CurrentDirectory + Config.FilePath;
		private static UnityWebRequest www;
		private static double elapsedTime = 0.0f;
		private static double startTime = 0.0f;
		
		/// <summary>
		/// 向服务器发送请求
		/// </summary>
		/// <param name="form">向服务器发送的数据</param>
		public static void CreateRequest(Dictionary<string, string> form)
		{
			form.Add("pass", Config.servicePassword);

			EditorApplication.update += EditorUpdate;

			www = UnityWebRequest.Post(Config.isDevMode ? Config.devWebServiceUrl : Config.webServiceUrl, form);
			startTime = EditorApplication.timeSinceStartup;
			www.Send();
		}

		static void EditorUpdate()
		{
			while (!www.isDone)
			{
				elapsedTime = EditorApplication.timeSinceStartup - startTime;
				if (elapsedTime >= Config.timeOutLimit)
				{
					GoogleDriveConnectorCore.ProcessResponse("TIME_OUT", (float) elapsedTime);
					EditorApplication.update -= EditorUpdate;
				}

				return;
			}

			EditorApplication.update -= EditorUpdate;

			if (www.isNetworkError)
			{
				GoogleDriveConnectorCore.ProcessResponse(
					GoogleDriveConnectorCore.MSG_CONN_ERR + "Connection error after " + elapsedTime.ToString() +
					" seconds: " + www.error, (float) elapsedTime);
				return;
			}

			if (www.isHttpError)
			{
				Debug.LogError("Web url error,please check");
				return;
			}

			GoogleDriveConnectorCore.ProcessResponse(www.downloadHandler.text, (float) elapsedTime);
		}
	}
}
#endif