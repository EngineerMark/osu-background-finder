using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections.Concurrent;

namespace osu_background_finder
{
    public class Finder
    {
        private string image_path;
        private string beatmap_directory;

        /// <summary>
        /// Amount of beatmaps
        /// </summary>
        private int directory_count;
        private string[] directories;

        public ConcurrentBag<FinderResult> results;

        public int ThreadCount = 10;
        private List<Thread> threads;

        public bool Finished = false;
        public int ThreadsFinished = 0;
        private int MapsProcessed = 0;
        public Vector originalResolution;
        private Bitmap checking_image;

        public Finder(string image_path, string beatmap_directory)
        {
            this.image_path = image_path;
            this.beatmap_directory = beatmap_directory;
        }

        public int Directory_count { get => directory_count; set => directory_count = value; }
        public string[] Directories { get => directories; set => directories = value; }

        public bool IsFinished(){
            return ThreadsFinished == ThreadCount;
        }

        public void ProcessDirectories()
        {
            string[] paths = Directory.GetDirectories(this.beatmap_directory);
            Directories = paths;
            Directory_count = paths.Length;
        }

        public void Start()
        {
            results = new ConcurrentBag<FinderResult>();


            // BitmapImage checking_image = 0;
            checking_image = new Bitmap(image_path);
            if (checking_image == null)
            {
                Console.WriteLine("Image file could not be made into a Bitmap, are you sure you picked the correct one?");
                return;
            }

            int iterationRange = (int)Math.Ceiling((double)directory_count / ThreadCount);
            threads = new List<Thread>();
            originalResolution = new Vector(checking_image.Width, checking_image.Height);
            for (int i = 0; i < ThreadCount; i++)
            {
                Bitmap clone = checking_image.Clone(new Rectangle(0, 0, checking_image.Width, checking_image.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                int start = iterationRange * i;
                int end = iterationRange * i + iterationRange;
                Thread t = new Thread(() => ProcessRange(clone, start, end));
                threads.Add(t);
                t.Start();
            }
        }

        public void Dispose()
        {
            checking_image.Dispose();
        }

        private void ProcessRange(Bitmap original, int start, int end)
        {
            var image_filter = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" };
            Random random = new Random();

            for (int i = start; i < end; i++)
            {
                if (i >= Directory_count)
                    break;

                string beatmap_dir = directories[i];
                Console.WriteLine(MapsProcessed + "/" + directory_count + ": Checking " + beatmap_dir);

                string[] files = GetFilesFrom(beatmap_dir, image_filter, false);

                if (files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        FileInfo _file = new FileInfo(file);

                        if (_file.Length < 100000)
                            continue;

                        Bitmap image = null;
                        try
                        {
                            image = new Bitmap(file);
                        }catch(Exception){
                            continue;
                        }

                        if (image == null)
                            continue;

                        if (image.Width < 300 || image.Height < 300)
                        {
                            image.Dispose();
                            continue;
                        }

                        double similarity = 0;
                        //we dont want to check every single pixel, so we generate a couple of random points on 0-1 scale and check those
                        for (int j = 0; j < 100; j++)
                        {
                            Vector point = new Vector(random.NextDouble(), random.NextDouble());

                            Vector pointOnLocal = point * new Vector(image.Width, image.Height);
                            Vector pointOnGlobal = point * new Vector(originalResolution.x, originalResolution.y);

                            Color localColor = image.GetPixel((int)pointOnLocal.x, (int)pointOnLocal.y);
                            Color globalColor = original.GetPixel((int)pointOnGlobal.x, (int)pointOnGlobal.y);

                            int diff_r = Math.Abs(localColor.R - globalColor.R);
                            int diff_g = Math.Abs(localColor.G - globalColor.G);
                            int diff_b = Math.Abs(localColor.B - globalColor.B);
                            double diff = (diff_r/255.0 + diff_g/255.0 + diff_b/255.0)/3;


                            if (diff <= 0.2)
                            {
                                similarity++;
                            }

                            //bool similar = CompareColors(localColor, globalColor) < 5;

                            //if (similar)
                            //{
                            //    Console.WriteLine("Same pixel color detected");
                            //    similarity++;
                            //}
                        }

                        if (similarity > 75)
                        {
                            Console.WriteLine("Similar image detected");
                            FinderResult res = new FinderResult();
                            res.Percentage = similarity;
                            res.BeatmapPath = beatmap_dir;
                            res.ImagePath = file;
                            results.Add(res);
                        }

                        image.Dispose();
                    }
                }
                Interlocked.Increment(ref MapsProcessed);
            }

            original.Dispose();
            Interlocked.Increment(ref ThreadsFinished);
            Console.WriteLine("Thread finished, total done: " + ThreadsFinished);
            if(ThreadsFinished==ThreadCount){
                Finished = true;
                Console.WriteLine("All finished");

                Console.WriteLine("Found " + results.Count + " similar images");

                foreach (FinderResult res in results)
                {
                    Console.WriteLine(res.BeatmapPath + ": " + res.Percentage + "%");
                }
            }
        }

        public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }
    }
}
