
#define BuildPath AddBackslash(SourcePath) + "..\release\Build"
#define OutputDir AddBackslash(BuildPath) + ".."

#define AppExeName "ExceptionExplorer.exe"

#define ExeFile AddBackslash(BuildPath) + AppExeName

#define AppURL "http://exceptionexplorer.net"

#define AppName GetStringFileInfo(ExeFile, PRODUCT_NAME)
#define AppPublisher GetFileCompany(ExeFile)
#define AppVersion GetFileProductVersion(ExeFile)
#define AppCopyright GetFileCopyright(ExeFile)

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{BFE9ED5B-2769-4219-9865-B4F90F436425}
AppName={#AppName}
AppVersion={#AppVersion}
;AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={code:getDefDir}\{#AppName}
LicenseFile=licence.txt
OutputDir={#OutputDir}
OutputBaseFilename=ExceptionExplorer
SetupIconFile=..\Images\icon\ExceptionExplorer.ico
Compression=lzma
SolidCompression=yes
WizardImageFile=compiler:WizModernImage-IS.bmp
WizardSmallImageFile=compiler:WizModernSmallImage-IS.bmp
ShowLanguageDialog=no
DisableReadyPage=True
DisableReadyMemo=True
AppCopyright={#AppCopyright}
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\ExceptionExplorer.exe
VersionInfoVersion={#AppVersion}
VersionInfoCompany={#AppPublisher}
VersionInfoCopyright={#AppCopyright}
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}
MinVersion=0,6.0
;SignTool=sign 

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#BuildPath}\ExceptionExplorer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildPath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{commondesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent



[Code]

function isAdmin(): Boolean;
begin
    Result := (IsAdminLoggedOn or IsPowerUserLoggedOn);
end;

// handle non-admin users
function getDefDir(Param: String): String;
begin
	if isAdmin then
		Result := ExpandConstant('{pf}')
	else
		Result := ExpandConstant('{localappdata}')
end;


function ShellExecute(hwnd: Integer; lpOperation, lpFile, lpParameters, lpDirectory: String; nShowCmd: Integer): Integer;
external 'ShellExecuteW@shell32.dll stdcall';


var
    close: Boolean;

procedure CancelButtonClick(CurPageID: Integer; var Cancel, Confirm: Boolean);
begin
    Cancel := True;
    // don't show the "are you sure?" box (if quitting due to elevation)
    Confirm := not close;
end;

procedure elevButtonOnClick(Sender: TObject);
var
    cmd: String;
    wnd: Integer;
    r: Integer;
begin
    // start this exe using 'runas' verb
    cmd := ExpandConstant('{srcexe}');
    wnd := StrToInt(ExpandConstant('{wizardhwnd}'));
    r := ShellExecute(wnd, 'runas', cmd, '', '', SW_SHOW);
    if r > 32 then
    begin
        close := True;
        WizardForm.Close;
    end;

end;

var
    elevButton: TButton;

procedure AdminButton_InitializeWizard;
begin

    // Create the button
    elevButton := TButton.Create(WizardForm);
    elevButton.Parent := WizardForm;
    elevButton.Left := WizardForm.ClientWidth - WizardForm.CancelButton.Left - WizardForm.CancelButton.Width;
    elevButton.Top := WizardForm.CancelButton.Top;
    elevButton.Width := 140;
    elevButton.Height := WizardForm.CancelButton.Height;
    elevButton.Caption := 'Run as &Administrator';
    elevButton.OnClick := @elevButtonOnClick;
    // send BCM_SETSHIELD to give it a shield
    SendMessage(elevButton.Handle, 5644, 0, 1);
end;

procedure CurPageChanged(CurPageID: Integer);
begin
    // don't show the button after the directory has been chosen
    elevButton.Visible := (CurPageID <= wpSelectDir) and not isAdmin;
end;

procedure InitializeWizard;
begin
AdminButton_InitializeWizard;

end;


