namespace kOS.Values
{
    public class OrbitInfo : SpecialValue
    {
        readonly Orbit orbitRef;

        public OrbitInfo(Orbit init)
        {
            orbitRef = init;
        }

        public override object GetSuffix(string suffixName)
        {
            switch (suffixName)
            {
                case "APOAPSIS":
                    return orbitRef.ApA;
                case "PERIAPSIS":
                    return orbitRef.PeA;
                case "BODY":
                    return orbitRef.referenceBody.name;
                default:
                    return null;
            }
        }

        public override string ToString()
        {
            return orbitRef != null ? (string) orbitRef.referenceBody.name : "";
        }
    }
}
