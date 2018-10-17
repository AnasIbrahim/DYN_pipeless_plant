using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace MULTIFORM_PCS.Datastructure.Schedule
{
  public class Schedule
  {
    private Datastructure.Model.Recipes.RecipeData recipeRawData;
    public Datastructure.Model.Recipes.RecipeData RawData
    {
      get { return recipeRawData; }
      set { recipeRawData = value; }
    }
    private List<string[]> bestSchedule;
    public List<string[]> BestSchedule
    {
      get { return bestSchedule; }
      set { bestSchedule = value; }
    }
    private List<List<float[]>> agvData;
    public List<List<float[]>> AgvData
    {
      get { return agvData; }
      set { agvData = value; }
    }
    private List<string[]> sequence_mapping;
    public List<string[]> Sequence_mapping
    {
      get { return sequence_mapping; }
      set { sequence_mapping = value; }
    }

    public Schedule(Datastructure.Model.Recipes.RecipeData recipeRawData, string pathToScheduleLog, string pathToMapsLog, string pathToRunsLog, string pathToSequenceList)
    {

      try
      {
        this.recipeRawData = recipeRawData;

        StreamReader scheduleReader = new StreamReader(pathToScheduleLog);
        List<string> lines = new List<string>();
        while (!scheduleReader.EndOfStream)
        {
          string line = scheduleReader.ReadLine();
          if (line != "")
          {
            lines.Add(line);
          }
        }
        scheduleReader.Close();

        if (lines.Count > 2)
        {
          int end = lines.Count - 1;
          int start = 0;
          for (int i = lines.Count - 2; i >= 0; i--)
          {
            if (lines[i] == "#" || lines[i].Contains("#"))
            {
              start = i;
              break;
            }
          }

          List<string> lastSchedule = lines.GetRange(start + 1, lines.Count - 1 - (start + 1));
          bestSchedule = new List<string[]>();
          for (int i = 0; i < lastSchedule.Count; i++)
          {
            bestSchedule.Add(lastSchedule[i].Split('\t', ' '));
          }

          StreamReader mapsReader = new StreamReader(pathToMapsLog);
          List<int> floatVariableCoding = new List<int>();
          while (!mapsReader.EndOfStream)
          {
            string line = mapsReader.ReadLine();
            if (line.Contains("product_variable_7"))
            {
              string[] splitline = line.Split(' ', '\t');
              floatVariableCoding.Add(int.Parse(splitline[3]));
            }
          }
          mapsReader.Close();

          if (floatVariableCoding.Count != recipeRawData.agvUsed)
          {
            GUI.PCSMainWindow.getInstance().postStatusMessage("Error in maps.log file!");
            return;
          }
          agvData = new List<List<float[]>>();
          for (int i = 0; i < floatVariableCoding.Count; i++)
          {
            agvData.Add(new List<float[]>());
          }

          StreamReader runsReader = new StreamReader(pathToRunsLog);
          string runsLog = "";
          while (!runsReader.EndOfStream)
          {
            runsLog = runsReader.ReadToEnd();
          }
          runsReader.Close();

          if (runsLog == "")
          {
            GUI.PCSMainWindow.getInstance().postStatusMessage("Error in runs.log file!");
            return;
          }
          int hash = runsLog.LastIndexOf("#0");
          runsLog = runsLog.Substring(hash);

          StringReader srRuns = new StringReader(runsLog);
          while (srRuns.Peek() != -1)
          {
            string trimRunsLogLine = srRuns.ReadLine();
            string[] splitTrimRunsLogLine = trimRunsLogLine.Split(' ', '\t'); //index 8 for shared and index 9 for clock valuations
            string sharedVariables = splitTrimRunsLogLine[8].Substring(1, splitTrimRunsLogLine[8].Length - 3); //remove [ and ],
            string[] sharedVarSplit = sharedVariables.Split(new char[] { ',' });

            string clockValus = splitTrimRunsLogLine[9].Substring(1, splitTrimRunsLogLine[9].Length - 2); //remove [ and ]
            string[] clockValuesSplit = clockValus.Split(new char[] { ',' });
            float globalClockVal = float.Parse(clockValuesSplit[0]);

            for (int i = 0; i < agvData.Count; i++)
            {
              if (agvData[i].Count == 0)
              {
                agvData[i].Add(new float[] { globalClockVal, float.Parse(sharedVarSplit[floatVariableCoding[i]]) });
              }
              else
              {
                float config = float.Parse(sharedVarSplit[floatVariableCoding[i]]);
                if (config == agvData[i][agvData[i].Count - 1][1])
                {
                  continue;
                }
                else
                {
                  agvData[i].Add(new float[] { globalClockVal, config });
                }
              }
            }
          }
          srRuns.Close();

          StreamReader sequence_reader = new StreamReader(pathToSequenceList);
          sequence_mapping = new List<string[]>();
          while (!sequence_reader.EndOfStream)
          {
            string seqLine = sequence_reader.ReadLine();
            if (seqLine != "")
            {
              sequence_mapping.Add(seqLine.Split('\t'));
            }
          }
          sequence_reader.Close();
        }
        else
        {
          bestSchedule = null;

          GUI.PCSMainWindow.getInstance().postStatusMessage("No schedule found!");
        }

      }
      catch (Exception)
      {
        GUI.PCSMainWindow.getInstance().postStatusMessage("schedules.log not found!");
      }
    }

  }
}
