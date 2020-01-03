using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using FileMode = System.IO.FileMode;

namespace GDFU
{
    public static class GoogleDriveConnectorCore
    {
        static readonly bool DebugMode = true;

        // name can't change, sames to google script.
        public enum QueryType
        {
            Fail,
            Success,
            getAllSheets, // Returns all worksheets on the spreadsheet.
            getSheet,
            testxlsx,
            
            getSheetFromName,
        }

        public const string MSG_UNKNOWN_ACTION = "UNKNOWN_ACTION";
        public const string MSG_BAD_ID = "FOLDERID_ERROR";
        public const string MSG_EMPTY_FOLDER = "EMPTY_FOLDER";
        public const string MSG_MISS_PARAM = "MISSING_PARAM";
        public const string MSG_CONN_ERR = "CONN_ERROR";
        public const string MSG_TIME_OUT = "TIME_OUT";
        public const string MSG_BAD_PASS = "PASS_ERROR";
        public const string MSG_GET_ALL_SHEET_SUCCESS = "GET_ALL_SHEET_SUCCESS";
        public const string MSG_GET_SHEET_SUCCESS = "GET_SHEET_SUCCESS";

        public static QueryType CurrentQueryType { get; private set; }
        static string currentContent = "";

        public static List<string[]> AllSheetsInfo = new List<string[]>();
        private static int _currentSheetIndex = 0;

