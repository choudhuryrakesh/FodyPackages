using FNF.Mask.Fody.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using WeavableAssembly.DTO;

namespace WeavableAssembly
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string seperator = "***************";
            try
            {
                var ssn = "12A4-567Q-45TF";
                var test = new Person
                {
                    SSN = ssn,
                    Name = "James Bond",
                };

                //var test = new Sample
                //{
                //    SSN = ssn,
                //};

                var newtonSoft = JsonConvert.SerializeObject(test);
                Console.WriteLine($"{ seperator}{ Environment.NewLine}Newtonsoft:{ Environment.NewLine}{ newtonSoft}");

                Console.WriteLine("Input Json to deserialize:");
                var toDeserialize = Console.ReadLine();

                var testDeserialized = JsonConvert.DeserializeObject<Person>(toDeserialize);
                Console.WriteLine($"{ seperator}{ Environment.NewLine}Newtonsoft:{ Environment.NewLine}{ testDeserialized.SSN}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Console.WriteLine(ex);
            }

            Console.ReadLine();
        }
    }
}