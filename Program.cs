using osu_background_finder;

partial class Program{
    static void Main(string[] arg){
        Console.WriteLine("Enter target image file path:");
        string img_path = Console.ReadLine();

        Console.WriteLine("Enter osu beatmap directory:");
        string osu_path = Console.ReadLine();

        Finder f = new Finder(img_path, osu_path);
        f.ProcessDirectories();

        Console.WriteLine("Found "+f.Directory_count+" beatmaps, press Enter to start processing them");
        Console.ReadLine();

        f.Start();
        while(!f.Finished){ }
        f.Dispose();
        Console.ReadKey(true);
    }
}