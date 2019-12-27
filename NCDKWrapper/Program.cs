using NCDK;
using NCDK.QSAR;
using System;
using System.Linq;
using System.Reflection;

namespace NCDKWrapper
{
    class Program
    {
        /*
         args: 0) Category (i.e. Molecule) currently only molecules are supported
               1) Descriptor names (comma delimited)
               2) SDF file names (space delimited)
        */
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 3)
                {
                    PrintHelp();
                }

                for (int i = 2; i < args.Length; i++)
                {
                    using (var suppl = Chem.SDMolSupplier(args[i]))
                    {
                        foreach (var mol in suppl)
                        {
                            if (mol == null)
                            {
                                continue;
                            }

                            foreach (var descriptorName in args[1].Split(','))
                            {
                                var str = $"NCDK.QSAR.Descriptors.{args[0]}.{descriptorName}";

                                var descriptorType = Type.GetType($"NCDK.QSAR.Descriptors.{args[0]}.{descriptorName}, NCDK");

                                var paramCount = descriptorType.GetConstructors()[0].GetParameters().Length;

                                var descriptorInstance = (IMolecularDescriptor)descriptorType.GetConstructors()[0].Invoke(new object[paramCount]);

                                var result = descriptorInstance.Calculate(mol);

                                foreach (var item in result)
                                {
                                    Console.Write($"{item.Key}:{item.Value.ToString()} ");
                                }
                            }
                            Console.Write("\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception has occured: \n" + ex);
            }
        }

        private static void PrintHelp()
        {
            Assembly ass = Assembly.Load("NCDK");

            foreach (var type in ass.GetTypes().Where(t => string.Equals(t.Namespace, "NCDK.QSAR.Descriptors.Moleculars", StringComparison.InvariantCultureIgnoreCase) && !t.IsNested))
            {
                Console.WriteLine(type.Name);
            }
        }
    }
}
