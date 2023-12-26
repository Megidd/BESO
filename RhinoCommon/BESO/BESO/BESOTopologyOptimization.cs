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
            // Material props are all based on mm, so STL unit would be converted to mm.
            string stlPth = Path.GetTempPath() + "input.stl";
            RhinoObject inObj = Helper.GetInputStl(doc.ModelUnitSystem, stlPth);
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

            return Result.Success;
        }
    }
}
