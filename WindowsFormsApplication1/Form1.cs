using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Imaging.ComplexFilters;
using AForge.Imaging.Filters;
using SVM;


using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using AForge.Imaging.Textures;
using System.IO;
namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {


        List<Bitmap> images;
        List<float[]> featuresofallimages;
        List<float[]> featuresofalltest;
        int count = 0;
        FileInfo[] Images;
        Model model;
        Bitmap im;
        Bitmap originalImage;
        public Form1()
        {
            InitializeComponent();
        }
        List<Bitmap> LoadImages(String folderpath)
        {

            String[] imagePath = Directory.GetFiles(folderpath, "*.*");
            images = new List<Bitmap>();
            foreach (var path in imagePath)
            {
                count++;
                Bitmap image = new Bitmap(path);
                images.Add(image);
            }
            return images;
        }
        public void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            featurefortest();

        }
        public void featurefortest()
        {
            string filepath = @"F:test.txt";
            List<int> dummy = new List<int>(0);
            OpenFileDialog openDialog = new OpenFileDialog();
            var returnValue = openDialog.ShowDialog();
            if (returnValue == DialogResult.OK)
            {
                List<float> featuresfortestcase = new List<float>();
                featuresofalltest = new List<float[]>();
                List<float[]> ftcase = new List<float[]>();
                originalImage = new Bitmap(openDialog.FileName);
                 featuresfortestcase = ExtractFeaturOfOneImage(originalImage);
                featuresofalltest.Add(featuresfortestcase.ToArray());
                FileStream fst = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                StreamWriter swt = new StreamWriter(fst);
                for (int i = 0; i < featuresofalltest.Count; i++)
                {
                    int c = 0;
                    swt.Write(0 + " ");
                    foreach (var value in featuresofalltest[i])
                    {
                        swt.Write(c + 1 + ":" + value + " ");
                        c++;
                    }
                    swt.WriteLine();

                }
                swt.Flush();
                swt.Close();
                fst.Close();

            }
            

        }
        void WriteToFile(String filepath, List<float[]> features, List<int> labels)
        {
            FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            for (int i = 0; i < features.Count; i++)
            {
                int c = 0;
                sw.Write(labels[i] + " ");
                foreach (var value in features[i])
                {
                    sw.Write(c + 1 + ":" + value + " ");
                    c++;
                }
                sw.WriteLine();

            }
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        public void finalfeature()
        {
            String[] lookuptable = {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"};
            String partialpath = @"F:\Gesture\";
            List<float[]> finalfeatures = new List<float[]>();
            List<int> labels = new List<int>();
            for (int i = 0; i < lookuptable.Length; i++)
            {
                String Fullpath = partialpath + lookuptable[i];
                List<Bitmap> rawimages = LoadImages(Fullpath);
                List<float[]> features = ExtractFeaturesOfAllImages(rawimages);
                finalfeatures.AddRange(features);
                for (int j = 0; j < features.Count; j++)
                {
                    labels.Add(i + 1);
                }
            }
            MessageBox.Show("done");
            WriteToFile(@"F:train.txt", finalfeatures, labels);
        }
        public Byte max(Byte a, Byte b, Byte c)
        {
            if (a > b && a > c) return a;
            else if (b > a && b > c) return b;
            else return c;
        }
        public Byte min(Byte a, Byte b, Byte c)
        {
            if (a < b && a < c) return a;
            else if (b < a && b < c) return b;
            else return c;
        }
        public Bitmap ResizeBitmap(Bitmap b, int nWidth, int nHeight)
        {
            Bitmap result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage((Bitmap)result)) g.DrawImage(b, 0, 0, nWidth, nHeight);
            return result;
        }
        public List<float> ExtractFeaturOfOneImage(Bitmap image1)
        {
            im = (Bitmap)image1.Clone();
            detect();


            /* for (int x = 0; x < im.Width; x++)
             {
                 for (int y = 0; y < im.Height; y++)
                 {
                     Color pixelColor = im.GetPixel(x, y);
                     Byte r = pixelColor.R;
                     Byte g = pixelColor.G;
                     Byte b = pixelColor.B;
                     if (r > 95 && g > 40 && b > 20 && (max(r, g, b) - min(r, g, b)) > 15 && Math.Abs(r - g) > 15 && r > g && r > b)
                     {
                         im.SetPixel(x, y, Color.White);

                     }
                     else
                         im.SetPixel(x, y, Color.Black);
                 }
             }*/
            Bitmap resizeim = ResizeBitmap(im, 500, 500);
            GrayscaleBT709 grayobj = new GrayscaleBT709();
            Bitmap imagegray = grayobj.Apply(resizeim);
            Threshold obj = new Threshold(100);
            Bitmap im1 = obj.Apply(imagegray);
            Closing cobj = new Closing();
            Bitmap im2 = cobj.Apply(im1);
            Opening oobj = new Opening();
            Bitmap im3 = oobj.Apply(im2);
            ExtractBiggestBlob eobj = new ExtractBiggestBlob();
            Bitmap eblob = eobj.Apply(im3);
            AForge.IntPoint a = eobj.BlobPosition;
            pictureBox3.Image = eblob;
            Bitmap im4 = (Bitmap)im.Clone();
            im4 = ResizeBitmap(im4, 500, 500);
            for (int i = 0; i < eblob.Width; i++)
            {

                for (int j = 0; j < eblob.Height; j++)
                {
                    int x = i + a.X;
                    int y = j + a.Y;
                    Color pixelcolor1 = eblob.GetPixel(i, j);
                    Byte r = pixelcolor1.R;
                    Byte g = pixelcolor1.G;
                    Byte b = pixelcolor1.B;

                    if (r == 255 && g == 255 && b == 255)
                        continue;
                    else
                        im4.SetPixel(x, y, Color.Black);
                }

            }
            for (int i = 0; i < eblob.Width; i++)
            {

                for (int j = 0; j < eblob.Height; j++)
                {
                    if ((i <= a.X || i >= eblob.Width + a.X) || (j <= a.Y || j >= eblob.Height + a.Y))
                    {

                        im4.SetPixel(i, j, Color.Black);
                    }
                }
            }
            GrayscaleBT709 objgray = new GrayscaleBT709();
            Bitmap grayim = objgray.Apply(im4);
            CannyEdgeDetector cannydetector = new CannyEdgeDetector();
            Bitmap cannyim = cannydetector.Apply(grayim);
            pictureBox2.Image = cannyim;
            int sum = 0;
            pictureBox4.Image = im4;
            int p = cannyim.Width / 6;
            int q = cannyim.Height / 6;
            List<int> featureVector = new List<int>();
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                {
                    count = 0;
                    for (int x = i * p; x < ((i * p) + p); x++)
                        for (int y = j * q; y < ((j * q) + q); y++)
                        {
                            Color pixelcolor1 = cannyim.GetPixel(x, y);

                            if (pixelcolor1.R != 0 && pixelcolor1.G != 0 && pixelcolor1.B != 0)
                             
                                count++;
                        }
                    sum += count;
                    featureVector.Add(count);
                    String str1 = Convert.ToString(featureVector[j + i]);
                    //  MessageBox.Show(str1);

                }
            // MessageBox.Show("done");
            String str4 = Convert.ToString(sum);
            //  MessageBox.Show(str4);
            List<float> per = new List<float>();
            for (int i = 0; i < 36; i++)
            {
                float gg = (float)(featureVector[i] / (sum * 1.0));
                per.Add(gg);
                String str3 = Convert.ToString(gg);

                // MessageBox.Show(str3);

            }
            return per;
        }
        public List<float[]> ExtractFeaturesOfAllImages(List<Bitmap> images)
        {
            featuresofallimages = new List<float[]>();
            List<float> featuresofoneimage = new List<float>();
            foreach (var image in images)
            {
                Bitmap image1 = ResizeBitmap(image, 300, 300);
                featuresofoneimage = ExtractFeaturOfOneImage(image);
                featuresofallimages.Add(featuresofoneimage.ToArray());
            }
            return featuresofallimages;
        }

        private void openAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            finalfeature();
        }

        private void multiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            finalfeature();
        }

        public void detect()
        {


            var rect = new Rectangle(0, 0, im.Width, im.Height);
            var data = im.LockBits(rect, ImageLockMode.ReadWrite, im.PixelFormat);
            var depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8; //bytes per pixel

            var buffer = new byte[data.Width * data.Height * depth];

            //copy pixels to buffer
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

            System.Threading.Tasks.Parallel.Invoke(
                () =>
                {
                    //upper-left
                    Process(buffer, 0, 0, data.Width / 2, data.Height / 2, data.Width, depth);
                },
                () =>
                {
                    //upper-right
                    Process(buffer, data.Width / 2, 0, data.Width, data.Height / 2, data.Width, depth);
                },
                () =>
                {
                    //lower-left
                    Process(buffer, 0, data.Height / 2, data.Width / 2, data.Height, data.Width, depth);
                },
                () =>
                {
                    //lower-right
                    Process(buffer, data.Width / 2, data.Height / 2, data.Width, data.Height, data.Width, depth);
                }
              
               

               
               

            );

            //Copy the buffer back to image
            Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);

            im.UnlockBits(data);
            //  pictureBox2.Image = sampleImage;
            // dstImg = sampleImage;
        }

        void Process(byte[] buffer, int x, int y, int endx, int endy, int width, int depth)
        {
            for (int i = x; i < endx; i++)
            {
                for (int j = y; j < endy; j++)
                {
                    var offset = ((j * width) + i) * depth;
                    var B = buffer[offset + 0];
                    var G = buffer[offset + 1];
                    var R = buffer[offset + 2];
                    var a = Math.Max(R, Math.Max(B, G));
                    var b = Math.Min(R, Math.Min(B, G));
                    if (!(((R > 95) && (G > 40) && (B > 20) && ((a - b) > 15) && (Math.Abs(R - G) > 15) && (R > G) && (R > B)) || ((R > 220) && (G > 210) && (B > 170) && ((a - b) > 15) && (Math.Abs(R - G) > 15) && (R > G) && (G > B))))
                    {
                        buffer[offset + 0] = buffer[offset + 1] = buffer[offset + 2] = 0;
                    }
                    else
                    {
                        buffer[offset + 0] = buffer[offset + 1] = buffer[offset + 2] = 255;
                    }
                }
            }

        }

        private void trainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Problem train = Problem.Read(@"F:train.txt");
            Parameter parameters = new Parameter();
            parameters.C = 32; parameters.Gamma = 8;
            model = Training.Train(train, parameters);
        }

        private void resultToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            Problem test = Problem.Read(@"F:test.txt");
            Prediction.Predict(test, @"F:result.text", model, false);

            string contents = File.ReadAllText(@"F:result.text");
            Console.WriteLine(contents);
            int x = Int32.Parse(contents);
            Char c = (char)(x + 64);
            MessageBox.Show(c.ToString());
        }
    }
}