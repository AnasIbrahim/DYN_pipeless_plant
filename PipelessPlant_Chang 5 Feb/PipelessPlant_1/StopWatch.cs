using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

public class StopWatch
{

    private DateTime startTime;
    private bool running = false;
    private TimeSpan elapsedTime;

    

    public void Stop()
    {
        this.running = false;
    }
   

    public bool update()
    {
        if (!running)
        {
            startTime = DateTime.Now;
            elapsedTime = startTime - startTime;
            running = true;
            return false;
        }
        else
        {
            var curTime = DateTime.Now;
            elapsedTime = curTime - startTime;
            startTime = curTime;
            return true;
        }
    }

    // elaspsed time in milliseconds
    public double GetElapsedTime()
    {
        /*
        TimeSpan interval;

        if (running)
            interval = DateTime.Now - startTime;
        else
            interval = stopTime - startTime;
         */

        return elapsedTime.TotalMilliseconds;
    }


    // elaspsed time in seconds
    public double GetElapsedTimeSecs()
    {
        return elapsedTime.TotalSeconds;
    }


   
}
