using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;

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
            return Result.Success;
        }
    }
}
