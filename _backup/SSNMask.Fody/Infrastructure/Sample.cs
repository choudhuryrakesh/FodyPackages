namespace SSNMask.Fody.Infrastructure
{
    /// <summary>
    /// This class is soley used to see how the compiled code is using IL viewer (ilDasm)
    /// </summary>
    internal class Sample
    {
        public string SSN { get; set; }

        public string SSNMask { get { return Masker.Mask(SSN); } }

        public bool ShouldSerializeSSN()
        {
            return false;
        }
    }
}