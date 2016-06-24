﻿using nom.tam.fits;
using nom.tam.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AstrophotographyBuddy.Utility {
    static class Utility {

        public class ImageArray {
            public Array SourceArray;
            public Int16[] FlatArray;
            public int X;
            public int Y;
        }

        private static PHD2Client _pHDClient;
        public static PHD2Client PHDClient {
            get {
                if (_pHDClient == null) {
                    _pHDClient = new PHD2Client();
                }
                return _pHDClient;
            }
            set {
                _pHDClient = value;
            }
        }

        public static ImageArray convert2DArray(Int32[,] arr) {
            /*ImageArray iarr = new ImageArray();
            iarr.SourceArray = arr;

            int width = arr.GetLength(0);
            int height = arr.GetLength(1);
            iarr.X = width;
            iarr.Y = height;
            Int16[] flatArray = new Int16[width * height];

            Int16 val;
            int idx = 0;
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                     
                     val = (Int16)((Int32)arr.GetValue(j, i));                    
                    flatArray[idx] = val;
                    idx++;
                }
            }*/
            ImageArray iarr = new ImageArray();
            iarr.SourceArray = arr; 
            int width = arr.GetLength(0);
            int height = arr.GetLength(1);
            iarr.Y = width;
            iarr.X = height;
            Int16[] flatArray = new Int16[arr.Length];
            
            unsafe { 
                fixed (Int32* ptr = arr)
                {
                    for (int i = 0; i < arr.Length; i++) {
                        Int16 b = (Int16)ptr[i];
                        
                        flatArray[i] = b;
                    }
                }
            }
            iarr.FlatArray = flatArray;
            return iarr;
            //iarr.FlatArray = flatArray;            
            //return iarr;
        }

        public static Int16[] flatten2DArray(Array arr) {
            int width = arr.GetLength(0);
            int height = arr.GetLength(1);
            Int16[] flatArray = new Int16[width * height];
            Int16 val;
            int idx = 0;
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    val = (Int16)(Int16.MinValue + Convert.ToInt16(arr.GetValue(j, i)));

                    flatArray[idx] = val;
                    idx++;
                }
            }
            return flatArray;
        }


        public static T[] flatten2DArray<T>(Array arr) {
            int width = arr.GetLength(0);
            int height = arr.GetLength(1);
            T[] flatArray = new T[width * height];
            T val;
            int idx = 0;
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    val = (T)Convert.ChangeType(arr.GetValue(j, i), typeof(T));

                    flatArray[idx] = val;
                    idx++;
                }
            }
            return flatArray;
        }

        public static T[] flatten3DArray<T>(Array arr) {
            int width = arr.GetLength(0);
            int height = arr.GetLength(1);
            int depth = arr.GetLength(2);
            T[] flatArray = new T[width * height * depth];
            T val;
            int idx = 0;
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    for (int k = 0; k < depth; k++) {

                        val = (T)Convert.ChangeType(arr.GetValue(j, i, k), typeof(T));

                        flatArray[idx] = val;
                        idx++;
                    }
                }
            }
            return flatArray;
        }

        /*public static void saveFits(ImageArray iarr) {
            Stopwatch sw = Stopwatch.StartNew();

            Header h = new Header();
            h.AddValue("SIMPLE", "T", "C# FITS");
            h.AddValue("BITPIX", 16, "");
            h.AddValue("NAXIS", 2, "Dimensionality");
            h.AddValue("NAXIS1", iarr.SourceArray.GetUpperBound(0)+1, "" );
            h.AddValue("NAXIS2", iarr.SourceArray.GetUpperBound(1) + 1, "");
            h.AddValue("BZERO", 32768, "");
            h.AddValue("EXTEND", "T", "Extensions are permitted");

            ImageData d = new ImageData(iarr.CurledArray);
            d = new ImageData();            
            sw.Stop();
            Console.WriteLine("CreateDataForHDU: " + sw.Elapsed);
            
            sw = Stopwatch.StartNew();
            ImageHDU imageHdu = new ImageHDU(h,d);

            //BasicHDU imageHdu = FitsFactory.HDUFactory(iarr.CurledArray);
            sw.Stop();
            Console.WriteLine("CreateHDU: " + sw.Elapsed);


            //imageHdu.AddValue("BZERO", 32768, "");

            sw = Stopwatch.StartNew();
            Fits f = new Fits();
            f.AddHDU(imageHdu);
            sw.Stop();
            Console.WriteLine("Create FITS: " + sw.Elapsed);

            sw = Stopwatch.StartNew();
            try {
                FileStream fs = File.Create("test.fit");
                f.Write(fs);
                fs.Close();

            } catch(Exception ex) {

            }
            sw.Stop();
            Console.WriteLine("Save FITS: " + sw.Elapsed);
        }*/

        public static void saveTiff(ImageArray iarr, String path) {         
            
            try {
                BitmapSource bmpSource = createSourceFromArray(iarr.FlatArray, iarr.X, iarr.Y);

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (FileStream fs = new FileStream(path + ".tif", FileMode.Create)) {
                    TiffBitmapEncoder encoder = new TiffBitmapEncoder();
                    encoder.Compression = TiffCompressOption.None;
                    encoder.Frames.Add(BitmapFrame.Create(bmpSource));
                    encoder.Save(fs);
                }
            } catch(Exception ex) {

                }
        }


        /*public static BitmapSource createSourceFromArray(Int16[,] arr) {
            BitmapSource source;
            unsafe
            {
                System.Windows.Media.PixelFormat pf = System.Windows.Media.PixelFormats.Gray16;
                int x = arr.GetUpperBound(0) + 1;
                int y = arr.GetUpperBound(1) + 1;
                //int stride = C.CameraYSize * ((Convert.ToString(C.MaxADU, 2)).Length + 7) / 8;
                int stride = (y * pf.BitsPerPixel + 7) / 8;
                double dpi = 96;
                fixed (Int16* intPtr = &arr[0,0]) {
                    
                    source = BitmapSource.Create(y, x, dpi, dpi, pf, null, new IntPtr(intPtr), arr.Length *2 , stride);
                }
            }
            
            return source;
        }*/

        public static BitmapSource createSourceFromArray(Array flatArray, int x, int y) {

            System.Windows.Media.PixelFormat pf = System.Windows.Media.PixelFormats.Gray16;

            //int stride = C.CameraYSize * ((Convert.ToString(C.MaxADU, 2)).Length + 7) / 8;
            int stride = (x * pf.BitsPerPixel + 7) / 8;
            double dpi = 96;

            BitmapSource source = BitmapSource.Create(x, y, dpi, dpi, pf, null, flatArray, stride);
            return source;
        }

        public static int[] getDim(Array arr) {
            int[] dim = new int[2];
            dim[0] = arr.GetUpperBound(1) + 1;
            dim[1] = arr.GetUpperBound(0) + 1;
            return dim;
        }

        public static string getImageFileString(ICollection<ViewModel.OptionsVM.ImagePattern> patterns) {
            string s = Settings.ImageFilePattern;
            foreach(ViewModel.OptionsVM.ImagePattern p in patterns) {
                s = s.Replace(p.Key, p.Value);
            }
            return s;
        }

        public async static Task<string> ConnectPHD2(string message) {
            return await Connect(Settings.PHD2ServerUrl, Settings.PHD2ServerPort, message);
        }
                
        public async static Task<string> Connect(string serverIP, int port, string message) {
            string output = "";

            try {
                // Create a TcpClient.
                // The client requires a TcpServer that is connected
                // to the same address specified by the server and port
                // combination.                
                TcpClient client = new TcpClient(serverIP, port);

                // Translate the passed message into ASCII and store it as a byte array.
                Byte[] data = new Byte[1024];
                data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                // Stream stream = client.GetStream();
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                output = "Sent: " + message;
                //System.Windows.MessageBox.Show(output);

                // Buffer to store the response bytes.
                data = new Byte[1024];

                // String to store the response ASCII representation.
                String responseData = String.Empty;


                // Read the first batch of the TcpServer response bytes.

                string resp = "";
                do {
                    Int32 bytes = await stream.ReadAsync(data, 0, data.Length);
                    resp = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    responseData += resp;
                } while (resp.Trim() != string.Empty);
                
                
                    

                //responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                output = responseData;
                //System.Windows.MessageBox.Show(output);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e) {
                output = e.Message;
                //System.Windows.MessageBox.Show(output);
            }
            catch (SocketException e) {
                output = e.Message;
                //System.Windows.MessageBox.Show(output);
            }
            return output;
        }
    }


}
