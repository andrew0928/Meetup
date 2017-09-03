# 容器驅動開發 ~ Container Driven Development




# commands & demo notes

取得 windows version:

```
ver.exe
```

從 registry 取得詳細的 windows build / revision number:

```
reg query "HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion" /v BuildLabEx
```

用 powershell 查看 .dll 的版本資訊

```
powershell (Get-ItemProperty -Path c:\windows\system32\kernel32.dll).VersionInfo.FileVersion
```


從 regedit 看 .net framework 的版本資訊

```
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Version
```
