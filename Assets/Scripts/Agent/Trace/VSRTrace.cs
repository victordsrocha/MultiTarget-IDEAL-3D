using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VSRTrace
{
    private static int decisionCycle = 0;
    private static int primitiveCycle = 0;

    public static void AddRecordHeaders()
    {
        string filepath = "recordFile_decisionCycle.csv";
        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                file.WriteLine("Decision Cycle" + "," + "Valence" + "," + "Success");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        filepath = "recordFile_primitiveCycle.csv";

        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                file.WriteLine("Primitive Cycle" + "," + "Valence" + "," + "Success" + "," + "Food" + "," + "Poison" +
                               "," + "Bump");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static void AddDecisionRecord(Interaction intendedInteraction, Interaction enactedInteraction)
    {
        decisionCycle++;

        string filepath = "recordFile_decisionCycle.csv";
        string valence = enactedInteraction.Valence.ToString();
        string scheme = enactedInteraction.Label;

        string predictionSuccess = (intendedInteraction == enactedInteraction ? 1 : 0).ToString();

        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                file.WriteLine(decisionCycle + "," + valence + "," + predictionSuccess);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static void AddPrimitiveRecord(Interaction primitiveIntendedInteraction,
        Interaction primitiveEnactedInteraction)
    {
        int IsBump(string scheme)
        {
            return scheme[9] == 'b' ? 1 : 0;
        }

        int IsFoodReached(string scheme)
        {
            return scheme[3] == 'r' ? 1 : 0;
        }

        int IsPoisonReached(string scheme)
        {
            return scheme[6] == 'r' ? 1 : 0;
        }

        primitiveCycle++;

        string filepath = "recordFile_primitiveCycle.csv";
        string valence = primitiveEnactedInteraction.Valence.ToString();
        string scheme = primitiveEnactedInteraction.Label;

        string predictionSuccess = (primitiveIntendedInteraction == primitiveEnactedInteraction ? 1 : 0).ToString();
        string food = IsFoodReached(scheme).ToString();
        string poison = IsPoisonReached(scheme).ToString();
        string bump = IsBump(scheme).ToString();

        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                file.WriteLine(primitiveCycle + "," + valence + "," + predictionSuccess + "," + food + "," + poison +
                               "," + bump);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}