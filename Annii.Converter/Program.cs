namespace Annii.Converter;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length <= 1) {
            Console.WriteLine("Usage: Annii.Converter.exe <input file> <output file>");
            return;       
        }
        
        var outFileName = Path.GetFileNameWithoutExtension(args[0]);
        var r5a = R5A.LoadFromFile(args[0]);
        r5a.ExportToObj(Path.Join(args[0], $"{outFileName}.obj"));
        
    }
}