@echo off
REM 设置工作目录为当前脚本所在目录
setlocal
cd /d %~dp0

REM 定义输入目录和输出目录
set INPUT_DIR=%~dp0
set OUTPUT_DIR=%~dp0output

REM 确保输出目录存在
if not exist "%OUTPUT_DIR%" (
    mkdir "%OUTPUT_DIR%"
)

REM 定义proto文件列表
set PROTO_FILES=account.proto app.proto quote.proto strategy.proto trading.proto

REM 遍历每个proto文件并编译为C++和C#代码
for %%F in (%PROTO_FILES%) do (
    echo 编译 %%F 为 C++ 和 C# 文件...

    REM 编译为C++文件
    bin\protoc.exe --cpp_out="%OUTPUT_DIR%" --experimental_allow_proto3_optional --proto_path="%INPUT_DIR%" %%F

    REM 编译为C#文件，使用驼峰命名法
    bin\protoc.exe --csharp_out="%OUTPUT_DIR%" --experimental_allow_proto3_optional --csharp_opt=property_name=camel_case --proto_path="%INPUT_DIR%" %%F
)

echo 编译完成，所有文件已输出到 "%OUTPUT_DIR%" 文件夹。
pause