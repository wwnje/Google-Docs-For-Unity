using UnityEngine;

namespace GDFU
{
    [CreateAssetMenu(menuName = "GoogleDriveConfig")]
    public class GoogleDriveConfig : ScriptableObject
    {
        public bool isDevMode;
        public string webServiceUrl;
        public string devWebServiceUrl;
        public string spreadsheetId;
        public string servicePassword;
        public float timeOutLimit;
        public string FilePath;
    }
}