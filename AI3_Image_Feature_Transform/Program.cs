using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI3_Image_Feature_Transform
{
    class Program
    {
        static void Main(string[] args)
        {
            Grafika g = new Grafika();
            g.LoadRGBMatrixFromPath("C:/Users/Intel/Desktop/pajablack.png");
            //g.PrintImageToTextFile("textpaja");
            //Algorytm działa na obrazakach czarno białych
            //g.ApplyBlackWhiteMaskAndSave("pajablack");
            g.FindFeaturePoints("featuresPaja", 30, detectionTreshold: 8);

            Console.WriteLine("Koniec programu");
            Console.ReadKey();
        }
    }


    class Grafika
    {
        private Bitmap img;
        private Check[] ChecksToPerform;

        public Grafika()
        {

        }

        public void LoadRGBMatrixFromPath(String path)
        {
            try
            {
                img = new Bitmap(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during loading RBG Matrix: {ex.Message}");
            }
        }

        public void FindFeaturePoints(String filename, Int32 treshold, Int32 detectionTreshold)
        {
            Bitmap imgN = new Bitmap(img.Width, img.Height);
            //zakladamy okrąg Bresenhama o promieniu 3 dla którego sprawdzamy pixele
            ChecksToPerform = new Check[]
            {
                new Check(0, -3),
                new Check(3,  0),
                new Check(0, 3),
                new Check(-3, 0),

                new Check(1, -3),
                new Check(2, -2),
                new Check(3, -1),
                new Check(3, 1),
                new Check(2, 2),
                new Check(1, 3),
                new Check(-1, 3),
                new Check(-2, 2),
                new Check(-3, 1),
                new Check(-3, -1),
                new Check(-2, -2),
                new Check(-1, -3),
            };

            List<Check> featurePoints = new List<Check>();

            for (Int32 w = 4; w < img.Width - 4; w++)
            {
                for (Int32 h = 4; h < img.Height - 4; h++)
                {
                    List<Check> localCheck = new List<Check>();
                    Int32 inspectedPixelDensity = img.GetPixel(w, h).R;

                    for (Int32 i = 0; i < 4; i++)
                    {
                        Boolean? brighter;
                        Int32 localPixelDensity = img.GetPixel(w + ChecksToPerform[i].X, h + ChecksToPerform[i].Y).R;
                        if (localPixelDensity > inspectedPixelDensity + treshold)
                            brighter = true;
                        else if (localPixelDensity < inspectedPixelDensity - treshold)
                            brighter = false;
                        else brighter = null;
                        localCheck.Add(new Check(w + ChecksToPerform[i].X, h + ChecksToPerform[i].Y) { Brighter = brighter, Positive = brighter != null });
                    }
                    Int32 brighterCount = localCheck.Count(c => c.Brighter != null && c.Brighter.Value);
                    Int32 NOTbrighterCount = localCheck.Count(c => c.Brighter != null && !c.Brighter.Value);
                    Int32 failureCount = localCheck.Count(c => c.Brighter == null);

                    if (failureCount > 1)
                        continue;

                    Boolean focusOnBright = brighterCount > NOTbrighterCount;

                    List<Check> localCheckRest = new List<Check>();

                    for (Int32 i = 4; i < 16; i++)
                    {
                        Boolean? brighter;
                        Int32 localPixelDensity = img.GetPixel(w + ChecksToPerform[i].X, h + ChecksToPerform[i].Y).R;
                        if (localPixelDensity > inspectedPixelDensity + treshold)
                            brighter = true;
                        else if (localPixelDensity < inspectedPixelDensity - treshold)
                            brighter = false;
                        else brighter = null;
                        localCheckRest.Add(new Check(w + ChecksToPerform[i].X, h + ChecksToPerform[i].Y) { Brighter = brighter, Positive = brighter != null });
                    }
                    Boolean desiredCountHit = localCheckRest.Count(c => c.Brighter == focusOnBright) > detectionTreshold;
                    if (desiredCountHit)
                        featurePoints.Add(new Check(w, h));
                }
            }

            Image imageLocal = img;

            Graphics g = Graphics.FromImage(imageLocal);

            foreach (Check featurePoint in featurePoints)
                g.DrawEllipse(Pens.Red, new Rectangle(featurePoint.X, featurePoint.Y, 3, 3));

            g.DrawImage(imageLocal, new Point(0, 0));

            imageLocal.Save($"C:/Users/Intel/Desktop/{filename}.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        public void ApplyBlackWhiteMaskAndSave(String filename)
        {
            for (Int32 w = 0; w < img.Width; w++)
            {
                for (Int32 h = 0; h < img.Height; h++)
                {
                    Color pxl = img.GetPixel(w, h);

                    Int32 avg = (pxl.R + pxl.G + pxl.B) / 3;

                    img.SetPixel(w, h, Color.FromArgb(avg, avg, avg));
                }
            }

            img.Save($"C:/Users/Intel/Desktop/{filename}.png");
        }

        public void PrintImageToTextFile(String filename)
        {
            StringBuilder sb = new StringBuilder();
            for (Int32 w = 0; w < img.Width; w++)
            {
                for (Int32 h = 0; h < img.Height; h++)
                {
                    Color pxl = img.GetPixel(w, h);

                    String pixelData = $"({pxl.R.ToString().PadLeft(3, '0')} {pxl.G.ToString().PadLeft(3, '0')} {pxl.B.ToString().PadLeft(3, '0')})";

                    sb.Append(pixelData);
                }
                sb.AppendLine();
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter($"C:/Users/Intel/Desktop/{filename}.txt"))
            {
                file.WriteLine(sb.ToString());
            }
        }

        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }

    public enum FilterKind
    {
        Gauss = 1,
        Blur = 2,
        Sharp = 3
    }

}
