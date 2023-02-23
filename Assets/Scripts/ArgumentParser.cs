using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Options;

public static class ArgumentParser
{
    public class CommandLineOptions
    {
        public int cameraFrequency;   // Number of steps for frame captures
        public string runID;          // Run Identifier
        public int numAgents;          // Display Sets (1 = Single Display, 2 = Opposite Displays, 4 = All)
        public bool clones;          // Display Sets (1 = Single Display, 2 = Opposite Displays, 4 = All)
    }

    private static CommandLineOptions options;
    public static CommandLineOptions Options
    {
        get
        {
            // Parse command line when this property is accessed for the first time.
            if (options == null) ParseCommandLineArgs();
            return options;
        }
    }

    private static void ParseCommandLineArgs()
    {
        var args = System.Environment.GetCommandLineArgs();

        var parser = new OptionSet() {
            // Required Options
            {"cam-frequency=", "Number of steps between recording a frame.",
                (int v) => options.cameraFrequency = v},
            {"agents=", "Number of agents to instantiate.",
                (int v) => options.numAgents= v},
             {"clones", "Are agents are all clones of the same brain?",
                v => options.clones = v != null},
            {"id=", "Run Identifier.",
                (string v) => options.runID = v},
        };

        options = new CommandLineOptions();
        try
        {
            parser.Parse(args);
        }
        catch (OptionException e)
        {
            Debug.Log("OptionException: " + e.ToString());
        }
    }
}
