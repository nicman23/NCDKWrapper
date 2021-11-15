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
                    return;
                }
                var descriptorsList = args[1].Split(',');
                NCDK.QSAR.IMolecularDescriptor[] descriptorInstance = new NCDK.QSAR.IMolecularDescriptor[descriptorsList.Length];
                // var descriptorInstance = new List<object>();
                for (int i = 0; i < descriptorsList.Length; i++)
                {
                    var str = $"NCDK.QSAR.Descriptors.{args[0]}.{descriptorsList[i]}";
                    var descriptorType = Type.GetType($"NCDK.QSAR.Descriptors.{args[0]}.{descriptorsList[i]}, NCDK");
                    var paramCount = descriptorType.GetConstructors()[0].GetParameters().Length;
                    descriptorInstance[i] = (IMolecularDescriptor)descriptorType.GetConstructors()[0].Invoke(new object[paramCount]);
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
                            for (int z = 0; z < descriptorsList.Length; z++)
                            {
                                var res = descriptorInstance[z].Calculate(mol);
                                foreach (var item in res)
                                {
																		if (z == 0) {
	                                    Console.Write($"\"{item.Value.ToString()}\"");
																		}
                                    Console.Write($",\"{item.Value.ToString()}\"");
//                                  Console.Write($",\"{item.Key.ToString()}\"");
//                                  Console.Write($",\"{item.Key}:{item.Value.ToString()}\"");
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
                if (type.Name == "ChiIndexUtils")
                {
                    continue;
                }
                Console.WriteLine(type.Name);
            }
        }
    }
}
