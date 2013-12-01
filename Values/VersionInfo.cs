namespace kOS.Values
{
    public class VersionInfo : SpecialValue
    {
        public double Major;
        public double Minor;

        public VersionInfo(double major, double minor)
        {
            Major = major;
            Minor = minor;
        }

        public override object GetSuffix(string suffixName)
        {
            switch (suffixName)
            {
                case "MAJOR":
                    return Major;
                case "MINOR":
                    return Minor;
                default:
                    return null;
            }
        }

        public override string ToString()
        {
            return Major.ToString() + "." + Minor.ToString("0.0");
        }
    }
}
