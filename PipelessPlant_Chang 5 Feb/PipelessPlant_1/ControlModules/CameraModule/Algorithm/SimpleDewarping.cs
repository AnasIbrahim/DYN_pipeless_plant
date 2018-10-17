using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace MULTIFORM_PCS.ControlModules.CameraModule.Algorithm
{
    public class SimpleDewarping
    {
        /**private double k1 = 1200.0d / 1000000000.0d;//1600x1200
        private double k2 = 1200.0d / 1000000000.0d;//1600x1200
        private double k3 = -650.0d / 100.0d;//1600x1200
        private double k4 = -100.0d / 100.0d;//1600x1200*/

        private double k1 = 1000.0d / 1000000000.0d;//800x600
        private double k2 = 1300.0d / 1000000000.0d;//800x600
        private double k3 = 100.0d / 100.0d;//800x600
        private double k4 = -100.0d / 100.0d;//800x600

        //private double kx0 = 371.8, kx1 = -0.005194, kx2 = -0.4709;
        //private double ky0 = 18.4, ky1 = 0.4621, ky2 = -0.003839;
        private int length = 800, width = 600;


        private double kx0 = 367.6, kx1 = -0.0696, kx2 = -0.3788, kx3 = 3.962e-5,
    kx4 = -7.468e-5, kx5 = -5.593e-6, kx6 = 5.27, kx7 = -5.238;
        private double ky0 = 32.04, ky1 = 0.1705, ky2 = 0.1905, ky3 = 0.0002672,
    ky4 = -0.0001278, ky5 = 1.38e-5, ky6 = 16.02, ky7 = -15.54;

        //private double offsetX = 180.0d;//1600x1200
        //private double offsetY = 115.0d;//1600x1200
        //private double scale = 0.785d;//1600x1200

        //Mapping of Fisheye Image Coords to Rectilinear Image Coords
        private Point[][] mapping;

        //Pre-calculate dewarping
        public SimpleDewarping(int width, int height)
        {
            mapping = new Point[width][];
            for (int i = 0; i < width; i++)
            {
                mapping[i] = new Point[height];
            }

            double centerX = (width / 2.0d) + 10;
            double centerY = height / 2.0d;

            Parallel.For(0, height, delegate(int y)
            {
                for (int x = 0; x < width; x++)
                {
                    double u = 0;
                    if (x <= centerX)
                    {
                        u = Math.Pow(y - centerY, 2) * k1 * Math.Abs(x - centerX) + x + k3 * Math.Pow(Math.Abs(x - centerX), 0.02);
                    }
                    else
                    {
                        u = Math.Pow(y - centerY, 2) * (-k1) * Math.Abs(x - centerX) + x - /**0.7 **/ k3 * Math.Pow(Math.Abs(x - centerX), 0.005);
                    }

                    double v = 0;
                    if (y <= centerY)
                    {
                        v = Math.Pow(x - centerX, 2) * k2 * Math.Abs(y - centerY) + y + k4 * Math.Pow(Math.Abs(y - centerY), 0.6);
                    }
                    else
                    {
                        v = Math.Pow(x - centerX, 2) * (-k2) * Math.Abs(y - centerY) + y - k4 * Math.Pow(Math.Abs(y - centerY), 0.6);
                    }

                    int u_1 = (int)(u);
                    int v_1 = (int)(v);
                    if (u_1 < width && v_1 < height && u_1 >= 0 && v_1 >= 0)
                    {

                        mapping[u_1][v_1] = new Point(x, y);
                      
                    }
                    else
                    {
                        //mapping[u_1][v_1] = new Point(-1, -1);
                    }
                }
            });

            //INTERPOLATE ALL POINTS WHICH CANNOT BE MAPPED DIRECTLY
            int interpolationParameter = 25;
            for (int i = interpolationParameter; i < width - interpolationParameter; i++)
            {
                for (int j = interpolationParameter; j < height - interpolationParameter; j++)
                {
                    if (mapping[i][j].X == 0 && mapping[i][j].Y == 0)
                    {
                        for (int k = 0; k < interpolationParameter; k++)
                        {
                            if (mapping[i + k][j + k].X != 0)
                            {
                                mapping[i][j].X = mapping[i + k][j + k].X;
                                mapping[i][j].Y = mapping[i + k][j + k].Y;
                            }
                            else if (mapping[i + k][j - k].X != 0)
                            {
                                mapping[i][j].X = mapping[i + k][j - k].X;
                                mapping[i][j].Y = mapping[i + k][j - k].Y;
                            }
                            else if (mapping[i - k][j - k].X != 0)
                            {
                                mapping[i][j].X = mapping[i - k][j - k].X;
                                mapping[i][j].Y = mapping[i - k][j - k].Y;
                            }
                            else if (mapping[i - k][j + k].X != 0)
                            {
                              mapping[i][j].X = mapping[i - k][j + k].X;
                              mapping[i][j].Y = mapping[i - k][j + k].Y;
                            }
                            else
                            {
                                mapping[i][j].X = -666;
                                mapping[i][j].Y = -666;

                            }
                        }
                      // Vorwärts Team Leonard !!!!11elf 
                    //    mapping[i][j].X = -666;
                    //    mapping[i][j].Y = -666;
                    }
                }
            }
        }

        //Dewarp complete image
        public int[] simpleDefishImage(int[] originalPixelData, int width, int height)
        {
            int[] dewarpedPixelData = new int[originalPixelData.Length];

            double centerX = width / 2.0d;
            double centerY = height / 2.0d;

            if (originalPixelData != null && dewarpedPixelData != null)
            {
                Parallel.For(0, height, delegate(int y)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double u = 0;
                        if (x <= centerX)
                        {
                            u = Math.Pow(y - centerY, 2) * k1 * Math.Abs(x - centerX) + x + k3 * Math.Pow(Math.Abs(x - centerX), 0.02);
                        }
                        else
                        {
                            u = Math.Pow(y - centerY, 2) * (-k1) * Math.Abs(x - centerX) + x - 0.7 * k3 * Math.Pow(Math.Abs(x - centerX), 0.005);
                        }

                        double v = 0;
                        if (y <= centerY)
                        {
                            v = Math.Pow(x - centerX, 2) * k2 * Math.Abs(y - centerY) + y + k4 * Math.Pow(Math.Abs(y - centerY), 0.6);
                        }
                        else
                        {
                            v = Math.Pow(x - centerX, 2) * (-k2) * Math.Abs(y - centerY) + y - k4 * Math.Pow(Math.Abs(y - centerY), 0.6);
                        }

                        int pixel = 0;
                        int u_1 = (int)(u);
                        int v_1 = (int)(v);
                        if (u_1 < width && v_1 < height && u_1 >= 0 && v_1 >= 0)
                        {
                            pixel = v_1 * width + u_1;
                            dewarpedPixelData[(y) * width + x] = originalPixelData[pixel];
                        }
                        else
                        {
                            dewarpedPixelData[(y) * width + x] = System.Drawing.Color.White.ToArgb();
                        }
                    }
                });
            }

            return dewarpedPixelData;
        }

        //Dewarp one point, e.g. center of robot
       /* public Point simpleDefishPoint(int x, int y)
        {
            if (x >= 50 && x < mapping.Length
                && y >= 0 && y < mapping[0].Length)
            {
              if(y>=1 && y < mapping[0].Length  -1)
              {
                if (((Math.Abs(mapping[x][y - 1].X - mapping[x][y].X) > 10
                  || Math.Abs(mapping[x][y - 1].Y - mapping[x][y].Y) > 10))
                &&((Math.Abs(mapping[x][y + 1].X - mapping[x][y].X) > 10
                  || Math.Abs(mapping[x][y + 1].Y - mapping[x][y].Y) > 10))
                  &&((Math.Abs(mapping[x][y - 1].X - mapping[x][y+1].X) < 10
                  || Math.Abs(mapping[x][y + 1].Y - mapping[x][y-1].Y) < 10)))
                {
                  return mapping[x][y-1];
                } 
              }
                return mapping[x][y];
            }
            else
            {
                Console.WriteLine("POINT OUT OF RANGE!");
                return new Point(-666, -666);
            }
        }*/
        public Point simpleDefishPoint(double u, double v)
        {
            double x, y;
            if (u >= 0 && v <= length && u >= 0 && v <= width)
            {
                x = kx0 + kx1 * u+ kx2 * v + kx3 * u * u + kx4 * v * v + kx5 * u * v + kx6 * u / v + kx7 * v / u;
                y = ky0 + ky1 * u + ky2 * v + ky3 * u * u + ky4 * v * v + ky5 * u * v + ky6 * u / v + ky7 * v / u;
                //x = kx0 + kx1 * u + kx2 * v;
                //y = ky0 + ky1 * u + ky2 * v;
            }
            else
            {
                x = -888;
                y = -888;
            }
            return new Point(Math.Round(x, 2), Math.Round(y, 2));
        }

    }
}
