using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIScripting
{
    public class PythonExecuter
    {
        private string _folder;

        /// <summary>
        /// 构造方法
        /// </summary>
        public PythonExecuter(string folder)
        {
            _folder = System.IO.Path.GetFullPath(folder);
        }

        /// <summary>
        /// 异步运行Python脚本
        /// </summary>
        /// <param name="script"></param>
        /// <param name="arguments"></param>
        /// <param name="workingDirectory"></param>
        /// <returns></returns>
        public async Task<int> RunPythonAsync(string script, string arguments, string workingDirectory = null)
        {
            // 使用Task.Run来包装进程的启动和等待，使其成为异步操作
            return await Task.Run(() =>
            {
                using (var process = new Process())
                {
                    // 配置进程
                    process.StartInfo.FileName = "python";
                    process.StartInfo.Arguments = $"{_folder}/{script}.py {arguments}";

                    // 如果提供了工作目录，则设置之
                    if (workingDirectory != null)
                    {
                        process.StartInfo.WorkingDirectory = workingDirectory;
                    }

                    // 指定输出和错误流向控制台（如果需要收集输出，可以重定向）
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false; // 需要设置为false以便重定向输出

                    // 启动进程
                    process.Start();

                    // 等待进程完成并获取退出代码
                    process.WaitForExit();
                    return process.ExitCode;
                }
            });
        }
    }
}
