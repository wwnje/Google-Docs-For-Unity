// ************** 全局变量 **************

var OPEN_SS_DATA;

var MSG_UNKNOWN_ACTION = "UNKNOWN_ACTION";
var MSG_MISS_PARAM = "MISSING_PARAM";
var MSG_BAD_PASS = "PASS_ERROR";
var MSG_BAD_ID = "FOLDERID_ERROR";
var MSG_EMPTY_FOLDER = "EMPTY_FOLDER";
var MSG_GET_ALL_SHEET_SUCCESS = "GET_ALL_SHEET_SUCCESS";
var MSG_GET_SHEET_SUCCESS = "GET_SHEET_SUCCESS";

var PASSWORD = "password";

// ************** Entry Point **************

function doPost(e) {
    return Entry(e);
}

// ************** Initialization functions **************
function Entry(e) {
    if (e.parameters.pass != PASSWORD)
        return ContentService.createTextOutput(MSG_BAD_PASS);

    var result = ParseFlow(e);

    return ContentService.createTextOutput(result);
}

function OpenSheetById(e) {
    if (e.parameters.id == null)
        return MSG_MISS_PARAM;
    OPEN_SS_DATA = [];
    var file = DriveApp.getFileById(e.parameters.id.toString());
    if (file == null) {
        return MSG_BAD_ID;
    }

    var url = "https://docs.google.com/feeds/download/spreadsheets/Export?key=" + e.parameters.id.toString() + "&exportFormat=xlsx";

    var params = {
        method: "get",
        headers: { "Authorization": "Bearer " + ScriptApp.getOAuthToken() },
        muteHttpExceptions: true
    };

    var blob = UrlFetchApp.fetch(url, params).getBlob();

    blob.setName(file.getName() + ".xlsx");
    OPEN_SS_DATA.push(Utilities.base64Encode(blob.getBytes()));
}

function OpenSheetByName(e) {
    if (e.parameters.name == null)
        return MSG_MISS_PARAM;
    OPEN_SS_DATA = [];

    var fileName = e.parameters.name.toString();
    var files = DriveApp.getFilesByName(fileName.toString());

    Logger.log(fileName);

    while (files.hasNext()) {
        var file = files.next();
        var url = "https://docs.google.com/feeds/download/spreadsheets/Export?key=" + file.getId().toString() + "&exportFormat=xlsx";
        var params = {
            method: "get",
            headers: { "Authorization": "Bearer " + ScriptApp.getOAuthToken() },
            muteHttpExceptions: true
        };

        var blob = UrlFetchApp.fetch(url, params).getBlob();

        blob.setName(file.getName() + ".xlsx");
        OPEN_SS_DATA.push(Utilities.base64Encode(blob.getBytes()));
        return;
    }

    return MSG_BAD_ID;
}

function getGoogleSpreadsheetAsExcel(e) {

    try {

        var file = DriveApp.getFileById(e.parameters.id.toString());
        if (file == null) {
            return MSG_BAD_ID;
        }

        OPEN_SS_DATA = [];

        var url = "https://docs.google.com/feeds/download/spreadsheets/Export?key=" + e.parameters.id.toString() + "&exportFormat=xlsx";

        var params = {
            method: "get",
            headers: { "Authorization": "Bearer " + ScriptApp.getOAuthToken() },
            muteHttpExceptions: true
        };

        var blob = UrlFetchApp.fetch(url, params).getBlob();

        blob.setName(file.getName() + ".xlsx");
        OPEN_SS_DATA.push(Utilities.base64Encode(blob.getBytes()));
    } catch (f) {
        Logger.log(f.toString());
    }
}

function GetAllSheet(e) {
    if (e.parameters.id == null)
        return MSG_MISS_PARAM;
    OPEN_SS_DATA = "";
    var targetFolder = DriveApp.getFolderById(e.parameters.id.toString());
    if (targetFolder == null) {
        return MSG_BAD_ID;
    }
    var files = targetFolder.getFiles();
    while (files.hasNext()) {
        var file = files.next();
        OPEN_SS_DATA += file.getName() + " " + file.getId() + " ";
    }
}

function ParseFlow(e) {
    var result = "";
    var action = "";

    if (e.parameters.action != null)
        action = e.parameters.action.toString();
    else
        return MSG_MISS_PARAM;

    switch (action) {
        case "getAllSheets":
            GetAllSheet(e);
            result += MSG_GET_ALL_SHEET_SUCCESS + OPEN_SS_DATA;
            break;
        case "getSheet":
            OpenSheetById(e);
            result += MSG_GET_SHEET_SUCCESS + OPEN_SS_DATA;
            break;
        case "getSheetFromName":
            OpenSheetByName(e);
            result += MSG_GET_SHEET_SUCCESS + OPEN_SS_DATA;
            break;
        case "testxlsx":
            getGoogleSpreadsheetAsExcel(e);
            result += MSG_GET_SHEET_SUCCESS + OPEN_SS_DATA;
            break;
            defualt:
                result = MSG_UNKNOWN_ACTION;
            break;
    }
    return result;
}