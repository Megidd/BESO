using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using System.IO;

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

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Input object to be saved as STL.
            string stlPth = Path.GetTempPath() + "input.stl";
            RhinoObject inObj = Helper.GetInputStl(doc.ModelUnitSystem, stlPth);
            if (inObj == null)
            {
                return Result.Failure;
            }
            return Result.Success;
        }
    }
}
