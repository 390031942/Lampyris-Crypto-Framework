namespace ProtocolCommonFileCodeGen
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;

    class ProtoGenerator
    {
        static void Main(string[] args)
        {
            // 检查是否提供了文件夹路径
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ProtoGenerator <proto_folder>");
                return;
            }

            // 获取文件夹路径
            string protoFolder = args[0];

            // 检查文件夹是否存在
            if (!Directory.Exists(protoFolder))
            {
                Console.WriteLine($"Error: Directory '{protoFolder}' does not exist.");
                return;
            }

            // 检查common.proto是否存在
            string commonProtoPath = Path.Combine(protoFolder, "common.proto");
            if (File.Exists(commonProtoPath))
            {
                File.Delete(commonProtoPath);
            }

            Console.WriteLine($"Scanning folder: {protoFolder}");

            // 扫描文件夹中的所有 .proto 文件
            List<string> protoFiles = new List<string>(Directory.GetFiles(protoFolder, "*.proto"));

            if (protoFiles.Count == 0)
            {
                Console.WriteLine("No .proto files found in the specified folder.");
                return;
            }

            Console.WriteLine($"Found {protoFiles.Count} .proto files.");

            // 提取所有消息类型
            Dictionary<string, string> requestMessages = new Dictionary<string, string>(); // 消息名称 -> package
            Dictionary<string, string> responseMessages = new Dictionary<string, string>(); // 消息名称 -> package

            foreach (string protoFile in protoFiles)
            {
                Console.WriteLine($"Processing file: {Path.GetFileName(protoFile)}");
                ExtractMessages(protoFile, requestMessages, responseMessages);
            }

            // 生成 common.proto 文件
            string commonProtoContent = GenerateCommonProto(protoFiles, requestMessages, responseMessages);

            // 保存 common.proto 文件
            File.WriteAllText(commonProtoPath, commonProtoContent);

            Console.WriteLine($"common.proto generated at: {commonProtoPath}");
        }

        /// <summary>
        /// 提取 .proto 文件中的消息类型
        /// </summary>
        /// <param name="protoFile">.proto 文件路径</param>
        /// <param name="requestMessages">请求消息列表</param>
        /// <param name="responseMessages">响应消息列表</param>
        static void ExtractMessages(string protoFile, Dictionary<string, string> requestMessages, Dictionary<string, string> responseMessages)
        {
            string[] lines = File.ReadAllLines(protoFile);
            string currentPackage = null;

            foreach (string line in lines)
            {
                // 匹配 package 定义
                Match packageMatch = Regex.Match(line.Trim(), @"^package\s+([\w\.]+);");
                if (packageMatch.Success)
                {
                    currentPackage = packageMatch.Groups[1].Value;
                }

                // 使用正则表达式匹配请求协议（以 Req 开头）
                Match requestMatch = Regex.Match(line.Trim(), @"^message\s+(Req\w+)\s*{");
                if (requestMatch.Success && currentPackage != null)
                {
                    requestMessages[requestMatch.Groups[1].Value] = currentPackage;
                }

                // 使用正则表达式匹配响应协议（以 Res 开头或以 Bean 结尾）
                Match responseMatch = Regex.Match(line.Trim(), @"^message\s+(Res\w+|.*Bean)\s*{");
                if (responseMatch.Success && currentPackage != null)
                {
                    responseMessages[responseMatch.Groups[1].Value] = currentPackage;
                }
            }
        }

        /// <summary>
        /// 生成 common.proto 文件内容
        /// </summary>
        /// <param name="protoFiles">扫描到的 .proto 文件列表</param>
        /// <param name="requestMessages">请求消息列表</param>
        /// <param name="responseMessages">响应消息列表</param>
        /// <returns>common.proto 文件内容</returns>
        static string GenerateCommonProto(List<string> protoFiles, Dictionary<string, string> requestMessages, Dictionary<string, string> responseMessages)
        {
            // 开始构造 common.proto 文件内容
            List<string> lines = new List<string>
            {
                "syntax = \"proto3\";",
                "",
                "// 自动生成的 common.proto 文件",
                "",
                "package lampyris.crypto.protocol.common;",
                ""
            };

            // 导入所有扫描到的 .proto 文件
            foreach (string protoFile in protoFiles)
            {
                string fileName = Path.GetFileName(protoFile);
                lines.Add($"import \"{fileName}\";");
            }

            lines.Add("");

            // 构造顶层请求消息
            lines.Add("message Request {");
            lines.Add("    oneof request_type {");
            int index = 1;
            foreach (var kvp in requestMessages)
            {
                string messageName = kvp.Key;
                string packageName = kvp.Value;
                lines.Add($"        {packageName}.{messageName} {ToCamelCase(messageName)} = {index++};");
            }
            lines.Add("    }");
            lines.Add("}");
            lines.Add("");

            // 构造顶层响应消息
            lines.Add("message Response {");
            lines.Add("    oneof response_type {");
            index = 1;
            foreach (var kvp in responseMessages)
            {
                string messageName = kvp.Key;
                string packageName = kvp.Value;
                lines.Add($"        {packageName}.{messageName} {ToCamelCase(messageName)} = {index++};");
            }
            lines.Add("    }");
            lines.Add("}");
            lines.Add("");

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// 将字符串的首字母变成小写
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>首字母小写的字符串</returns>
        static string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToLower(input[0]) + input.Substring(1);
        }
    }
}
