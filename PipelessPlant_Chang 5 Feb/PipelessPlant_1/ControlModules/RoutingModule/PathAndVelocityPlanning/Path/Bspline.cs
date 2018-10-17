using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.Path
{
    using MULTIFORM_PCS.ControlModules.RoutingModule.PathAndVelocityPlanning.DataTypes;
    class Bspline
    {
        public int FindSpan(int numberOfControlPoints, int degreeOfBSpline, float inputU, float[] knotVector)
        {
            /*Determine the knot span index */
            /*Input n,p,u,U*/
            /*Return : the knot span index*/
            /*Resource : Algorithm A2.1 from book : The NURBS Book -- by Les Piegl & Wayne Tiller*/
            if (inputU == knotVector[numberOfControlPoints + 1])
            {
                return numberOfControlPoints;  /*Special case*/
            }

            int low = degreeOfBSpline;   /*Do Binary search*/
            int high = numberOfControlPoints + 1;

            int mid = (low + high) / 2;

            while (inputU < knotVector[mid] || inputU >= knotVector[mid + 1])
            {
                if (inputU < knotVector[mid])
                {
                    high = mid;
                }
                else
                {
                    low = mid;
                }

                mid = (low + high) / 2;
            }

            return mid;
        }

        public void NonVanishingBasisFunctions(int spanIndex, float inputU, int degreeOfBSpline, float[] knotVector, float[] N)
        {
            /*Compute the nonvanishing basis functions for B-spline*/
            /*input: i , u, p, U */
            /*output: stores all nonvnishing basis functions for a given i in an array called N (N[0] , N[1], N[2] , ... ,N[p])*/
            /*Resource : Algorithm A2.2 from book : The NURBS Book -- by Les Piegl & Wayne Tiller*/
            //float[] N      =  new float[degreeOfBSpline + 1];
            float[] left = new float[degreeOfBSpline + 1];
            float[] right = new float[degreeOfBSpline + 1];

            N[0] = 1.0f;

            for (int j = 1; j <= degreeOfBSpline; j++)
            {
                left[j] = inputU - knotVector[spanIndex + 1 - j];
                right[j] = knotVector[spanIndex + j] - inputU;
                float saved = 0.0f;

                for (int r = 0; r < j; r++)
                {
                    float temp = N[r] / (right[r + 1] + left[j - r]);
                    N[r] = saved + right[r + 1] * temp;
                    saved = left[j - r] * temp;
                }
                N[j] = saved;
                //Console.WriteLine(N[j]);
            }
        }

        public float[] generateKnotVector(int n, int p)
        {
            /* n = numberOfControlPoints - 1 */
            /* p = degree Of B-Spline        */
            float[] knotVectors = new float[n + p + 2];
            int highestdegreeInKnotVector = knotVectors.Length - 1;


            for (int i = 0; i <= p; i++)
            {
                knotVectors[i] = 0;
                knotVectors[highestdegreeInKnotVector - p + i] = 1;
            }
            for (int j = 1; j <= (n - p); j++)
            {
                knotVectors[j + p] = (float)j / (n - p + 1);
                // Console.WriteLine(knotVectors[j + p]);
            }

            return knotVectors;
        }

        public Position[] getBSpline(int degree, List<Position> points, float[] knotVector)
        {
            float step = (float)1 / 40;
            int numberOfsteps = 41;

            Position[] spline = new Position[numberOfsteps];
            float[] xs = new float[numberOfsteps];
            float[] ys = new float[numberOfsteps];
            float temp_x = 0.0f;
            float temp_y = 0.0f;
            float[] N = new float[degree + 1];
            //int p = 0;
            for (int j = 0; j < numberOfsteps; j++)
            {
                temp_x = 0;
                temp_y = 0;
                int span = FindSpan(points.Count - 1, degree, (float)(j * step), knotVector);
                NonVanishingBasisFunctions(span, (float)(j * step), degree, knotVector, N);
                for (int i = 0; i <= degree; i++)
                {
                    temp_x = temp_x + (N[i] * points[span - degree + i].X);
                    temp_y = temp_y + (N[i] * points[span - degree + i].Y);
                }

                spline[j] = new Position(temp_x, temp_y);

                xs[j] = spline[j].X;
                ys[j] = spline[j].Y;


            }
            return spline;
        }


    }
}
