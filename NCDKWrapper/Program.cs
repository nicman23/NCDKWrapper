using NCDK;
using NCDK.QSAR;
using System;
using System.Linq;
using System.Reflection;
using System.IO;

namespace NCDKWrapper
{
  class Program
  {
    /*
     args: 0) Category (i.e. Molecule) currently only molecules are supported
           1) Descriptor names (comma delimited)
           2) SDF file names (space delimited)
    */
    static int Main(string[] args)
    {
      try
      {
        if (args.Length < 3)
        {
          PrintHelp();
          return 1;
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
              Console.Write($"\"{mol.Title.ToString()}\"");
              for (int z = 0; z < descriptorsList.Length; z++)
              {
                try {
                  var res = descriptorInstance[z].Calculate(mol);
                  foreach (var item in res)
                  {
                    Console.Write($",\"{item.Value.ToString()}\"");
//                  Console.Write($",\"{item.Key}:{item.Value.ToString()}\"");
                  }
                } catch (Exception ex) {
                  using (TextWriter errorWriter = Console.Error) {
                    Console.Write(",\"\"");
                    errorWriter.WriteLine($"{ex}");
                  }
                }
              }
              Console.Write("\n");
            }
          }
        }
      } catch (Exception ex)
      {
        Console.WriteLine("Exception has occured: \n" + ex);
        return (int)2;
      }
      return (int)0;
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
