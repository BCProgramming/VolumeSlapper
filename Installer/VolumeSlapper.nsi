
!define PROGPATH "..\bin\Debug"
!define PROGFILE "VolumeSlapper"

!tempfile INC_FILE
!system '"!NSIS Version Getter.exe" "${PROGPATH}\${PROGFILE}".exe "${INC_FILE}"'
!include ${INC_FILE}

!undef INC_FILE



!define FULLNAME "${COMPANY}\${PACKAGE}\${PROGFILE}"

!define INSTNAME "Volume Slapper (${PROGDATE}).exe"

; The file to write
OutFile "${INSTNAME}"

; The name of the installer
Name "${PROGFILE}"
Icon "VolumeKnob.ico"

; Version and copyright information.
!searchparse /noerrors "${PROGVER}" '' V1 '.' V2 '.' V3 '.' V4
!if "${V4}" == ""
	VIProductVersion "${PROGVER}.0"
!else
	VIProductVersion "${PROGVER}"
!endif
VIAddVersionKey "ProductName" "${PROGFILE}"
VIAddVersionKey "ProductVersion" "${PROGVER}"
VIAddVersionKey "ProductDate" "${PROGDATE}"
VIAddVersionKey "CompanyName" "${COMPANY}"
VIAddVersionKey "LegalCopyright" "${COPYRIGHT}"
VIAddVersionKey "FileVersion" "${PROGVER}"
VIAddVersionKey "FileDescription" "${PROGNAME} Installer"
VIAddVersionKey "OriginalFilename" "${INSTNAME}"

; The default installation directory
InstallDir "$PROGRAMFILES\${FULLNAME}"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\${FULLNAME}" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

AutoCloseWindow true

;--------------------------------

; Pages

Page components
Page directory
Page instfiles "" ""

UninstPage uninstConfirm
UninstPage instfiles

;--------------------------------

; The stuff to install
Section "${PROGFILE} (required)"

  SectionIn RO
  ReadRegStr $R0 HKLM \
   "Software\${FULLNAME}" \
   "Install_Dir"
  
   StrCmp $R0 "" done uninst

 ;Run the uninstaller
uninst:

  ExecWait '"$INSTDIR\uninstall.exe" /S _?=$INSTDIR'
Delete "$INSTDIR\uninstall.exe"
done:
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  ; Put file there
  File "/oname=${PROGFILE}.exe" "${PROGPATH}\${PROGFILE}.exe"
  File "${PROGPATH}\VolumeSlapper.exe"
  File "${PROGPATH}\VolumeSlapper.exe.config"
  File "${PROGPATH}\Vannatech.CoreAudio.dll"


  ; Lib files
  ; Set back to base directory.
  SetOutPath $INSTDIR
  
  ; Write the installation path into the registry
  WriteRegStr HKLM "Software\${FULLNAME}" "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${FULLNAME}" "DisplayName" "${PROGNAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${FULLNAME}" "DisplayIcon" "$INSTDIR\${PROGNAME}.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${FULLNAME}" "Publisher" "${COMPANY}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${FULLNAME}" "HelpTelephone" "1-800-845-5445"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${FULLNAME}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${FULLNAME}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${FULLNAME}" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
SectionEnd
; Optional section (can be disabled by the user)
SectionGroup /e "Shortcuts"
  Section "Start Menu"
    SectionIn RO
    SetShellVarContext all
    CreateDirectory "$SMPROGRAMS\${FULLNAME}"
    CreateShortCut "$SMPROGRAMS\${FULLNAME}\${PROGNAME}.lnk" "$INSTDIR\${PROGNAME}.exe" "" "$INSTDIR\${PROGNAME}.exe" 0
    CreateShortCut "$SMPROGRAMS\${FULLNAME}\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  SectionEnd
  Section "Desktop"
    SetShellVarContext all
    CreateShortCut "$DESKTOP\${PROGNAME}.lnk" "$INSTDIR\${PROGNAME}.exe" "" "$INSTDIR\${PROGNAME}.exe" 0
  SectionEnd
  Section "Quick Launch"
    SetShellVarContext all
    CreateShortCut "$QUICKLAUNCH\${PROGNAME}.lnk" "$INSTDIR\${PROGNAME}.exe" "" "$INSTDIR\${PROGNAME}.exe" 0
  SectionEnd
SectionGroupEnd
;--------------------------------

; Uninstaller

Section "Uninstall"
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${FULLNAME}"
  DeleteRegKey /ifempty HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANY}\${PACKAGE}"
  DeleteRegKey /ifempty HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANY}"
  DeleteRegKey HKLM "Software\${FULLNAME}"
  DeleteRegKey /ifempty HKLM "Software\${COMPANY}\${PACKAGE}"
  DeleteRegKey /ifempty HKLM "Software\${COMPANY}"

  ; Remove files and uninstaller

  Delete "$INSTDIR\VolumeSlapper.exe"
  Delete "$INSTDIR\VannaTech.CoreAudio.dll"
  Delete "$INSTDIR\Uninstall.exe"
  
  ; Remove directories.  Not doing this recursively though so we'll need to break out of the defined variables
  ; (which have multi-level paths).

  RMDir "$INSTDIR"
  RMDir "$PROGRAMFILES\${COMPANY}\${PACKAGE}"
  RMDir "$PROGRAMFILES\${COMPANY}"

  ; Remove shortcuts, if any
  Delete "$DESKTOP\${PROGNAME}.lnk"
  Delete "$SMPROGRAMS\${FULLNAME}\*.*"
  Delete "$QUICKLAUNCH\${PROGNAME}.lnk"
  ; Remove directories.  Again, with some hard-coding needed.
  RMDir "$SMPROGRAMS\${FULLNAME}"
  RMDir "$SMPROGRAMS\${COMPANY}\${PACKAGE}"
  RMDir "$SMPROGRAMS\${COMPANY}"
SectionEnd

