using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class CodeUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateControllersClassFiles(string assemblyPath, string mainNamespace, string classSuffix, string viewClassSuffix, string modelClassSuffix, string serviceClassSuffix, string folderName, StreamWriter log, IEnumerable<string> namespaceImports)
        {
            IEnumerable<string> typeNames = null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            var types = assembly.GetTypes();
            var hashKeys = new HashSet<string>();
            var specificHashKeys = new HashSet<string>();
            var folderPath = Path.Combine(Environment.CurrentDirectory, folderName);
            Directory.CreateDirectory(folderPath);

            Parallel.ForEach(GetFilteredTypes(types), x =>
            {
                hashKeys.Add(x.Name);
            });

            if (typeNames != null && typeNames.Count() > 0)
            {
                specificHashKeys = new HashSet<string>(typeNames);
            }
            else
            {
                specificHashKeys = new HashSet<string>(hashKeys);
            }

            ConcurrentQueue<string> buffer = new ConcurrentQueue<string>();

            Parallel.ForEach(GetFilteredTypes(types), type =>
            {
                try
                {
                    if (!type.IsGenericType && !type.Name.Contains("<"))
                    {
                        var typeName = type.Name;
                        if (specificHashKeys.Contains(typeName))
                        {
                            var mainTypeName = GetCamelCaseName(typeName);
                            typeName = GetCamelCaseName(typeName) + classSuffix;
                            buffer.Enqueue(string.Format("[{0:yyyy-MM-dd HH:mm:ss}] : {1}.cs", DateTime.Now, typeName));

                            using (StreamWriter writer = new StreamWriter(string.Format("{0}\\{1}.cs", folderPath, typeName)))
                            {
                                foreach (var namespaceToWrite in namespaceImports)
                                {
                                    writer.WriteLine(namespaceToWrite);
                                }

                                writer.WriteLine();
                                writer.WriteLine("namespace " + mainNamespace);
                                writer.WriteLine("{");
                                writer.WriteLine("    public class {0} : Controller", typeName);
                                writer.WriteLine("    {");
                                writer.WriteLine("        private readonly IServiceFactory _factory = null;");
                                writer.WriteLine("        private readonly Func<{0}{1}, {0}{2}> _toBusinessModel = null;", mainTypeName, viewClassSuffix, modelClassSuffix);
                                writer.WriteLine("        private readonly Func<{0}{1}, {0}{2}> _toViewModel = null;", mainTypeName, modelClassSuffix, viewClassSuffix);

                                var propertyNames = new List<string>();

                                foreach (var property in type.GetProperties())
                                {
                                    var nullableType = Nullable.GetUnderlyingType(property.PropertyType);
                                    var isNullable = nullableType != null;

                                    if (!property.PropertyType.IsGenericType || isNullable)
                                    {
                                        var localType = isNullable ? nullableType : property.PropertyType;
                                        var propertytypeName = GetFriendlyTypeName(localType.Name, isNullable);
                                        if (!hashKeys.Contains(propertytypeName))
                                        {
                                            var propertyName = GetCamelCaseName(property.Name);

                                            if (propertyName != "RowVersion")
                                            {
                                                propertyNames.Add(propertyName);
                                            }
                                        }
                                    }
                                }

                                writer.WriteLine();
                                writer.WriteLine("        public {0}(IServiceFactory factory)", typeName);
                                writer.WriteLine("        {");
                                writer.WriteLine("            this._factory = factory;");
                                writer.WriteLine();
                                writer.WriteLine("            this._toViewModel = x => new {0}{1}", mainTypeName, viewClassSuffix);
                                writer.WriteLine("            {");

                                foreach (var propertyName in propertyNames)
                                {
                                    writer.WriteLine("                {0} = x.{0},", propertyName);
                                }

                                writer.WriteLine("            };");
                                writer.WriteLine();
                                writer.WriteLine("            this._toBusinessModel = x => new {0}{1}", mainTypeName, modelClassSuffix);
                                writer.WriteLine("            {");
                                foreach (var propertyName in propertyNames)
                                {
                                    writer.WriteLine("                {0} = x.{0},", propertyName);
                                }
                                writer.WriteLine("            };");
                                writer.WriteLine("        }");
                                writer.WriteLine();

                                // Create Action : GET
                                writer.WriteLine("        //");
                                writer.WriteLine("        // GET: /{0}/Create", mainTypeName);
                                writer.WriteLine("        public ActionResult Create()");
                                writer.WriteLine("        {");
                                writer.WriteLine("            return View(new {0}{1}());", mainTypeName, viewClassSuffix);
                                writer.WriteLine("        }");
                                writer.WriteLine();

                                // Create Action : POST
                                WritePostAction(writer, "Create", mainTypeName, viewClassSuffix, serviceClassSuffix, "Add", true);
                                writer.WriteLine();

                                // Delete Action : GET
                                WriteDisplayAction(writer, "Delete", mainTypeName, viewClassSuffix, serviceClassSuffix);
                                writer.WriteLine();

                                // Delete Action : POST
                                WritePostAction(writer, "Delete", mainTypeName, viewClassSuffix, serviceClassSuffix, "Delete", false);
                                writer.WriteLine();

                                // Details Action : GET
                                WriteDisplayAction(writer, "Details", mainTypeName, viewClassSuffix, serviceClassSuffix);
                                writer.WriteLine();

                                // Edit Action : GET
                                WriteDisplayAction(writer, "Edit", mainTypeName, viewClassSuffix, serviceClassSuffix);
                                writer.WriteLine();

                                // Edit Action : POST
                                WritePostAction(writer, "Edit", mainTypeName, viewClassSuffix, serviceClassSuffix, "Update", false);
                                writer.WriteLine();

                                // Index Action : GET
                                writer.WriteLine("        //");
                                writer.WriteLine("        // GET: /{0}/", mainTypeName);
                                writer.WriteLine("        public ActionResult Index(int? page = 1)");
                                writer.WriteLine("        {");
                                writer.WriteLine("            IPagedList<{0}{1}> modelList = null;", mainTypeName, viewClassSuffix);
                                writer.WriteLine();
                                writer.WriteLine("            using (ModelRepository db = new ModelRepository(this._factory.GetDatabaseObject()))");
                                writer.WriteLine("            {");
                                writer.WriteLine("                {0}{1} service = new {0}{1}(db);", mainTypeName, serviceClassSuffix);
                                writer.WriteLine("                modelList = service.GetPagedList(this._toViewModel, page ?? 1);");
                                writer.WriteLine("            }");
                                writer.WriteLine();
                                writer.WriteLine("            return View(modelList);");
                                writer.WriteLine("        }");
                                writer.WriteLine("    }");
                                writer.Write("}");
                            }
                        }
                    }
                }
                catch
                {
                }
            });

            log.AutoFlush = false;
            string line = null;
            while (buffer.TryDequeue(out line))
            {
                log.WriteLine(line);
            }
            log.Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateModelClassFiles(string assemblyPath, string mainNamespace, string classSuffix, string folderName, StreamWriter log, bool isViewModel)
        {
            IEnumerable<string> typeNames = null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            var types = assembly.GetTypes();
            var hashKeys = new HashSet<string>();
            var specificHashKeys = new HashSet<string>();
            var folderPath = Path.Combine(Environment.CurrentDirectory, folderName);
            Directory.CreateDirectory(folderPath);

            Parallel.ForEach(GetFilteredTypes(types), x =>
            {
                hashKeys.Add(x.Name);
            });

            if (typeNames != null && typeNames.Count() > 0)
            {
                specificHashKeys = new HashSet<string>(typeNames);
            }
            else
            {
                specificHashKeys = new HashSet<string>(hashKeys);
            }

            ConcurrentQueue<string> buffer = new ConcurrentQueue<string>();

            Parallel.ForEach(GetFilteredTypes(types), type =>
            {
                try
                {
                    if (!type.IsGenericType && !type.Name.Contains("<"))
                    {
                        var typeName = type.Name;
                        if (specificHashKeys.Contains(typeName))
                        {
                            typeName = GetCamelCaseName(typeName) + classSuffix;
                            buffer.Enqueue(string.Format("[{0:yyyy-MM-dd HH:mm:ss}] : {1}.cs", DateTime.Now, typeName));

                            using (StreamWriter writer = new StreamWriter(string.Format("{0}\\{1}.cs", folderPath, typeName)))
                            {
                                writer.WriteLine("using System;");

                                if (isViewModel)
                                {
                                    writer.WriteLine("using System.ComponentModel;");
                                }

                                writer.WriteLine();
                                writer.WriteLine("namespace " + mainNamespace);
                                writer.WriteLine("{");
                                writer.WriteLine("    public class {0}", typeName);
                                writer.WriteLine("    {");

                                foreach (var property in type.GetProperties())
                                {
                                    var nullableType = Nullable.GetUnderlyingType(property.PropertyType);
                                    var isNullable = nullableType != null;

                                    if (!property.PropertyType.IsGenericType || isNullable)
                                    {
                                        var localType = isNullable ? nullableType : property.PropertyType;
                                        var propertytypeName = GetFriendlyTypeName(localType.Name, isNullable);
                                        if (!hashKeys.Contains(propertytypeName))
                                        {
                                            var propertyName = GetCamelCaseName(property.Name);

                                            if (propertyName != "RowVersion")
                                            {
                                                writer.WriteLine();
                                                writer.WriteLine("        // " + property.Name);

                                                if (isViewModel)
                                                {
                                                    writer.WriteLine(string.Format("        [DisplayName(\"{0}\")]", GetDisplayName(propertyName)));
                                                }

                                                writer.WriteLine(string.Format("        public {0} {1}", propertytypeName, GetCamelCaseName(property.Name)) + " { get; set; }");
                                            }
                                        }
                                    }
                                }

                                writer.WriteLine("    }");
                                writer.Write("}");
                            }
                        }
                    }
                }
                catch
                {
                }
            });

            log.AutoFlush = false;
            string line = null;
            while (buffer.TryDequeue(out line))
            {
                log.WriteLine(line);
            }
            log.Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateServiceClassFiles(string assemblyPath, string mainNamespace, string serviceClassSuffix, string modelClassSuffix, string folderName, StreamWriter log, IEnumerable<string> namespaceImports)
        {
            var folderPath = Path.Combine(Environment.CurrentDirectory, folderName);
            Directory.CreateDirectory(folderPath);

            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            var typeList = assembly.GetTypes();
            var typeMap = new Dictionary<Type, Tuple<string, string>>();

            foreach (var type in GetFilteredTypes(typeList))
            {
                typeMap.Add(type, new Tuple<string, string>(type.Name, GetCamelCaseName(type.Name)));
            }

            ConcurrentQueue<string> buffer = new ConcurrentQueue<string>();

            Parallel.ForEach(typeMap, typeKey =>
            {
                var typeName = typeKey.Value.Item2 + serviceClassSuffix;
                var primaryTypeName = typeKey.Value.Item1;
                var convertedTypeName = typeKey.Value.Item2 + modelClassSuffix;

                using (StreamWriter writer = new StreamWriter(string.Format("{0}\\{1}.cs", folderPath, typeName)))
                {
                    buffer.Enqueue(string.Format("[{0:yyyy-MM-dd HH:mm:ss}] : {1}.cs", DateTime.Now, typeName));

                    foreach (var lineToWrite in namespaceImports)
                    {
                        writer.WriteLine(lineToWrite);
                    }

                    writer.WriteLine();
                    writer.WriteLine("namespace " + mainNamespace);
                    writer.WriteLine("{");
                    writer.WriteLine("    public class {0} : ModelService<{1}, {2}, long>", typeName, primaryTypeName, convertedTypeName);
                    writer.WriteLine("    {");
                    writer.WriteLine("        public {0}(ModelRepository db)", typeName);
                    writer.WriteLine("            : base(new ModelServiceBase<{0}, {1}, long>(db, x =>", primaryTypeName, convertedTypeName);
                    writer.WriteLine("                  new {0}", convertedTypeName);
                    writer.WriteLine("                  {");

                    var propertyMap = new Dictionary<string, string>();

                    foreach (var property in typeKey.Key.GetProperties())
                    {
                        var nullableType = Nullable.GetUnderlyingType(property.PropertyType);
                        var isNullable = nullableType != null;

                        if (!property.PropertyType.IsGenericType || isNullable)
                        {
                            var localType = isNullable ? nullableType : property.PropertyType;
                            var propertytypeName = GetFriendlyTypeName(localType.Name, isNullable);
                            if (!typeMap.ContainsKey(property.PropertyType))
                            {
                                var propertyName = GetCamelCaseName(property.Name);

                                if (propertyName != "RowVersion")
                                {
                                    propertyMap.Add(property.Name, propertyName);
                                }
                            }
                        }
                    }

                    foreach (var property in propertyMap)
                    {
                        writer.WriteLine("                      {0} = x.{1},", property.Value, property.Key);
                    }

                    writer.WriteLine("                  }, x =>");
                    writer.WriteLine("                  new {0}", primaryTypeName);
                    writer.WriteLine("                  {");

                    foreach (var property in propertyMap)
                    {
                        writer.WriteLine("                      {0} = x.{1},", property.Key, property.Value);
                    }

                    writer.WriteLine("                  }, x => x.id))");
                    writer.WriteLine("        {");
                    writer.WriteLine("        }");
                    writer.WriteLine("    }");
                    writer.Write("}");
                }
            });

            log.AutoFlush = false;
            string line = null;
            while (buffer.TryDequeue(out line))
            {
                log.WriteLine(line);
            }
            log.Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateViewFiles(string assemblyPath, string mainNamespace, string viewClassSuffix, string folderName, StreamWriter log)
        {
            IEnumerable<string> typeNames = null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            var types = assembly.GetTypes();
            var hashKeys = new HashSet<string>();
            var specificHashKeys = new HashSet<string>();
            var folderPath = Path.Combine(Environment.CurrentDirectory, folderName);
            Directory.CreateDirectory(folderPath);

            Parallel.ForEach(GetFilteredTypes(types), x =>
            {
                hashKeys.Add(x.Name);
            });

            if (typeNames != null && typeNames.Count() > 0)
            {
                specificHashKeys = new HashSet<string>(typeNames);
            }
            else
            {
                specificHashKeys = new HashSet<string>(hashKeys);
            }

            ConcurrentQueue<string> buffer = new ConcurrentQueue<string>();

            Parallel.ForEach(GetFilteredTypes(types), type =>
            {
                try
                {
                    if (!type.IsGenericType && !type.Name.Contains("<"))
                    {
                        var typeName = type.Name;
                        if (specificHashKeys.Contains(typeName))
                        {
                            var mainTypeName = GetCamelCaseName(typeName);
                            typeName = GetCamelCaseName(typeName);
                            buffer.Enqueue(string.Format("[{0:yyyy-MM-dd HH:mm:ss}] : {1}.cshtml", DateTime.Now, typeName));
                            var localFolderPath = Path.Combine(folderPath, mainTypeName);
                            Directory.CreateDirectory(localFolderPath);

                            var propertyNames = new Dictionary<string, Type>();

                            foreach (var property in type.GetProperties())
                            {
                                var nullableType = Nullable.GetUnderlyingType(property.PropertyType);
                                var isNullable = nullableType != null;

                                if (!property.PropertyType.IsGenericType || isNullable)
                                {
                                    var localType = isNullable ? nullableType : property.PropertyType;
                                    var propertytypeName = GetFriendlyTypeName(localType.Name, isNullable);
                                    if (!hashKeys.Contains(propertytypeName))
                                    {
                                        var propertyName = GetCamelCaseName(property.Name);

                                        if (propertyName != "RowVersion")
                                        {
                                            propertyNames.Add(propertyName, property.PropertyType);
                                        }
                                    }
                                }
                            }

                            // Create.cshtml
                            using (StreamWriter writer = new StreamWriter(string.Format("{0}\\Create.cshtml", localFolderPath)))
                            {
                                writer.WriteLine("@model {0}{1}", typeName, viewClassSuffix);
                                writer.WriteLine();
                                writer.WriteLine("@{");
                                writer.WriteLine("    ViewBag.Title = \"Create\";");
                                writer.WriteLine("}");
                                writer.WriteLine();
                                writer.WriteLine("<h2>Create</h2>");
                                writer.WriteLine();
                                writer.WriteLine();
                                writer.WriteLine("@using (Html.BeginForm()) ");
                                writer.WriteLine("{");
                                writer.WriteLine("    @Html.AntiForgeryToken()");
                                writer.WriteLine();
                                writer.WriteLine("    <div class=\"form-horizontal\">");
                                writer.WriteLine("        <h4>{0}</h4>", GetDisplayName(typeName));
                                writer.WriteLine("        <hr />");
                                writer.WriteLine("        @Html.ValidationSummary(true)");
                                writer.WriteLine();

                                foreach (var localProperty in propertyNames)
                                {
                                    if (localProperty.Key != "Id")
                                    {
                                        var key = localProperty.Key;

                                        writer.WriteLine("        <div class=\"form-group\">");
                                        writer.WriteLine("            @Html.LabelFor(model => model." + key + ", new { @class = \"control-label col-md-2\" })");
                                        writer.WriteLine("            <div class=\"col-md-10\">");
                                        if (localProperty.Value == typeof(string))
                                        {
                                            writer.WriteLine("                @Html.TextBoxFor(model => model." + key + ", new { @class = \"form-control\" })");
                                        }
                                        else
                                        {
                                            writer.WriteLine("                @Html.EditorFor(model => model." + key + ", new { @class = \"form-control\" })");
                                        }

                                        writer.WriteLine("                @Html.ValidationMessageFor(model => model." + key + ")");
                                        writer.WriteLine("            </div>");
                                        writer.WriteLine("        </div>");
                                    }
                                }
                                writer.WriteLine();
                                writer.WriteLine("        <div class=\"form-group\">");
                                writer.WriteLine("            <div class=\"col-md-offset-2 col-md-10\">");
                                writer.WriteLine("                <input type=\"submit\" value=\"Create\" class=\"btn btn-default\" />");
                                writer.WriteLine("            </div>");
                                writer.WriteLine("        </div>");
                                writer.WriteLine("    </div>");
                                writer.WriteLine("}");
                                writer.WriteLine();
                                writer.WriteLine("<div>");
                                writer.WriteLine("    @Html.ActionLink(\"Back to List\", \"Index\")");
                                writer.WriteLine("</div>");
                                writer.WriteLine();
                                writer.WriteLine("@section Scripts {");
                                writer.WriteLine("    @Scripts.Render(\"~/bundles/jqueryval\")");
                                writer.Write("}");
                            }

                            // Delete.cshtml
                            using (StreamWriter writer = new StreamWriter(string.Format("{0}\\Delete.cshtml", localFolderPath)))
                            {
                                writer.WriteLine("@model {0}{1}", typeName, viewClassSuffix);
                                writer.WriteLine();
                                writer.WriteLine("@{");
                                writer.WriteLine("    ViewBag.Title = \"Delete\";");
                                writer.WriteLine("}");
                                writer.WriteLine();
                                writer.WriteLine("<h2>Delete</h2>");
                                writer.WriteLine();
                                writer.WriteLine("<h3>Are you sure you want to delete this?</h3>");
                                writer.WriteLine("<div>");
                                writer.WriteLine("    <h4>{0}</h4>", GetDisplayName(typeName));
                                writer.WriteLine("    <hr />");

                                foreach (var localProperty in propertyNames)
                                {
                                    if (localProperty.Key != "Id")
                                    {
                                        var key = localProperty.Key;

                                        writer.WriteLine("    <dl class=\"dl-horizontal\">");
                                        writer.WriteLine("        <dt>");
                                        writer.WriteLine("            @Html.DisplayNameFor(model => model." + key + ")");
                                        writer.WriteLine("        </dt>");
                                        writer.WriteLine();
                                        writer.WriteLine("        <dd>");
                                        writer.WriteLine("            @Html.DisplayFor(model => model." + key + ")");
                                        writer.WriteLine("        </dd>");
                                        writer.WriteLine();
                                        writer.WriteLine("    </dl>");
                                    }
                                }

                                writer.WriteLine("    @using (Html.BeginForm()) {");
                                writer.WriteLine("        @Html.AntiForgeryToken()");
                                writer.WriteLine();
                                writer.WriteLine("        <div class=\"form-actions no-color\">");
                                writer.WriteLine("            <input type=\"submit\" value=\"Delete\" class=\"btn btn-default\" /> |");
                                writer.WriteLine("            @Html.ActionLink(\"Back to List\", \"Index\")");
                                writer.WriteLine("        </div>");
                                writer.WriteLine("    }");
                                writer.WriteLine("</div>");
                            }

                            // Details.cshtml
                            using (StreamWriter writer = new StreamWriter(string.Format("{0}\\Details.cshtml", localFolderPath)))
                            {
                                writer.WriteLine("@model {0}{1}", typeName, viewClassSuffix);
                                writer.WriteLine();
                                writer.WriteLine("@{");
                                writer.WriteLine("    ViewBag.Title = \"Details\";");
                                writer.WriteLine("}");
                                writer.WriteLine();
                                writer.WriteLine("<h2>Details</h2>");
                                writer.WriteLine();
                                writer.WriteLine("<div>");
                                writer.WriteLine("    <h4>{0}</h4>", GetDisplayName(typeName));
                                writer.WriteLine("    <hr />");

                                foreach (var localProperty in propertyNames)
                                {
                                    if (localProperty.Key != "Id")
                                    {
                                        var key = localProperty.Key;

                                        writer.WriteLine("    <dl class=\"dl-horizontal\">");
                                        writer.WriteLine("        <dt>");
                                        writer.WriteLine("            @Html.DisplayNameFor(model => model." + key + ")");
                                        writer.WriteLine("        </dt>");
                                        writer.WriteLine();
                                        writer.WriteLine("        <dd>");
                                        writer.WriteLine("            @Html.DisplayFor(model => model." + key + ")");
                                        writer.WriteLine("        </dd>");
                                        writer.WriteLine();
                                        writer.WriteLine("    </dl>");
                                    }
                                }

                                writer.WriteLine("</div>");
                                writer.WriteLine("<p>");
                                writer.WriteLine("    @Html.ActionLink(\"Edit\", \"Edit\", new { id = Model.Id }) |");
                                writer.WriteLine("    @Html.ActionLink(\"Back to List\", \"Index\")");
                                writer.WriteLine("</p>");
                            }

                            // Edit.cshtml
                            using (StreamWriter writer = new StreamWriter(string.Format("{0}\\Edit.cshtml", localFolderPath)))
                            {
                                writer.WriteLine("@model {0}{1}", typeName, viewClassSuffix);
                                writer.WriteLine();
                                writer.WriteLine("@{");
                                writer.WriteLine("    ViewBag.Title = \"Edit\";");
                                writer.WriteLine("}");
                                writer.WriteLine();
                                writer.WriteLine("<h2>Edit</h2>");
                                writer.WriteLine();
                                writer.WriteLine();
                                writer.WriteLine("@using (Html.BeginForm()) ");
                                writer.WriteLine("{");
                                writer.WriteLine("    @Html.AntiForgeryToken()");
                                writer.WriteLine();
                                writer.WriteLine("    <div class=\"form-horizontal\">");
                                writer.WriteLine("        <h4>{0}</h4>", GetDisplayName(typeName));
                                writer.WriteLine("        <hr />");
                                writer.WriteLine("        @Html.ValidationSummary(true)");
                                writer.WriteLine("        @Html.HiddenFor(model => model.Id)");
                                writer.WriteLine();

                                foreach (var localProperty in propertyNames)
                                {
                                    if (localProperty.Key != "Id")
                                    {
                                        var key = localProperty.Key;

                                        writer.WriteLine("        <div class=\"form-group\">");
                                        writer.WriteLine("            @Html.LabelFor(model => model." + key + ", new { @class = \"control-label col-md-2\" })");
                                        writer.WriteLine("            <div class=\"col-md-10\">");

                                        if (localProperty.Value == typeof(string))
                                        {
                                            writer.WriteLine("                @Html.TextBoxFor(model => model." + key + ", new { @class = \"form-control\" })");
                                        }
                                        else
                                        {
                                            writer.WriteLine("                @Html.EditorFor(model => model." + key + ", new { @class = \"form-control\" })");
                                        }

                                        writer.WriteLine("                @Html.ValidationMessageFor(model => model." + key + ")");
                                        writer.WriteLine("            </div>");
                                        writer.WriteLine("        </div>");
                                    }
                                }
                                writer.WriteLine();
                                writer.WriteLine("        <div class=\"form-group\">");
                                writer.WriteLine("            <div class=\"col-md-offset-2 col-md-10\">");
                                writer.WriteLine("                <input type=\"submit\" value=\"Save\" class=\"btn btn-default\" />");
                                writer.WriteLine("            </div>");
                                writer.WriteLine("        </div>");
                                writer.WriteLine("    </div>");
                                writer.WriteLine("}");
                                writer.WriteLine();
                                writer.WriteLine("<div>");
                                writer.WriteLine("    @Html.ActionLink(\"Back to List\", \"Index\")");
                                writer.WriteLine("</div>");
                                writer.WriteLine();
                                writer.WriteLine("@section Scripts {");
                                writer.WriteLine("    @Scripts.Render(\"~/bundles/jqueryval\")");
                                writer.Write("}");
                            }

                            // Index.cshtml
                            using (StreamWriter writer = new StreamWriter(string.Format("{0}\\Index.cshtml", localFolderPath)))
                            {
                                writer.WriteLine("@model IPagedList<{0}{1}>", typeName, viewClassSuffix);
                                writer.WriteLine();
                                writer.WriteLine("@{");
                                writer.WriteLine("    ViewBag.Title = \"Index\";");
                                writer.WriteLine("}");
                                writer.WriteLine();
                                writer.WriteLine("<h2>Index</h2>");
                                writer.WriteLine();
                                writer.WriteLine("<h4>{0}</h4>", GetDisplayName(typeName));
                                writer.WriteLine();
                                writer.WriteLine("<p>");
                                writer.WriteLine("    @Html.ActionLink(\"Create New\", \"Create\")");
                                writer.WriteLine("</p>");
                                writer.WriteLine();
                                writer.WriteLine("@Html.Pager(Model, page => Url.Action(\"Index\", new { page = page }))");
                                writer.WriteLine();
                                writer.WriteLine("<table class=\"table\">");
                                writer.WriteLine("    <tr>");

                                foreach (var localProperty in propertyNames)
                                {
                                    if (localProperty.Key != "Id")
                                    {
                                        var key = localProperty.Key;

                                        writer.WriteLine("        <th>");
                                        writer.WriteLine("            @Html.DisplayNameFor(model => model.FirstOrDefault()." + key + ")");
                                        writer.WriteLine("        </th>");
                                    }
                                }

                                writer.WriteLine("        <th>");
                                writer.WriteLine("            Actions");
                                writer.WriteLine("        </th>");
                                writer.WriteLine("    </tr>");
                                writer.WriteLine();
                                writer.WriteLine("@foreach (var item in Model) {");
                                writer.WriteLine("    <tr>");

                                foreach (var localProperty in propertyNames)
                                {
                                    if (localProperty.Key != "Id")
                                    {
                                        var key = localProperty.Key;

                                        writer.WriteLine("         <td>");
                                        writer.WriteLine("            @Html.DisplayFor(modelItem => item." + key + ")");
                                        writer.WriteLine("         </td>");
                                    }
                                }

                                writer.WriteLine("         <td>");
                                writer.WriteLine("            @Html.ActionLink(\"Edit\", \"Edit\", new { id=item.Id }) |");
                                writer.WriteLine("            @Html.ActionLink(\"Details\", \"Details\", new { id=item.Id }) |");
                                writer.WriteLine("            @Html.ActionLink(\"Delete\", \"Delete\", new { id=item.Id })");
                                writer.WriteLine("        </td>");
                                writer.WriteLine("    </tr>");
                                writer.WriteLine("}");
                                writer.WriteLine();
                                writer.Write("</table>");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    buffer.Enqueue(ex.ToString());
                }
            });

            log.AutoFlush = false;
            string line = null;
            while (buffer.TryDequeue(out line))
            {
                log.WriteLine(line);
            }
            log.Flush();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetCamelCaseName(string name)
        {
            var camelCaseName = string.Empty;
            var tokens = name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            for (var x = 0; x < tokens.Length; x++)
            {
                var token = tokens[x];

                if (x != 0 || token.Length > 1)
                {
                    token = token.Substring(0, 1).ToUpper() + (token.Length > 1 ? token.Substring(1, token.Length - 1) : string.Empty);
                    camelCaseName += token;
                }
            }

            return camelCaseName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDisplayName(string name)
        {
            var displayName = string.Empty;
            var tokens = name.ToCharArray();
            var hashKeys = new HashSet<char>("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());

            for (var x = 0; x < tokens.Length; x++)
            {
                var token = tokens[x];
                displayName += (hashKeys.Contains(token) ? " " : string.Empty) + token;
            }

            displayName = (displayName.EndsWith(" Id") && displayName.Length > 3 ? displayName.Substring(0, displayName.Length - 3) : displayName).Trim();

            return displayName;
        }

        public static IEnumerable<Type> GetFilteredTypes(IEnumerable<Type> typeList)
        {
            foreach (var type in typeList)
            {
                if (!type.IsGenericType && !type.Name.Contains("<") && !type.Name.Contains("=") && (type.BaseType == null || (type.BaseType != null && type.BaseType.Name != "DbContext")))
                {
                    yield return type;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetFriendlyTypeName(string typeName, bool isNullable)
        {
            switch (typeName.ToLower())
            {
                case "string":
                    typeName = "string";
                    break;

                case "int32":
                    typeName = "int";
                    break;

                case "int64":
                    typeName = "long";
                    break;

                case "double":
                    typeName = "double";
                    break;

                case "decimal":
                    typeName = "decimal";
                    break;

                case "boolean":
                    typeName = "bool";
                    break;

                case "byte":
                    typeName = "byte";
                    break;

                case "byte[]":
                    typeName = "byte[]";
                    break;
            }

            typeName = isNullable ? typeName + "?" : typeName;

            return typeName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDisplayAction(StreamWriter writer, string actionName, string mainTypeName, string viewClassSuffix, string serviceClassSuffix)
        {
            writer.WriteLine("        //");
            writer.WriteLine("        // GET: /{0}/{1}", mainTypeName, actionName);
            writer.WriteLine("        public ActionResult {0}(long? id)", actionName);
            writer.WriteLine("        {");
            writer.WriteLine("            if (id == null)");
            writer.WriteLine("            {");
            writer.WriteLine("                return RedirectToAction(\"Index\");");
            writer.WriteLine("            }");
            writer.WriteLine();
            writer.WriteLine("            var viewModel = new {0}{1}();", mainTypeName, viewClassSuffix);
            writer.WriteLine();
            writer.WriteLine("            using (ModelRepository db = new ModelRepository(this._factory.GetDatabaseObject()))");
            writer.WriteLine("            {");
            writer.WriteLine("                {0}{1} service = new {0}{1}(db);", mainTypeName, serviceClassSuffix);
            writer.WriteLine("                var model = service.GetObject(x => x.id == id);");
            writer.WriteLine();
            writer.WriteLine("                if (model == null)");
            writer.WriteLine("                {");
            writer.WriteLine("                    return RedirectToAction(\"Index\");");
            writer.WriteLine("                }");
            writer.WriteLine();
            writer.WriteLine("                viewModel = this._toViewModel(model);");
            writer.WriteLine("            }");
            writer.WriteLine();
            writer.WriteLine("            return View(viewModel);");
            writer.WriteLine("        }");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WritePostAction(StreamWriter writer, string actionName, string mainTypeName, string viewClassSuffix, string serviceClassSuffix, string methodName, bool isAdd)
        {
            writer.WriteLine("        //");
            writer.WriteLine("        // POST: /{0}/{1}", mainTypeName, actionName);
            writer.WriteLine("        [HttpPost]");
            writer.WriteLine("        [ValidateAntiForgeryToken]");
            writer.WriteLine("        public ActionResult {0}({1}{2} viewModel)", actionName, mainTypeName, viewClassSuffix);
            writer.WriteLine("        {");
            writer.WriteLine("            if (!ModelState.IsValid)");
            writer.WriteLine("            {");
            writer.WriteLine("                return View(viewModel);");
            writer.WriteLine("            }");
            writer.WriteLine();
            writer.WriteLine("            using (ModelRepository db = new ModelRepository(this._factory.GetDatabaseObject()))");
            writer.WriteLine("            {");
            writer.WriteLine("                {0}{1} service = new {0}{1}(db);", mainTypeName, serviceClassSuffix);

            if (isAdd)
            {
                writer.WriteLine("                service.{0}(this._toBusinessModel(viewModel));", methodName);
            }
            else
            {
                writer.WriteLine("                var model = service.GetObject(x => x.id == viewModel.Id);");
                writer.WriteLine();
                writer.WriteLine("                if (model == null)");
                writer.WriteLine("                {");
                writer.WriteLine("                    return RedirectToAction(\"Index\");");
                writer.WriteLine("                }");
                writer.WriteLine();
                writer.WriteLine("                service.{0}(this._toBusinessModel(viewModel));", methodName);
                writer.WriteLine();
            }

            writer.WriteLine("                db.SaveChanges();");
            writer.WriteLine("            }");
            writer.WriteLine();
            writer.WriteLine("            return RedirectToAction(\"Index\");");
            writer.WriteLine("        }");
        }
    }
}