﻿using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BESO
{
    public class BESOTopologyOptimization : Command
    {
        public BESOTopologyOptimization()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static BESOTopologyOptimization Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "BESOTopologyOptimization";

        // Includes all the temp folder and files.
        private Paths paths = null;

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Initialize here to have new temp folder for every command run.
            paths = new Paths();

            RhinoObject inObj = Helper.GetInputStl(doc.ModelUnitSystem, paths.stl);
            if (inObj == null)
            {
                return Result.Failure;
            }

            // Units of measurement:
            // https://engineering.stackexchange.com/q/54454/15178
            double MassDensity = 0; // (N*s2/mm4)
            double YoungModulus = 0; // MPa (N/mm2)
            double PoissonRatio = 0;

            uint materialProps = Helper.GetUint32FromUser("Enter material/metal type. Gold=1, Silver=2, Steel=3", 3, 1, 3);
            switch (materialProps)
            {
                case 1:
                    // Gold
                    MassDensity = 19.3e-9; // (N*s2/mm4) // 19.3 g/cm3
                    YoungModulus = 79000; // MPa (N/mm2) // 79 GPa
                    PoissonRatio = 0.4;
                    break;
                case 2:
                    // Silver
                    MassDensity = 10.49e-9; // (N*s2/mm4) // 10.49 g/cm3
                    YoungModulus = 83000; // MPa (N/mm2) // 83 GPa
                    PoissonRatio = 0.37;
                    break;
                case 3:
                    // Steel
                    MassDensity = 7.85e-9; // (N*s2/mm4) // 7.85 g/cm3
                    YoungModulus = 210000; // MPa (N/mm2) // 210 GPa
                    PoissonRatio = 0.3;
                    break;
                default:
                    RhinoApp.WriteLine("It's out of range");
                    return Result.Failure;
            }

            // Unit of measurement is Newton (N).
            Double loadMagnitude = 0;
            loadMagnitude = Helper.GetDoubleFromUser(100, 1, 1000, "Enter load magnitude in Newton (N)");

            // Resolution is voxel (3D pixel) count on longest axis of 3D model AABB.
            // NOTE: It will be further calibrated by the logic. Don't worry about it.
            uint resolution = 30;

            uint precision = Helper.GetUint32FromUser("Enter precision of computation. It highly affects the speed. VeryLow=1, Low=2, Medium=3, High=4, VeryHigh=5", 3, 1, 5);
            switch (precision)
            {
                case 1:
                    resolution = 30;
                    break;
                case 2:
                    resolution = 60;
                    break;
                case 3:
                    resolution = 90;
                    break;
                case 4:
                    resolution = 120;
                    break;
                case 5:
                    resolution = 150;
                    break;
                default:
                    RhinoApp.WriteLine("Precision must be 1, 2, 3, 4, or 5 i.e. VeryLow=1, Low=2, Medium=3, High=4, VeryHigh=5");
                    return Result.Failure;
            }

            List<Point3d> loadPoints = Helper.GetPointOnMesh(inObj, "Select load points on mesh (Esc/Enter to finish)");
            if (loadPoints == null || loadPoints.Count < 1)
            {
                RhinoApp.WriteLine("No points are selected");
                return Result.Failure;
            }

            List<Vector3d> loadNormals = new List<Vector3d>();
            Mesh inMesh = inObj.Geometry as Mesh;
            for (var i = 0; i < loadPoints.Count; i++)
            {
                MeshPoint mp = inMesh.ClosestMeshPoint(loadPoints[i], 0.0);
                Vector3d normal = inMesh.NormalAt(mp);
                loadNormals.Add(normal);
            }

            RhinoApp.WriteLine("Load/force points count: {0}", loadPoints.Count);

            List<Load> loads = new List<Load>();
            for (var i = 0; i < loadPoints.Count; i++)
            {
                Load load = new Load();
                load.LocX = loadPoints[i].X;
                load.LocY = loadPoints[i].Y;
                load.LocZ = loadPoints[i].Z;
                bool good = loadNormals[i].Unitize();
                if (!good) RhinoApp.WriteLine("Warning: cannot normalize the load direction: {0}", loadNormals[i]);
                load.MagX = loadNormals[i].X * loadMagnitude;
                load.MagY = loadNormals[i].Y * loadMagnitude;
                load.MagZ = loadNormals[i].Z * loadMagnitude;
                loads.Add(load);
            }

            List<Point3d> restraintPoints = Helper.GetPointOnMesh(inObj, "Select restraint points on mesh (Esc/Enter to finish)");
            if (restraintPoints == null || restraintPoints.Count < 1)
            {
                RhinoApp.WriteLine("No points are selected");
                return Result.Failure;
            }

            RhinoApp.WriteLine("Restraint points count: {0}", restraintPoints.Count);

            List<Restraint> restraints = new List<Restraint>();
            for (var i = 0; i < restraintPoints.Count; i++)
            {
                Restraint restraint = new Restraint();
                restraint.LocX = restraintPoints[i].X;
                restraint.LocY = restraintPoints[i].Y;
                restraint.LocZ = restraintPoints[i].Z;
                restraint.IsFixedX = true;
                restraint.IsFixedY = true;
                restraint.IsFixedZ = true;
                restraints.Add(restraint);
            }

            string loadJson = JsonSerializer.Serialize(loads);
            File.WriteAllText(paths.load, loadJson);

            string restraintJson = JsonSerializer.Serialize(restraints);
            File.WriteAllText(paths.restraint, restraintJson);

            Dictionary<string, dynamic> specs = new Dictionary<string, dynamic>();
            specs.Add("PathResult", paths.result);
            specs.Add("PathReport", paths.report);
            specs.Add("PathStl", paths.stl);
            specs.Add("PathLoadPoints", paths.load);
            specs.Add("PathRestraintPoints", paths.restraint);
            specs.Add("MassDensity", MassDensity);
            specs.Add("YoungModulus", YoungModulus);
            specs.Add("PoissonRatio", PoissonRatio);
            specs.Add("GravityDirectionX", 0);
            specs.Add("GravityDirectionY", 0);
            specs.Add("GravityDirectionZ", -1);
            specs.Add("GravityMagnitude", Unit.Convert(9.810f, UnitSystem.Meters, Helper.unitOfStlFile));
            specs.Add("GravityIsNeeded", false); // Let's not consider gravity. Point loads should be dominant.
            specs.Add("Resolution", resolution);
            specs.Add("NonlinearConsidered", false);
            specs.Add("ExactSurfaceConsidered", true);
            specs.Add("ModelUnitSystem", doc.ModelUnitSystem.ToString());
            specs.Add("ModelUnitSystemOfSavedStlFile", Helper.unitOfStlFile.ToString());

            string specsJson = JsonSerializer.Serialize(specs);
            File.WriteAllText(paths.specs, specsJson);

            // Prepare arguments as text fields.
            string args = "";
            args += " ";
            args += paths.specs;

            RhinoApp.WriteLine("Working: {0}", paths.fe);

            if (File.Exists(paths.fe))
            {
                // Run external process
                // Generate finite elements required by FEA.
                Helper.RunLogicWithLog(paths.fe, args, runFEA);
            }
            else
            {
                RhinoApp.WriteLine("Doesn't exist: ", paths.fe);
                return Result.Failure;
            }

            return Result.Success;
        }

        private void runFEA(object sender, EventArgs e)
        {
            try
            {
                RhinoApp.WriteLine("Working: {0}", paths.ccx);

                if (File.Exists(paths.ccx))
                {
                    string args = "-i" + " " + paths.resultNoExt;
                    // Do FEA i.e. finite element analysis.
                    Helper.RunLogicWithLog(paths.ccx, args, displayFEA);
                }
                else
                {
                    RhinoApp.WriteLine("Doesn't exist: ", paths.ccx);
                }

            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error: {0}", ex.Message);
            }
        }

        private void displayFEA(object sender, EventArgs e)
        {
            try
            {
                // Create a CGX config file with the correct FRD file name.
                string text = File.ReadAllText(paths.cgx_cfg_fea_org);
                text = text.Replace("result.frd", paths.result_frd);
                File.WriteAllText(paths.cgx_cfg_fea_new, text);
                string args = "-b" + " " + paths.cgx_cfg_fea_new;

                RhinoApp.WriteLine("Working: {0}", paths.cgx);

                if (File.Exists(paths.cgx))
                {
                    // Run external process
                    // Visualize FEA result.
                    Helper.RunLogic(paths.cgx, args, runBESO);
                }
                else
                {
                    RhinoApp.WriteLine("Doesn't exist: ", paths.cgx);
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error: {0}", ex.Message);
            }
        }

        private void runBESO(object sender, EventArgs e)
        {
            try
            {
                RhinoApp.WriteLine("Working: {0}", paths.beso);

                // Every command run has its own temp directory.
                // So, no need to delete files of previous run.

                Helper.ReplaceLineInFile(
                    paths.beso_cfg,
                    "path =",
                    "path = \"" + paths.escape(paths.tmpDir) + "" + "\"  # path to the working directory (without whitespaces) where the initial file is located"
                    );
                // Modify `beso_conf.py` pointing to the correct `file_name` of INP.
                Helper.ReplaceLineInFile(
                    paths.beso_cfg,
                    "file_name =",
                    "file_name = \"" + paths.resultName + "\" # file with prepared linear static analysis"
                    );
                // Point `beso_conf.py` to CCX executable.
                Helper.ReplaceLineInFile(
                    paths.beso_cfg,
                    "path_calculix =",
                    "path_calculix = \"" + paths.escape(paths.ccx) + "\" # path to the CalculiX solver"
                    );
                // Limit cpu cores to avoid freezing the device.
                Helper.ReplaceLineInFile(
                    paths.beso_cfg,
                    "cpu_cores =",
                    "cpu_cores = 1  # 0 - use all processor cores, N - will use N number of processor cores"
                    );
                Helper.RunLogicBESO(paths.beso, displayBESO);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error: {0}", ex.Message);
            }
        }

        private void displayBESO(object sender, EventArgs e)
        {
            try
            {
                RhinoApp.WriteLine("Working: {0}", paths.tmpDir);
                // Display the last file.
                string lastFileName = Helper.GetLastFileName(paths.tmpDir, paths.beso_result_format);
                // Create a CGX config file with the correct file name.
                string text = File.ReadAllText(paths.cgx_cfg_beso_org);
                text = text.Replace("file?_state1.inp", paths.tmpDir + Path.DirectorySeparatorChar + lastFileName);
                File.WriteAllText(paths.cgx_cfg_beso_new, text);
                string args = "-b" + " " + paths.cgx_cfg_beso_new;

                RhinoApp.WriteLine("Working: {0}", paths.cgx);

                if (File.Exists(paths.cgx))
                {
                    // Visualize BESO result.
                    Helper.RunLogic(paths.cgx, args, done);
                }
                else
                {
                    RhinoApp.WriteLine("Doesn't exist: ", paths.cgx);
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error: {0}", ex.Message);
            }
        }

        private static void done(object sender, EventArgs e)
        {
            try
            {
                RhinoApp.WriteLine("BESO done.");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error: {0}", ex.Message);
            }
        }
    }
}
