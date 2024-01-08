using System;
using System.IO;
using System.Reflection;

namespace BESO
{
    internal class Paths
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        // Input object to be saved as STL.
        // Material props are all based on mm, so STL unit would be converted to mm.
        public static string stl = Path.GetTempPath() + "input.stl";

        public static string load = Path.GetTempPath() + "load-points.json";
        public static string restraint = Path.GetTempPath() + "restraint-points.json";

        public static string result = Path.GetTempPath() + "result.inp";
        public static string resultEscap
        {
            get
            {
                // Sometimes, must write to Python config file in this format:
                // "C:\\Users\\m3\\AppData\\Local\\Temp\\result.inp"
                // Not this format:
                // "C:\Users\m3\AppData\Local\Temp\result.inp"

                Char psep = Path.DirectorySeparatorChar;
                string p = Path.GetTempPath().Replace($@"{psep}", $@"{psep}{psep}");
                return p + "result.inp";
            }
        }
        public static string resultNoExt = Path.GetTempPath() + "result";
        public static string report = Path.GetTempPath() + "report.json";

        public static string specs = Path.GetTempPath() + "specs.json";

        public static string fe = Path.Combine(AssemblyDirectory, "finite_elements.exe");

        public static string ccx = Path.Combine(AssemblyDirectory, "ccx_static.exe");

        public static string cgx_cfg_fea_org = Path.Combine(AssemblyDirectory, "cfg.fbd");
        public static string cgx_cfg_fea_new = Path.GetTempPath() + "cfg.fbd";
        public static string result_frd = Path.GetTempPath() + "result.frd";

        public static string cgx = Path.Combine(AssemblyDirectory, "cgx_STATIC.exe");

        public static string beso = Path.Combine(AssemblyDirectory, "beso");

        public static string beso_cfg = beso + Path.DirectorySeparatorChar + "beso_conf.py";

        public static string beso_result_format = "file*_state1.inp";

        public static string cgx_cfg_beso_org = Path.Combine(AssemblyDirectory, "cfg-beso.fbd");
        public static string cgx_cfg_beso_new = Path.GetTempPath() + "cfg-beso.fbd";
    }
}
