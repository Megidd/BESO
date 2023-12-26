using Rhino;
using System;

namespace BESO
{
    internal class Unit
    {
        public static float Convert(float f, UnitSystem unitI, UnitSystem unitO)
        {
            // Convert from unitI to meters
            float inMeters = f * GetUnitFactor(unitI);

            // Convert from meters to unitO
            float finalValue = inMeters / GetUnitFactor(unitO);

            return finalValue;
        }

        private static float GetUnitFactor(UnitSystem unit)
        {
            switch (unit)
            {
                case UnitSystem.None:
                    return 1;
                case UnitSystem.Angstroms:
                    return 1.0e-10f;
                case UnitSystem.Nanometers:
                    return 1.0e-9f;
                case UnitSystem.Microns:
                    return 1.0e-6f;
                case UnitSystem.Millimeters:
                    return 1.0e-3f;
                case UnitSystem.Centimeters:
                    return 1.0e-2f;
                case UnitSystem.Decimeters:
                    return 1.0e-1f;
                case UnitSystem.Meters:
                    return 1;
                case UnitSystem.Dekameters:
                    return 10;
                case UnitSystem.Hectometers:
                    return 100;
                case UnitSystem.Kilometers:
                    return 1.0e+3f;
                case UnitSystem.Megameters:
                    return 1.0e+6f;
                case UnitSystem.Gigameters:
                    return 1.0e+9f;
                case UnitSystem.Microinches:
                    return 2.54e-8f;
                case UnitSystem.Mils:
                    return 2.54e-5f;
                case UnitSystem.Inches:
                    return 0.0254f;
                case UnitSystem.Feet:
                    return 0.3048f;
                case UnitSystem.Yards:
                    return 0.9144f;
                case UnitSystem.Miles:
                    return 1609.344f;
                case UnitSystem.PrinterPoints:
                    return 0.0254f / 72;
                case UnitSystem.PrinterPicas:
                    return 0.0254f / 6;
                case UnitSystem.NauticalMiles:
                    return 1852;
                case UnitSystem.AstronomicalUnits:
                    return 1.4959787e+11f;
                case UnitSystem.LightYears:
                    return 9.4607304725808e+15f;
                case UnitSystem.Parsecs:
                    return 3.08567758e+16f;
                case UnitSystem.CustomUnits:
                    throw new Exception("Custom units not supported");
                case UnitSystem.Unset:
                    throw new Exception("Unit system is unset");
                default:
                    throw new Exception("Unknown unit system");
            }
        }
    }
}
