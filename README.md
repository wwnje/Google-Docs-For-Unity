# Google-Docs-For-Unity

使用GoogleDrive和Google Script实现Unity和Google Sheets的云同步

## Setup

### Unity

path:Assets\Settings\GoogleDriveConfig

![](http://img.orvnge.com/uploads/2001/030936021381.png)

Web Service Url:Google Script Url

![](http://img.orvnge.com/uploads/2001/030939197904.png)

Spreadsheet Id:Google Drive Folder Id

![](http://img.orvnge.com/uploads/2001/030938304771.png)

Service Password: Google Script Password

File Path:本地配置表储存位置，相对路径

### Google Drive

在谷歌网盘上找一个文件夹，找到上述Spreadsheet Id即可，里面存储自己需要的excel文件，共享给项目中每个成员即可，这样就可以云同步文档。

> 注意事项

1. 所有需要读取的文档都应存储在该文件下，不能存储在其子文件夹下
2. 文档的大小不得超过50m
3. 如果不想转为Google Sheet格式，可以关闭设置

![](http://img.orvnge.com/uploads/2001/030943259705.png)

### Google Script部署

1. 在script.google.com中新建自己的项目，并添加 一个脚本
2. 脚本源码见Github中Code.gs，其中```var PASSWORD = "password";```密码就是Unity中password需要配置的。

#### 部署

![](http://img.orvnge.com/uploads/2001/030953502561.png)

![](http://img.orvnge.com/uploads/2001/030954432772.png)

最后更新获得Unity中的Web Service Url

## Use

在Unity中Tools/Google Drive Connector中有对应选项，目前有同步所有文档和同步单个文档的功能

1. Loading all Sheets：同步所有文档
2. Loading single Sheet id：同步单个文档

## 源码

[github](https://github.com/wwnje/Google-Docs-For-Unity)

