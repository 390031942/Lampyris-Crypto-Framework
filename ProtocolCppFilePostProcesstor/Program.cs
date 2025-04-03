namespace ProtocolCppFilePostProcesstor
{
    class FilePostProcessor
    {
        static void Main(string[] args)
        {
            // 检查是否提供了文件夹路径
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: FilePostProcessor <directory>");
                return;
            }

            // 获取文件夹路径
            string directoryPath = args[0];

            // 检查文件夹是否存在
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Error: Directory '{directoryPath}' does not exist.");
                return;
            }

            Console.WriteLine($"Processing files in directory: {directoryPath}");

            // 处理 .pb.h 文件
            ProcessFiles(directoryPath, "*.pb.h", ".h");

            // 处理 .pb.cc 文件
            ProcessFiles(directoryPath, "*.pb.cc", ".cpp");

            Console.WriteLine("File processing completed.");
        }

        /// <summary>
        /// 处理指定类型的文件：重命名并修改内容
        /// </summary>
        /// <param name="directoryPath">文件夹路径</param>
        /// <param name="searchPattern">文件搜索模式（如 *.pb.h）</param>
        /// <param name="newExtension">新的文件扩展名（如 .h 或 .cpp）</param>
        static void ProcessFiles(string directoryPath, string searchPattern, string newExtension)
        {
            // 获取所有匹配的文件
            string[] files = Directory.GetFiles(directoryPath, searchPattern);

            foreach (string filePath in files)
            {
                // 获取文件名（不包括路径）
                string fileName = Path.GetFileName(filePath);

                // 获取文件名（不包括扩展名）
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

                // 构造新的文件名
                string newFileName = fileNameWithoutExtension.Replace(".pb", "") + newExtension;
                string newFilePath = Path.Combine(directoryPath, newFileName);

                // 重命名文件
                File.Move(filePath, newFilePath, true);
                Console.WriteLine($"Renamed: {fileName} -> {newFileName}");

                // 如果是 .cpp 文件，修改内容
                if (newExtension == ".cpp")
                {
                    ModifyCppFile(newFilePath);
                }
            }
        }

        /// <summary>
        /// 修改 .cpp 文件中的 #include "xxxx.pb.h" 为 #include "xxxx.h"
        /// </summary>
        /// <param name="filePath">.cpp 文件路径</param>
        static void ModifyCppFile(string filePath)
        {
            // 读取文件内容
            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                // 修改 #include "xxxx.pb.h" 为 #include "xxxx.h"
                if (lines[i].Contains("#include") && lines[i].Contains(".pb.h"))
                {
                    lines[i] = lines[i].Replace(".pb.h", ".h");
                    Console.WriteLine($"Modified line in {Path.GetFileName(filePath)}: {lines[i]}");
                }
            }

            // 写回修改后的内容
            File.WriteAllLines(filePath, lines);
        }
    }

}
