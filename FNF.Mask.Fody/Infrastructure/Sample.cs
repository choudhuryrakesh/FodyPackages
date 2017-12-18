using FNF.Mask.Fody.Maskers;
using System.Runtime.Serialization;

namespace FNF.Mask.Fody.Infrastructure
{
    /// <summary>
    /// This class is soley used to see how the compiled code is using IL viewer (ilDasm)
    /// </summary>
    public class Sample
    {
        public string SSN { get; set; }


        [DataMember(Name = "SSN")]
        public string SSNMask { get { return SSNMasker.Mask(SSN); } }

        public bool ShouldSerializeSSN()
        {
            return false;
        }
    }
}