        /// <summary>
        /// 向服务器发送命令，请求对应数据
        /// </summary>
        /// <param name="type">命令类型</param>
        /// <param name="id">对象网盘ID</param>
        private static void Request(QueryType type, string id = null)
        {
            Debug.LogFormat($"QueryType:{type.ToString()}, param = {id}");
            switch (type)
            {
                case QueryType.getAllSheets:
                    CurrentQueryType = QueryType.getAllSheets;
                    GetAllSheetsInfo();
                    break;
                case QueryType.getSheet:
                    CurrentQueryType = QueryType.getSheet;
                    GetSheetById(id);
                    break;
                case QueryType.getSheetFromName:
                    CurrentQueryType = QueryType.getSheetFromName;
                    GetSheetByName(id);
                    break;
                case QueryType.testxlsx:
                    CurrentQueryType = QueryType.testxlsx;
                    GetTest(id);
                    break;
                case QueryType.Fail:
                    break;
                case QueryType.Success:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// 处理服务器回包
        /// </summary>
        /// <param name="response">服务器回包</param>
        /// <param name="time">回复延迟</param>
        public static void ProcessResponse(string response, float time)
        {
//            Debug.LogError(response);
            string timeApendix = " Time: " + time.ToString();
            string logOutput = "";
            currentContent = "";

            if (response.StartsWith(MSG_BAD_PASS))
            {
                response = MSG_BAD_PASS;
            }

            string errorMsg = "Undefined connection error.";
            if (response.StartsWith(MSG_CONN_ERR))
            {
                errorMsg = response.Substring(MSG_CONN_ERR.Length);
                response = MSG_CONN_ERR;
            }

            if (response.StartsWith(MSG_GET_ALL_SHEET_SUCCESS))
            {
                logOutput = ProcessAllSheet(response.Remove(0, MSG_GET_ALL_SHEET_SUCCESS.Length));
                response = MSG_GET_ALL_SHEET_SUCCESS;
            }

            if (response.StartsWith(MSG_GET_SHEET_SUCCESS))
            {
                currentContent = response.Remove(0, MSG_GET_SHEET_SUCCESS.Length);
                response = MSG_GET_SHEET_SUCCESS;
            }

            CurrentQueryType = QueryType.Fail;
            switch (response)
            {
                case MSG_MISS_PARAM:
                    logOutput = "Parsing Error: Missing parameters.";
                    break;
                case MSG_TIME_OUT:
                    logOutput =
                        "Operation timed out, connection aborted. Check your internet connection and try again.";
                    break;
                case MSG_CONN_ERR:
                    logOutput = errorMsg;
                    break;
                case MSG_BAD_PASS:
                    logOutput = "Error: password incorrect.";
                    break;
                case MSG_BAD_ID:
                    logOutput = "Error: can't find folder.";
                    break;
                case MSG_EMPTY_FOLDER:
                    logOutput = "Error: empty folder.";
                    break;
                case MSG_UNKNOWN_ACTION:
                    logOutput = "Error: unknown action.";
                    break;
                case MSG_GET_ALL_SHEET_SUCCESS:
                    CurrentQueryType = QueryType.Success;
                    logOutput = "Get files' info end";
                    break;
                case MSG_GET_SHEET_SUCCESS:
                    CurrentQueryType = QueryType.Success;
                    logOutput = "Download Success!";
                    break;
                default:
                    logOutput = "Undefined server response: \n" + response;
                    break;
            }

            UpdateStatus(logOutput + timeApendix);
        }

        /// <summary>
        /// 根据文件名获取文档ID，在获取完网盘所有文档信息后才可执行
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetFileID(string fileName)
        {
            if (AllSheetsInfo.Count < 0)
            {
                return "";
            }

            foreach (var sheet in AllSheetsInfo)
            {
                if (sheet[0] == fileName)
                {
                    return sheet[1];
                }
            }

            return "";
        }

        /// <summary>
        /// 是否在等待服务器回复
        /// </summary>
        /// <returns></returns>
        private static bool QueryEnd()
        {
            return CurrentQueryType == QueryType.Fail || CurrentQueryType == QueryType.Success;
        }

        /// <summary>
        /// 储存服务器返回的所有文档信息并生成对应的占位文件。
        /// </summary>
        /// <param name="content">所有文档信息</param>
        /// <returns></returns>
        private static string ProcessAllSheet(string content)
        {
            if (content.Length == 0)
            {
                return "Fail";
            }

            AllSheetsInfo.Clear();
            string[] files = content.Split(' ');
            if (!Directory.Exists(GoogleDriveConnector.FilePath))
            {
                Directory.CreateDirectory(GoogleDriveConnector.FilePath);
            }

            for (int j = 0; j < files.Length - 1; j += 2)
            {
                var fileName = files[j] + ".xlsx";
                AllSheetsInfo.Add(new[] {fileName, files[j + 1]});
                if (!File.Exists(GoogleDriveConnector.FilePath + fileName))
                {
                    FileStream file = new FileStream(GoogleDriveConnector.FilePath + fileName, FileMode.Create);
                    file.Dispose();
                }
            }

            return "Success";
        }

        /// <summary>
        /// 生成对应文档
        /// </summary>
        /// <param name="fileInfo">文档位置</param>
        /// <param name="content">文档内容</param>
        private static void ProcessSheet(string fileInfo, string content)
        {
            if (content.Length == 0)
            {
                Debug.LogError("Empty file :" + fileInfo);
                return;
            }

            FileStream fs = new FileStream(GoogleDriveConnector.FilePath + fileInfo, FileMode.Create);
            byte[] bs = Convert.FromBase64String(content);
            fs.Write(bs, 0, bs.Length);
            fs.Dispose();
            Debug.Log("Download " + fileInfo + " success");
        }

        private static void UpdateStatus(string status)
        {
            if (DebugMode)
                Debug.Log($"<color=green>{status}</color>");
        }

        /// <summary>
        /// 获取所有的文档
        /// </summary>
        /// <returns></returns>
        public static IEnumerator GetAllSheets()
        {
            Debug.Log($"<color=green>GetAllSheets Info Start</color>");

            Request(QueryType.getAllSheets);
            yield return new WaitUntil(QueryEnd);

            Debug.Log($"<color=green>GetAllSheets Info Success</color>");

            var index = 0;
            foreach (var sheetInfo in AllSheetsInfo)
            {
                Request(QueryType.getSheet, sheetInfo[1]);
                Debug.Log($"<color=green>{++index}/{AllSheetsInfo.Count} - Downloading: {sheetInfo[0]}</color>");
                yield return new WaitUntil(QueryEnd);
                ProcessSheet(sheetInfo[0], currentContent);
            }

            Debug.Log("<color=green>Download all sheets success</color>");
        }

        /// <summary>
        /// 获取单个文档
        /// </summary>
        /// <returns></returns>
        public static IEnumerator LoadingSingleSheet()
        {
            Request(QueryType.getAllSheets);
            yield return new WaitUntil(QueryEnd);

            if (CurrentQueryType == QueryType.Fail)
            {
                Debug.LogError("Get sheets info fail");
            }
            else
            {
                string rootPath = GoogleDriveConnector.FilePath;
                string filePath = EditorUtility.OpenFilePanel(string.Empty, rootPath, string.Empty);
                if (filePath == "")
                {
                    yield break;
                }

                rootPath = rootPath.Replace("\\", "/");
                string fileName = filePath.Replace(rootPath, "");
                string fileID = GetFileID(fileName);
                if (fileID == "")
                {
                    Debug.LogError("File: {" + fileName + "} info error!");
                }
                else
                {
                    Debug.Log("Downloading " + fileName);
                    Request(QueryType.getSheet, fileID);
                    yield return new WaitUntil(QueryEnd);
                    ProcessSheet(fileName, currentContent);

//                    ExcelParser.ExportSingleExcelData(filePath);
                    Debug.LogError($"TODO 配置文件导出成功:{filePath}");
                }
            }
        }

        private static void GetSheetById(string id)
        {
            var form = new Dictionary<string, string> {{"action", QueryType.getSheet.ToString()}, {"id", id}};
            GoogleDriveConnector.CreateRequest(form);
        }
        
        private static void GetSheetByName(string name)
        {
            name = name.Replace(".xlsx", "");
            var form = new Dictionary<string, string>
            {
                {"action", QueryType.getSheetFromName.ToString()}, {"name", name}
            };
            
            GoogleDriveConnector.CreateRequest(form);
        }

        private static void GetTest(string id)
        {
            var form = new Dictionary<string, string>
            {
                {"action", QueryType.testxlsx.ToString()}, {"id", id}
            };

            Debug.Log("GetTest :");
            GoogleDriveConnector.CreateRequest(form);
        }

        private static void GetAllSheetsInfo()
        {
            Dictionary<string, string> form = new Dictionary<string, string>
            {
                {"action", QueryType.getAllSheets.ToString()}, {"id", GoogleDriveConnector.Config.spreadsheetId}
            };
            GoogleDriveConnector.CreateRequest(form);
        }

        #region Test

        public static IEnumerator LoadingTest()
        {
            Debug.Log("Downloading LoadingTest ");
            Request(QueryType.testxlsx, "17hcUKX9FaHrgwRC9BdDGYTGEGeR_xYCAaRik-W3vEwM");
            yield return new WaitUntil(QueryEnd);
            Debug.Log("End :" + CurrentQueryType.ToString());
        }

        public static IEnumerator LoadingSingleSheetFromName()
        {
            var rootPath = GoogleDriveConnector.FilePath;
            var filePath = EditorUtility.OpenFilePanel(string.Empty, rootPath, string.Empty);
            if (filePath == "")
            {
                yield break;
            }

            rootPath = rootPath.Replace("\\", "/");
            var fileName = filePath.Replace(rootPath, "");
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("File: {" + fileName + "} info error!");
            }
            else
            {
                Debug.Log("Downloading " + fileName);
                Request(QueryType.getSheetFromName, fileName);
                yield return new WaitUntil(QueryEnd);
                ProcessSheet(fileName, currentContent);

//                ExcelParser.ExportSingleExcelData(filePath);
                Debug.LogError($"TODO 配置文件导出成功:{filePath}");
            }
        }

        #endregion
    }
}
#endif