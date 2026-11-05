// Exports the toolbar icons of all Robot Components components and parameters to
// docs/assets/icons/rc/<slug>.png and rewrites their entries in docs/assets/icons/manifest.json.
// Entries for native Grasshopper icons (assets/icons/gh/...) are kept as they are.
//
// Run it inside Rhino 8 with the CURRENT build of Robot Components loaded (Rhino MCP run_csharp,
// or Rhino's ScriptEditor). New components then also need an entry in docs/data/registry.yml
// (icon path, page) before pages can use [[Name]].
//
// Slugs: the component name in lowercase with hyphens; "param-" for parameters; the toolbar
// tab as a prefix when two components share a name (e.g. controller-utility-set-digital-output).
//
// Native Grasshopper icons (assets/icons/gh/) are exported by hand, e.g.
//   Instances.ComponentServer.EmitObjectProxy(new Guid("<guid>")).Icon.Save(path, ImageFormat.Png);

using Grasshopper;
using Grasshopper.Kernel;
using System.Text.Json;
using System.Text.RegularExpressions;

const string Repo = @"C:\Users\jpdrude\OneDrive\Dateien\01_Forschung\02_RobotComponents\repository";
var docs = System.IO.Path.Combine(Repo, "docs");
var manifestPath = System.IO.Path.Combine(docs, @"assets\icons\manifest.json");

string Slug(string s) => Regex.Replace(s.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
string J(string s) => JsonSerializer.Serialize(s ?? "");

// keep the existing non-Robot-Components entries
var kept = new List<string>();
if (System.IO.File.Exists(manifestPath))
{
    using var old = JsonDocument.Parse(System.IO.File.ReadAllText(manifestPath));
    foreach (var e in old.RootElement.EnumerateArray())
        if (!e.GetProperty("icon").GetString().StartsWith("assets/icons/rc/")) kept.Add("  " + e.GetRawText());
}

var rc = Instances.ComponentServer.ObjectProxies.Where(p => p.Desc.Category == "Robot Components ABB" && !p.Obsolete &&
    p.Exposure != GH_Exposure.hidden && p.Kind == GH_ObjectType.CompiledObject && p.Desc.SubCategory != "EDEK Presets").ToList();
var dir = System.IO.Path.Combine(docs, @"assets\icons\rc");
System.IO.Directory.CreateDirectory(dir);
var entries = new List<string>();
foreach (var p in rc)
{
    var slug = Slug(p.Desc.Name);
    if (p.Desc.SubCategory == "Parameters") slug = "param-" + slug;
    else if (rc.Count(q => Slug(q.Desc.Name) == Slug(p.Desc.Name) && q.Desc.SubCategory != "Parameters") > 1) slug = Slug(p.Desc.SubCategory) + "-" + slug;
    if (p.Icon != null) p.Icon.Save(System.IO.Path.Combine(dir, slug + ".png"), System.Drawing.Imaging.ImageFormat.Png);
    entries.Add("  {\"name\": " + J(p.Desc.Name) + ", \"kind\": " + J(p.Kind.ToString()) + ", \"subcategory\": " + J(p.Desc.SubCategory) +
                ", \"guid\": " + J(p.Guid.ToString()) + ", \"icon\": " + J("assets/icons/rc/" + slug + ".png") + "}");
}
System.IO.File.WriteAllText(manifestPath, "[\n" + string.Join(",\n", entries.Concat(kept)) + "\n]\n");
Console.WriteLine($"Robot Components icons: {entries.Count}; other entries kept: {kept.Count}");
