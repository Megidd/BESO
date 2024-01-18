using System;
using System.IO;
using System.Reflection;

namespace BESO
{
    internal class Paths
    {
        public Paths()
        {
            tmpDir = CreateTempSubdirectory() + Path.DirectorySeparatorChar;
        }
        private string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public string tmpDir;

        // https://stackoverflow.com/a/278457/3405291
        private string CreateTempSubdirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            if (File.Exists(tempDirectory))
            {
                return CreateTempSubdirectory();
            }
            else
            {
                Directory.CreateDirectory(tempDirectory);
                return tempDirectory;
            }
        }

        // Input object to be saved as STL.
        // Material props are all based on mm, so STL unit would be converted to mm.
        public string stl { get { return tmpDir + "input.stl"; } }

        public string load { get { return tmpDir + "load-points.json"; } }
        public string restraint { get { return tmpDir + "restraint-points.json"; } }

        public string resultName { get { return "result.inp"; } }
        public string result { get { return tmpDir + resultName; } }

        // Sometimes, must write to Python config file in this format:
        // "C:\\Users\\m3\\AppData\\Local\\Temp\\result.inp"
        // Not this format:
        // "C:\Users\m3\AppData\Local\Temp\result.inp"
        public string escape(string pth)
        {

            Char psep = Path.DirectorySeparatorChar;
            string p = pth.Replace($@"{psep}", $@"{psep}{psep}");
            return p;
        }
        
        public string resultNoExt { get { return tmpDir + "result"; } }
        public string report { get { return tmpDir + "report.json"; } }

        public string specs { get { return tmpDir + "specs.json"; } }

        public string fe { get { return Path.Combine(AssemblyDirectory, "finite_elements.exe"); } }

        public string ccx { get { return Path.Combine(AssemblyDirectory, "ccx_static.exe"); } }

        public string cgx_cfg_fea_org { get { return Path.Combine(AssemblyDirectory, "cfg.fbd"); } }
        public string cgx_cfg_fea_new { get { return tmpDir + "cfg.fbd"; } }
        public string result_frd { get { return tmpDir + "result.frd"; } }

        public string cgx { get { return Path.Combine(AssemblyDirectory, "cgx_STATIC.exe"); } }

        public string beso { get { return Path.Combine(AssemblyDirectory, "beso"); } }

        public string beso_cfg { get { return beso + Path.DirectorySeparatorChar + "beso_conf.py"; } }

        public string beso_result_format { get { return "file*_state1.inp"; } }

        public string cgx_cfg_beso_org { get { return Path.Combine(AssemblyDirectory, "cfg-beso.fbd"); } }
        public string cgx_cfg_beso_new { get { return tmpDir + "cfg-beso.fbd"; } }
    }
}
