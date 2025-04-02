@echo on
REM Set the working directory to the current script's location
setlocal
cd /d %~dp0

REM Define input directory and output directories for C++ and C#
set INPUT_DIR=%~dp0
set CPP_OUTPUT_DIR=%~dp0output/cpp
set CSHARP_OUTPUT_DIR=%~dp0output/csharp

REM Define the parent directory for copying files
set CPP_DST_DIR=%~dp0../Lampyris.Client.Crypto.Core/sources/Protocol
set CSHARP_DST_DIR=%~dp0../Lampyris.Server.Crypto.Common/sources/Protocol

REM Ensure the output directories exist
if not exist "%CPP_OUTPUT_DIR%" (
    mkdir "%CPP_OUTPUT_DIR%"
)
if not exist "%CSHARP_OUTPUT_DIR%" (
    mkdir "%CSHARP_OUTPUT_DIR%"
)

REM Define the list of proto files to compile
set PROTO_FILES=account.proto app.proto quote.proto strategy.proto trading.proto

REM Loop through each proto file and compile to C++ and C# code
for %%F in (%PROTO_FILES%) do (
    echo Compiling %%F to C++ and C# files...

    REM Compile to C++ files
    bin\protoc.exe --cpp_out="%CPP_OUTPUT_DIR%" --experimental_allow_proto3_optional %%F
    if %errorlevel% neq 0 (
        echo [ERROR] Failed to compile %%F to C++ files.
        pause
        exit /b %errorlevel%
    )

    REM Compile to C# files
    bin\protoc.exe --csharp_out="%CSHARP_OUTPUT_DIR%" --experimental_allow_proto3_optional %%F
    if %errorlevel% neq 0 (
        echo [ERROR] Failed to compile %%F to C# files.
        pause
        exit /b %errorlevel%
    )

    echo [INFO] Successfully compiled %%F.

    REM Rename .pb.cc and .pb.h files to .cc and .h
    echo Renaming generated C++ files for %%F...

    for %%G in ("%CPP_OUTPUT_DIR%\*.pb.h") do (
        ren "%%G" "%%~nG"
    )
    for %%G in ("%CPP_OUTPUT_DIR%\*.pb") do (
        ren "%%G" "%%~nG.h"
    )
    for %%G in ("%CPP_OUTPUT_DIR%\*.pb.cc") do (
        ren "%%G" "%%~nG"
    )
    for %%G in ("%CPP_OUTPUT_DIR%\*.pb") do (
        ren "%%G" "%%~nG.cpp"
    )
    echo [INFO] Renaming completed for %%F.

    REM Copy renamed files to the parent directory
    echo Copying renamed files for %%F to the parent directory...
    xcopy "%CPP_OUTPUT_DIR%" "%CPP_DST_DIR%" /E /Y >nul
    xcopy "%CSHARP_OUTPUT_DIR%" "%CSHARP_DST_DIR%" /E /Y >nul
    echo [INFO] Files for %%F copied successfully.
)

echo Compilation completed. All files have been exported to the "output" folder and copied to the parent directory.
pause
