using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    static public int CurrentLevel = 0;
    static public int CurrentPhase = 0;

    List<List<List<int>>> Levels;
    string sourceFile = "LevelData";
    char LevelSymbol = '#';
    char PhaseSymbol = '-';
    char Separator = ',';

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // LoadLevelData()
    //
    // load level data from Resources/LevelData.txt
    public List<List<List<int>>> LoadLevelData()
    {
        TextAsset data = Resources.Load<TextAsset>(sourceFile);
        string[] lines = data.text.Split('\n');

        Levels = new List<List<List<int>>>();
        List<List<int>> Level = new List<List<int>>();
        List<int> Phase = new List<int>();

        for (int i=0; i<lines.Length; i++)
        {
            // remove white space 
            string line = lines[i].Replace(" ", "");

            if (line.Length == 0) continue;

            // get first char 
            if (line[0] == LevelSymbol)
            {
                // add last phase to current Level
                if (Phase.Count > 0) Level.Add(Phase);
                // start new phase 
                Phase = new List<int>();
                // add last level to Levels
                if (Level.Count>0) Levels.Add(Level);
                // start new level
                Level = new List<List<int>>();
                continue;
            }

            if (line[0] == PhaseSymbol)
            {
                // add last phase to current Level
                if (Phase.Count > 0) Level.Add(Phase);
                // start new phase 
                Phase = new List<int>();

                // now split data and populate this phase
                line = line.Substring(1);
                string[] splitstring = line.Split(Separator);
                foreach(string str in splitstring)
                {
                    int result;
                    if(int.TryParse(str, out result))
                    {
                        Phase.Add(result);
                    }
                    else
                    {
                        Debug.Log("Conversion failed!");
                        break;
                    }
                }
            }
        }

        // add last phase and level to data
        if (Phase.Count > 0) Level.Add(Phase);
        if (Level.Count > 0) Levels.Add(Level);

        // done!
        return GetLevelData();
    }

    public List<List<List<int>>> GetLevelData()
    {
        return Levels;
    }
}
