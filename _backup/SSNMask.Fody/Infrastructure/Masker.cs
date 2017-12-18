namespace SSNMask.Fody.Infrastructure
{
    public class Masker
    {
        public static string Mask(string ssn)
        {
            return "XXX-XXX-1234";
        }
    }

    internal static class StringExtensions
    {
        internal static string MaskSSN(this string ssn)
        {
            return Masker.Mask(ssn);
        }
    }
}