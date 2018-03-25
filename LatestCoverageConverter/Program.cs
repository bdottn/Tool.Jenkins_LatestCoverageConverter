using Microsoft.VisualStudio.Coverage.Analysis;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace LatestCoverageConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null && args.Length == 0)
            {
                throw new ArgumentNullException("參數錯誤：未傳入參數！");
            }
            else
            {
                var argument = GenerateCoverageConverterArgument(args);

                ConvertCoverage(argument);
            }
        }

        private static CoverageConverterArgument GenerateCoverageConverterArgument(string[] args)
        {
            var argument = new CoverageConverterArgument();

            foreach (var arg in args.Select(a => a.Split('=')))
            {
                switch (arg[0].ToLower())
                {
                    // Workspace Path
                    case "-workspace":

                        argument.Workspace = arg[1];

                        break;

                    // TestResultsFolder
                    case "-testresultsfolder":

                        argument.TestResultsFolder = arg[1];

                        break;

                    // Output FileName
                    case "-outputfilename":

                        argument.OutputFileName = arg[1];

                        break;
                }
            }

            return argument;
        }

        private static void ConvertCoverage(CoverageConverterArgument argument)
        {
            var testResultsDirectory = Path.Combine(argument.Workspace, argument.TestResultsFolder);

            var targetFilePath = Path.Combine(testResultsDirectory, argument.OutputFileName);

            if (Directory.Exists(testResultsDirectory) == false)
            {
                throw new DirectoryNotFoundException(string.Format("資料夾錯誤：資料夾【{0}】不存在！", testResultsDirectory));
            }
            else
            {
                var sourceCoverageFile = Directory.GetFiles(testResultsDirectory, "*.coverage", SearchOption.AllDirectories).OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault();

                if (File.Exists(sourceCoverageFile))
                {
                    CoverageInfo coverageInfo = CoverageInfo.CreateFromFile(sourceCoverageFile);

                    CoverageDS coverageDataSet = coverageInfo.BuildDataSet();

                    using (XmlReader reader = new XmlTextReader(new StringReader(coverageDataSet.GetXml())))
                    {
                        using (XmlWriter writer = new XmlTextWriter(targetFilePath, Encoding.UTF8))
                        {
                            var transform = new XslCompiledTransform();

                            transform.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MSTestCoverageToEmma.xsl"));

                            transform.Transform(reader, writer);
                        }
                    }

                    Console.WriteLine(string.Format("轉換檔案：【{0}】→【{1}】。", sourceCoverageFile, targetFilePath));
                }
                else
                {
                    throw new FileNotFoundException(string.Format("檔案錯誤：檔案【{0}】不存在！", sourceCoverageFile));
                }
            }
        }
    }
}