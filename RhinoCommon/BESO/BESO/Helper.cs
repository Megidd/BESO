using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BESO
{
    internal class Helper
    {
        // Any STL file will be saved by this unit of measurement.
        public static UnitSystem unitOfStlFile = UnitSystem.Millimeters;

        public static RhinoObject GetInputStl(UnitSystem unit, string filename = "mesh.stl")
        {
            RhinoObject obj = GetSingle();
            if (null == obj || obj.ObjectType != ObjectType.Mesh)
            {
                RhinoApp.WriteLine("Mesh is not valid.");
                return null;
            }
            Mesh mesh = obj.Geometry as Mesh;
            if (mesh == null)
            {
                RhinoApp.WriteLine("Mesh is not valid.");
                return null;
            }

            SaveAsStl(unit, mesh, filename);

            return obj;
        }

        /// <summary>
        /// A simple method to prompt the user to select a single mesh.
        /// </summary>
        /// <returns>Selected object that should be of type mesh.</returns>
        public static RhinoObject GetSingle(String message = "Select a single mesh")
        {
            GetObject go = new GetObject();
            go.SetCommandPrompt(message);
            go.GeometryFilter = ObjectType.Mesh;
            go.Get();
            if (go.CommandResult() != Result.Success) return null;
            if (go.ObjectCount != 1) return null;
            RhinoObject obj = go.Object(0).Object();
            if (obj.ObjectType != ObjectType.Mesh) return null;
            return obj;
        }

        public static List<Point3d> GetPointOnMesh(RhinoObject obj, String message = "Select points on mesh (Esc to cancel)")
        {
            // Validate input
            if (obj == null || obj.ObjectType != ObjectType.Mesh)
            {
                RhinoApp.WriteLine("Invalid mesh object.");
                return null;
            }

            Mesh mesh = obj.Geometry as Mesh;
            if (mesh == null)
            {
                RhinoApp.WriteLine("Invalid mesh object.");
                return null;
            }

            List<Point3d> points = new List<Point3d>();

            while (true)
            {
                GetPoint gp = new GetPoint();
                gp.SetCommandPrompt(message);
                gp.Constrain(mesh, false);
                gp.Get();

                if (gp.CommandResult() != Result.Success)
                    break; // User cancelled.

                var pickedPoint = gp.Point();
                if (pickedPoint == null || pickedPoint == Point3d.Unset)
                {
                    RhinoApp.WriteLine("Couldn't get a point on mesh.");
                    continue;
                }

                //RhinoApp.WriteLine("Picked point: {0}", pickedPoint);
                points.Add(pickedPoint);
            }

            return points;
        }

        public static void SaveAsStl(UnitSystem unit, Mesh mesh, string fileName)
        {
            // Extract vertex buffer and index buffer.
            float[] vertexBuffer;
            int[] indexBuffer;
            GetBuffers(mesh, out vertexBuffer, out indexBuffer);

            SaveBuffersAsStl(unit, vertexBuffer, indexBuffer, fileName);
        }

        public static void GetBuffers(Mesh mesh, out float[] vertexBuffer, out int[] indexBuffer)
        {
            // Convert quads to triangles
            bool converted = mesh.Faces.ConvertQuadsToTriangles();
            if (!converted)
            {
                RhinoApp.WriteLine("Mesh quads couldn't be converted to triangles.");
            }

            // Get vertex buffer
            Point3f[] vertices = mesh.Vertices.ToPoint3fArray();
            vertexBuffer = new float[vertices.Length * 3];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertexBuffer[i * 3] = vertices[i].X;
                vertexBuffer[i * 3 + 1] = vertices[i].Y;
                vertexBuffer[i * 3 + 2] = vertices[i].Z;
            }

            // Get index buffer
            MeshFace[] faces = mesh.Faces.ToArray();
            indexBuffer = new int[faces.Length * 3];
            for (int i = 0; i < faces.Length; i++)
            {
                MeshFace face = faces[i];
                indexBuffer[i * 3] = face.A;
                indexBuffer[i * 3 + 1] = face.B;
                indexBuffer[i * 3 + 2] = face.C;
            }
        }

        public static void SaveBuffersAsStl(UnitSystem unit, float[] vertexBuffer, int[] indexBuffer, string fileName)
        {
            // Open the file for writing
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                // Write the STL header
                byte[] header = new byte[80];
                fileStream.Write(header, 0, header.Length);

                // Write the number of triangles
                int triangleCount = indexBuffer.Length / 3;
                byte[] triangleCountBytes = BitConverter.GetBytes(triangleCount);
                fileStream.Write(triangleCountBytes, 0, 4);

                // Write the triangles
                for (int i = 0; i < indexBuffer.Length; i += 3)
                {
                    // Get vertices for the current triangle
                    float x1 = vertexBuffer[indexBuffer[i] * 3];
                    float y1 = vertexBuffer[indexBuffer[i] * 3 + 1];
                    float z1 = vertexBuffer[indexBuffer[i] * 3 + 2];
                    float x2 = vertexBuffer[indexBuffer[i + 1] * 3];
                    float y2 = vertexBuffer[indexBuffer[i + 1] * 3 + 1];
                    float z2 = vertexBuffer[indexBuffer[i + 1] * 3 + 2];
                    float x3 = vertexBuffer[indexBuffer[i + 2] * 3];
                    float y3 = vertexBuffer[indexBuffer[i + 2] * 3 + 1];
                    float z3 = vertexBuffer[indexBuffer[i + 2] * 3 + 2];

                    if (unit != Helper.unitOfStlFile)
                    {
                        x1 = Unit.Convert(x1, unit, Helper.unitOfStlFile);
                        y1 = Unit.Convert(y1, unit, Helper.unitOfStlFile);
                        z1 = Unit.Convert(z1, unit, Helper.unitOfStlFile);
                        x2 = Unit.Convert(x2, unit, Helper.unitOfStlFile);
                        y2 = Unit.Convert(y2, unit, Helper.unitOfStlFile);
                        z2 = Unit.Convert(z2, unit, Helper.unitOfStlFile);
                        x3 = Unit.Convert(x3, unit, Helper.unitOfStlFile);
                        y3 = Unit.Convert(y3, unit, Helper.unitOfStlFile);
                        z3 = Unit.Convert(z3, unit, Helper.unitOfStlFile);
                    }

                    // Compute the normal vector of the triangle
                    float nx = (y2 - y1) * (z3 - z1) - (z2 - z1) * (y3 - y1);
                    float ny = (z2 - z1) * (x3 - x1) - (x2 - x1) * (z3 - z1);
                    float nz = (x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1);
                    float length = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    nx /= length;
                    ny /= length;
                    nz /= length;

                    // Write the normal vector
                    byte[] normal = new byte[12];
                    BitConverter.GetBytes(nx).CopyTo(normal, 0);
                    BitConverter.GetBytes(ny).CopyTo(normal, 4);
                    BitConverter.GetBytes(nz).CopyTo(normal, 8);
                    fileStream.Write(normal, 0, normal.Length);

                    // Write the vertices in counter-clockwise order
                    byte[] triangle = new byte[36];
                    BitConverter.GetBytes(x1).CopyTo(triangle, 12);
                    BitConverter.GetBytes(y1).CopyTo(triangle, 16);
                    BitConverter.GetBytes(z1).CopyTo(triangle, 20);
                    BitConverter.GetBytes(x3).CopyTo(triangle, 0);
                    BitConverter.GetBytes(y3).CopyTo(triangle, 4);
                    BitConverter.GetBytes(z3).CopyTo(triangle, 8);
                    BitConverter.GetBytes(x2).CopyTo(triangle, 24);
                    BitConverter.GetBytes(y2).CopyTo(triangle, 28);
                    BitConverter.GetBytes(z2).CopyTo(triangle, 32);
                    fileStream.Write(triangle, 0, triangle.Length);

                    // Write the triangle attribute (zero)
                    byte[] attribute = new byte[2];
                    fileStream.Write(attribute, 0, attribute.Length);
                }
            }
        }

        public static float GetFloatFromUser(double defaultValue, double lowerLimit, double upperLimit, string message)
        {
            // Create a GetNumber object
            GetNumber numberGetter = new GetNumber();
            numberGetter.SetLowerLimit(lowerLimit, false);
            numberGetter.SetUpperLimit(upperLimit, false);
            numberGetter.SetDefaultNumber(defaultValue);
            numberGetter.SetCommandPrompt(message);

            // Prompt the user to enter a number
            GetResult result = numberGetter.Get();

            // Check if the user entered a number
            switch (result)
            {
                case GetResult.Number:
                    break;
                default:
                    return Convert.ToSingle(defaultValue);
            }

            // Get the number entered by the user
            double number = numberGetter.Number();

            return Convert.ToSingle(number);
        }


        public static uint GetUint32FromUser(string prompt, uint defaultValue, uint lowerLimit, uint upperLimit)
        {
            double doubleResult = defaultValue;
            uint result = defaultValue;
            while (true)
            {
                var getNumberResult = RhinoGet.GetNumber(prompt, false, ref doubleResult, lowerLimit, upperLimit);
                if (getNumberResult == Result.Cancel)
                {
                    RhinoApp.WriteLine("Canceled by user.");
                    return defaultValue;
                }
                else if (getNumberResult == Result.Success)
                {
                    result = (uint)doubleResult;
                    if (result < lowerLimit || result > upperLimit)
                        RhinoApp.WriteLine("Input out of range.");
                    else
                        return result;
                }
                else
                    RhinoApp.WriteLine("Invalid input.");
            }
        }

        public static bool GetYesNoFromUser(string prompt)
        {
            bool boolResult = false;
            while (true)
            {
                var getBoolResult = RhinoGet.GetBool(prompt, true, "No", "Yes", ref boolResult);
                if (getBoolResult == Result.Cancel)
                    RhinoApp.WriteLine("Canceled by user.");
                else if (getBoolResult == Result.Success)
                    return boolResult;
                else
                    RhinoApp.WriteLine("Invalid input.");
            }
        }

        public static int RunLogicWithLogAndWait(string exePath, string args)
        {
            cmd = new Process();
            try
            {
                cmd.StartInfo.FileName = exePath;
                cmd.StartInfo.Arguments = args;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.RedirectStandardError = true;
                cmd.StartInfo.RedirectStandardInput = true;

                cmd.EnableRaisingEvents = true;
                cmd.OutputDataReceived += new DataReceivedEventHandler(cmd_LogReceived);
                cmd.ErrorDataReceived += new DataReceivedEventHandler(cmd_LogReceived);

                cmd.Start();

                // Begin asynchronous log.
                cmd.BeginOutputReadLine();
                cmd.BeginErrorReadLine();

                // https://stackoverflow.com/a/4251752/3405291
                cmd.WaitForExit();
                return cmd.ExitCode;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error on process run: {0}", ex.Message);
                return -2;
            }
        }

        private static Process cmd;
        public delegate void PostProcess(object sender, EventArgs e);
        public static void RunLogic(string exePath, string args, PostProcess pp)
        {
            cmd = new Process();

            try
            {
                cmd.StartInfo.FileName = exePath;
                cmd.StartInfo.Arguments = args;
                cmd.StartInfo.UseShellExecute = true;
                cmd.StartInfo.CreateNoWindow = false;
                cmd.StartInfo.RedirectStandardOutput = false;
                cmd.StartInfo.RedirectStandardError = false;
                cmd.StartInfo.RedirectStandardInput = false;
                // Vista or higher check.
                // https://stackoverflow.com/a/2532775/3405291
                // Rhino 7 requires Windows 11, 10 or 8.1 so we are good :)
                // https://www.rhino3d.com/7/system-requirements/
                if (System.Environment.OSVersion.Version.Major >= 6)
                {
                    // Run with admin privileges to avoid non-responsive CGX window.
                    cmd.StartInfo.Verb = "runas";
                }
                cmd.EnableRaisingEvents = true;
                cmd.Exited += new EventHandler(cmd_Exited);
                cmd.Exited += new EventHandler(pp);

                cmd.Start();
            }

            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error on process start: {0}", ex.Message);
            }
        }

        public static void RunLogicBESO(string path, PostProcess pp)
        {
            cmd = new Process();

            try
            {
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.Arguments = "";
                cmd.StartInfo.UseShellExecute = false; // To be able to activate Python env.
                cmd.StartInfo.CreateNoWindow = false;
                cmd.StartInfo.RedirectStandardOutput = false;
                cmd.StartInfo.RedirectStandardError = false;
                cmd.StartInfo.RedirectStandardInput = true; // To be able to write line.
                // Vista or higher check.
                // https://stackoverflow.com/a/2532775/3405291
                // Rhino 7 requires Windows 11, 10 or 8.1 so we are good :)
                // https://www.rhino3d.com/7/system-requirements/
                if (System.Environment.OSVersion.Version.Major >= 6)
                {
                    // Run with admin privileges to avoid non-responsive CGX window.
                    cmd.StartInfo.Verb = "runas";
                }
                cmd.EnableRaisingEvents = true;
                cmd.Exited += new EventHandler(cmd_Exited);
                cmd.Exited += new EventHandler(pp);

                cmd.Start();
                cmd.StandardInput.WriteLine(String.Format("cd {0}", path));
                cmd.StandardInput.WriteLine("virtual_env\\Scripts\\activate.bat");
                // Python virtual env already has the numpy and matplotlib.
                cmd.StandardInput.WriteLine("python beso_main.py");
                // Have to exit to jump to the post-process.
                cmd.StandardInput.WriteLine("exit");
            }

            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error on process start: {0}", ex.Message);
            }
        }

        private static void cmd_Exited(object sender, EventArgs e)
        {
            try
            {
                RhinoApp.WriteLine("Process finished.");
                cmd.Dispose();
            }

            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error on process exit: {0}", ex.Message);
            }
        }

        public static void RunLogicWithLog(string exePath, string args, PostProcess pp)
        {
            cmd = new Process();

            try
            {
                cmd.StartInfo.FileName = exePath;
                cmd.StartInfo.Arguments = args;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.RedirectStandardError = true;
                cmd.StartInfo.RedirectStandardInput = true;

                cmd.EnableRaisingEvents = true;
                cmd.OutputDataReceived += new DataReceivedEventHandler(cmd_LogReceived);
                cmd.ErrorDataReceived += new DataReceivedEventHandler(cmd_LogReceived);
                cmd.Exited += new EventHandler(cmd_Exited);
                cmd.Exited += new EventHandler(pp);

                cmd.Start();

                // Begin asynchronous log.
                cmd.BeginOutputReadLine();
                cmd.BeginErrorReadLine();
            }

            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error on process start: {0}", ex.Message);
            }
        }

        private static void cmd_LogReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    // The external process logs are usually line-by-line.
                    // So, don't worry about the new line character.
                    RhinoApp.WriteLine("Process log: {0}", e.Data);
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error on process log: {0}", ex.Message);
            }
        }

        public static Mesh LoadStlAsMesh(string fileName)
        {
            Mesh mesh = new Mesh();

            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                // Skip the header
                reader.ReadBytes(80);

                // Read the number of triangles
                uint numTriangles = reader.ReadUInt32();

                // Read each triangle
                for (int i = 0; i < numTriangles; i++)
                {
                    // Read the normal (we don't use it here, but it's part of the STL format)
                    reader.ReadBytes(12);

                    // Read and add the vertices
                    Point3f[] vertices = new Point3f[3];
                    for (int j = 0; j < 3; j++)
                    {
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        float z = reader.ReadSingle();
                        vertices[j] = new Point3f(x, y, z);
                    }

                    // Add the vertices and the face to the mesh
                    int vertexIndex = mesh.Vertices.Count;
                    mesh.Vertices.AddVertices(vertices);
                    mesh.Faces.AddFace(new MeshFace(vertexIndex, vertexIndex + 1, vertexIndex + 2));

                    // Skip the attribute byte count
                    reader.ReadBytes(2);
                }
            }

            // Recompute normals for the mesh
            mesh.Normals.ComputeNormals();

            // Compact the mesh to improve efficiency
            mesh.Compact();

            return mesh;
        }


        public static bool HasInvalidVertexIndices(Mesh mesh)
        {
            // Check each face in the mesh
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                MeshFace face = mesh.Faces[i];

                // Check each vertex index in the face
                if (face.A < 0 || face.A >= mesh.Vertices.Count ||
                    face.B < 0 || face.B >= mesh.Vertices.Count ||
                    face.C < 0 || face.C >= mesh.Vertices.Count)
                {
                    // If any vertex index is invalid, return true
                    return true;
                }
            }

            // If all face vertex indices are valid, return false
            return false;
        }

        public static double GetDoubleFromUser(double defaultValue, double lowerLimit, double upperLimit, string message)
        {
            // Create a GetNumber object
            GetNumber gn = new GetNumber();
            gn.SetLowerLimit(lowerLimit, false);
            gn.SetUpperLimit(upperLimit, false);
            gn.SetDefaultNumber(defaultValue);
            gn.SetCommandPrompt(message);

            // Prompt the user to enter a number
            GetResult result = gn.Get();

            // Check if the user entered a number
            switch (result)
            {
                case GetResult.Number:
                    break;
                default:
                    return defaultValue;
            }

            return gn.Number();
        }

        public static string GetLastFileName(string directoryPath, string searchPattern)
        {
            string[] files = Directory.GetFiles(directoryPath, searchPattern);
            Array.Sort(files);

            if (files.Length > 0)
            {
                string lastFileName = Path.GetFileName(files[files.Length - 1]);
                return lastFileName;
            }
            else
            {
                throw new FileNotFoundException("No files found in the directory.");
            }
        }

        public static void ReplaceLineInFile(string filePath, string lineToReplace, string newLine)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                int lineIndex = Array.FindIndex(lines, line => line.Contains(lineToReplace));

                if (lineIndex >= 0)
                {
                    // Replace the line with the new line
                    lines[lineIndex] = newLine;

                    File.WriteAllLines(filePath, lines);
                }
                else
                {
                    RhinoApp.WriteLine("Line not found in the file.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine(ex.Message);
            }
        }

        public static void DeleteFilesByPattern(string directoryPath, string searchPattern)
        {
            try
            {
                string[] files = Directory.GetFiles(directoryPath, searchPattern);

                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine(ex.Message);
            }
        }
    }

    public class Restraint
    {
        public double LocX { get; set; }
        public double LocY { get; set; }
        public double LocZ { get; set; }
        public bool IsFixedX { get; set; }
        public bool IsFixedY { get; set; }
        public bool IsFixedZ { get; set; }
    }

    public class Load
    {
        public double LocX { get; set; }
        public double LocY { get; set; }
        public double LocZ { get; set; }
        public double MagX { get; set; }
        public double MagY { get; set; }
        public double MagZ { get; set; }
    }
}
