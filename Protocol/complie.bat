@echo off
REM ���ù���Ŀ¼Ϊ��ǰ�ű�����Ŀ¼
setlocal
cd /d %~dp0

REM ��������Ŀ¼�����Ŀ¼
set INPUT_DIR=%~dp0
set OUTPUT_DIR=%~dp0output

REM ȷ�����Ŀ¼����
if not exist "%OUTPUT_DIR%" (
    mkdir "%OUTPUT_DIR%"
)

REM ����proto�ļ��б�
set PROTO_FILES=account.proto app.proto quote.proto strategy.proto trading.proto

REM ����ÿ��proto�ļ�������ΪC++��C#����
for %%F in (%PROTO_FILES%) do (
    echo ���� %%F Ϊ C++ �� C# �ļ�...

    REM ����ΪC++�ļ�
    bin\protoc.exe --cpp_out="%OUTPUT_DIR%" --experimental_allow_proto3_optional --proto_path="%INPUT_DIR%" %%F

    REM ����ΪC#�ļ���ʹ���շ�������
    bin\protoc.exe --csharp_out="%OUTPUT_DIR%" --experimental_allow_proto3_optional --csharp_opt=property_name=camel_case --proto_path="%INPUT_DIR%" %%F
)

echo ������ɣ������ļ�������� "%OUTPUT_DIR%" �ļ��С�
pause