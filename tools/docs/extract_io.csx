// Regenerates docs/data/io.json (the input/output tables behind every [[io:Name]] shortcode)
// from the Robot Components components that are loaded in Grasshopper right now.
//
// Run it inside Rhino 8 with the CURRENT build of Robot Components loaded:
//   - Rhino MCP: run_csharp with this file's content, or
//   - Rhino's ScriptEditor (C#): paste and run.
// Then review `git diff docs/data/io.json` and run `mkdocs build --strict`.
//
// Right-click (menu) parameters that aren't registered yet are found through the component's
// private IGH_Param fields and written with "menu": true. "Draw Full Names" is switched off
// while extracting so that nicknames are the real nicknames.

using Grasshopper;
using Grasshopper.Kernel;
using System.Reflection;
using System.Text;

const string Repo = @"C:\Users\jpdrude\OneDrive\Dateien\01_Forschung\02_RobotComponents\repository";

bool orig = CentralSettings.CanvasFullNames;
CentralSettings.CanvasFullNames = false;
try
{
    string J(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n") + "\"";
    string P(IGH_Param p, bool optionalMenu) =>
        "{\"name\": " + J(p.Name) + ", \"nick\": " + J(p.NickName) + ", \"type\": " + J(p.TypeName) + ", \"typeGuid\": " + J(p.ComponentGuid.ToString()) +
        ", \"access\": " + J(p.Access.ToString()) + ", \"optional\": " + (p.Optional ? "true" : "false") + ", \"menu\": " + (optionalMenu ? "true" : "false") +
        ", \"description\": " + J(p.Description) + "}";

    var proxies = Instances.ComponentServer.ObjectProxies.Where(p => p.Desc.Category == "Robot Components ABB" && !p.Obsolete &&
        p.Exposure != GH_Exposure.hidden && p.Kind == GH_ObjectType.CompiledObject &&
        p.Desc.SubCategory != "Parameters" && p.Desc.SubCategory != "EDEK Presets").ToList();

    var sb = new StringBuilder("{\n");
    int n = 0;
    foreach (var px in proxies)
    {
        var c = Instances.ComponentServer.EmitObject(px.Guid) as IGH_Component;
        if (c == null) continue;
        var ins = c.Params.Input.ToList();
        var outs = c.Params.Output.ToList();
        var extraIn = new List<IGH_Param>();
        var extraOut = new List<IGH_Param>();
        foreach (var f in c.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            IEnumerable<IGH_Param> arr = null;
            var v = f.GetValue(c);
            if (v is IGH_Param[] a) arr = a;
            else if (v is List<IGH_Param> l) arr = l;
            else if (v is IGH_Param single && f.Name.ToLower().Contains("param")) arr = new[] { single };
            if (arr == null) continue;
            bool isOut = f.Name.ToLower().Contains("output");
            foreach (var p in arr.Where(p => p != null))
            {
                var t = isOut ? outs : ins;
                var e = isOut ? extraOut : extraIn;
                if (!t.Any(q => q.Name == p.Name) && !e.Any(q => q.Name == p.Name)) e.Add(p);
            }
        }
        // The Controller Utility signal components share their names with the Code Generation
        // ones; the wiki registry calls them "... (Controller)".
        string key = px.Desc.SubCategory == "Controller Utility" && (px.Desc.Name.StartsWith("Get ") || px.Desc.Name.StartsWith("Set ")) &&
                     (px.Desc.Name.Contains("Input") || px.Desc.Name.Contains("Output")) ? px.Desc.Name + " (Controller)" : px.Desc.Name;
        sb.Append(n++ == 0 ? "" : ",\n");
        sb.Append("  " + J(key) + ": {\"nick\": " + J(px.Desc.NickName) + ", \"subcategory\": " + J(px.Desc.SubCategory) + ", \"guid\": " + J(px.Guid.ToString()) +
            ", \"description\": " + J(px.Desc.Description.Split(new[] { "\r\n\r\nRobot Components:", "\n\nRobot Components:" }, StringSplitOptions.None)[0]) +
            ", \"inputs\": [" + string.Join(", ", ins.Select(p => P(p, false)).Concat(extraIn.Select(p => P(p, true)))) + "]" +
            ", \"outputs\": [" + string.Join(", ", outs.Select(p => P(p, false)).Concat(extraOut.Select(p => P(p, true)))) + "]}");
    }
    sb.Append("\n}\n");
    System.IO.File.WriteAllText(System.IO.Path.Combine(Repo, @"docs\data\io.json"), sb.ToString());
    Console.WriteLine("components written to io.json: " + n);
}
finally
{
    CentralSettings.CanvasFullNames = orig;
}
