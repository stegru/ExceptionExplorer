#ifndef _SETUP_ADMIN_BUTTON_PAS_
#define _SETUP_ADMIN_BUTTON_PAS_

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
external 'ShellExecuteA@shell32.dll stdcall';


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
begin
    // start this exe using 'runas' verb
    cmd := ExpandConstant('{srcexe}');
    wnd := StrToInt(ExpandConstant('{wizardhwnd}'));
    if ShellExecute(wnd, 'runas', cmd, '', '', SW_SHOW) > 32 then
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

#endif 
//_SETUP_ADMIN_BUTTON_PAS_
