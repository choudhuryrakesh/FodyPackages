using FNF.Mask.Fody.Attributes;
using Newtonsoft.Json;

namespace WeavableAssembly.DTO
{
    public class Person
    {
        [MaskSSN]
        public string SSN { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}