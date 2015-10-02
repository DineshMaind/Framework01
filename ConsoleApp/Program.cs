using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            using (StreamWriter errorLog = new StreamWriter(string.Format("ErrorLog{0:yyyyMMddHHmmss}.txt", DateTime.Now)))
            {
                try
                {
                    Console.WriteLine("Creating models...");

                    var assemblyPath = args[0];
                    var mainNamespace = args[1];
                    var logicNamespace = args[2];
                    var namespaceImports = new string[]
                    {
                        "using " + logicNamespace + ".Models;",
                        "using " + mainNamespace + ".Core;",
                        "using " + mainNamespace + ".Entities;"
                    };

                    var ctrlNamespaceImports = new string[]
                    {
                        "using " + mainNamespace + ".Models.Views;",
                        "using " + logicNamespace + ".Models;",
                        "using " + logicNamespace + ".Services;",
                        "using " + mainNamespace + ".Core;",
                        "using System;",
                        "using System.Collections.Generic;",
                        "using System.Linq;",
                        "using System.Web.Mvc;"
                    };

                    var t1 = Task.Factory.StartNew(() =>
                    {
                        using (StreamWriter log = new StreamWriter(string.Format("ViewModelLog{0:yyyyMMddHHmmss}.txt", DateTime.Now)))
                        {
                            CodeUtility.CreateModelClassFiles(assemblyPath, mainNamespace + ".Models.Views", "ViewModel", mainNamespace + "\\Models\\Views", log, true);
                        }
                    });

                    var t2 = Task.Factory.StartNew(() =>
                    {
                        using (StreamWriter log = new StreamWriter(string.Format("ModelLog{0:yyyyMMddHHmmss}.txt", DateTime.Now)))
                        {
                            CodeUtility.CreateModelClassFiles(assemblyPath, logicNamespace + ".Models", "Model", logicNamespace + "\\Models", log, false);
                        }
                    });

                    var t3 = Task.Factory.StartNew(() =>
                    {
                        using (StreamWriter log = new StreamWriter(string.Format("ControllerLog{0:yyyyMMddHHmmss}.txt", DateTime.Now)))
                        {
                            CodeUtility.CreateControllersClassFiles(assemblyPath, mainNamespace + ".Controllers", "Controller", "ViewModel", "Model", "Service", mainNamespace + "\\Controllers", log, ctrlNamespaceImports);
                        }
                    });

                    var t4 = Task.Factory.StartNew(() =>
                    {
                        using (StreamWriter log = new StreamWriter(string.Format("ServiceLog{0:yyyyMMddHHmmss}.txt", DateTime.Now)))
                        {
                            CodeUtility.CreateServiceClassFiles(assemblyPath, logicNamespace + ".Services", "Service", "Model", logicNamespace + "\\Services", log, namespaceImports);
                        }
                    });

                    var t5 = Task.Factory.StartNew(() =>
                    {
                        using (StreamWriter log = new StreamWriter(string.Format("ViewFileLog{0:yyyyMMddHHmmss}.txt", DateTime.Now)))
                        {
                            CodeUtility.CreateViewFiles(assemblyPath, mainNamespace + ".Models.Views", "ViewModel", mainNamespace + "\\Views", log);
                        }
                    });

                    Task.WaitAll(t1, t2, t3, t4, t5);

                    Console.WriteLine("Model creation completed...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    errorLog.WriteLine("[{0:yyyy-MM-dd HH:mm:ss}] : {1}", DateTime.Now, ex);
                }
            }
        }
    }
}