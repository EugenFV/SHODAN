using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Threading;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.Train;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation;
using System.IO;
using FPSControllerLPFP;
using UnityEngine.UI;

public class ShodanNN : MonoBehaviour
{
    [SerializeField] private Text text = null;
    [SerializeField] private bool retrain = false;
    private double[][] XORInput = null;
    private double[][] XORIdeal = null;
    MyPatterns myObjects = new MyPatterns();
    BasicNetwork network = null;

    // Start is called before the first frame update
    void Start()
    {
        if (retrain)
        {
            RetrainSodan();
            return;
        }
        StartShodan();
    }

    public void RetrainSodan()
    {
        XORInput = new double[myObjects.Patterns.Count][];
        for (var i = 0; i < myObjects.Patterns.Count; ++i)
        {
            XORInput[i] = new double[myObjects.Patterns[i].Inputs.Length];
            for (var indx = 0; indx < myObjects.Patterns[i].Inputs.Length; ++indx)
            {
                XORInput[i][indx] = myObjects.Patterns[i].Inputs[indx];
            }
        }

        XORIdeal = new double[myObjects.Patterns.Count][];
        for (var i = 0; i < myObjects.Patterns.Count; ++i)
        {
            XORIdeal[i] = new double[myObjects.Patterns[i].Outputs.Length];
            for (var indx = 0; indx < myObjects.Patterns[i].Outputs.Length; ++indx)
            {
                XORIdeal[i][indx] = myObjects.Patterns[i].Outputs[indx];
            }
        }

        network = (BasicNetwork)Encog.Util.SerializeObject.Load("shodan.ser");
        IMLDataSet trainingSet = new BasicMLDataSet(XORInput, XORIdeal);

        // train the neural network
        //IMLTrain train = new ResilientPropagation(network, trainingSet);
        train = new ResilientPropagation(network, trainingSet);

        epoch = 1;

    }

    public void StartShodan()
    {
        UnityEngine.Debug.LogError("Start Shodan");
        LoadShodanPatterns();

        //var network = (BasicNetwork)Encog.Util.SerializeObject.Load("shodan.ser");
        //if (null != network)
        //{
        //    UnityEngine.Debug.LogError("Shodan is loaded");
        //    return;
        //}
        XORInput = new double[myObjects.Patterns.Count][];
        for (var i = 0; i < myObjects.Patterns.Count; ++i)
        {
            XORInput[i] = new double[myObjects.Patterns[i].Inputs.Length];
            for (var indx = 0; indx < myObjects.Patterns[i].Inputs.Length; ++indx)
            {
                XORInput[i][indx] = myObjects.Patterns[i].Inputs[indx];
            }
        }

        XORIdeal = new double[myObjects.Patterns.Count][];
        for (var i = 0; i < myObjects.Patterns.Count; ++i)
        {
            XORIdeal[i] = new double[myObjects.Patterns[i].Outputs.Length];
            for (var indx = 0; indx < myObjects.Patterns[i].Outputs.Length; ++indx)
            {
                XORIdeal[i][indx] = myObjects.Patterns[i].Outputs[indx];
            }
        }

        network = new BasicNetwork();
        network.AddLayer(new BasicLayer(null, true, myObjects.Patterns[0].Inputs.Length));

        network.AddLayer(new BasicLayer(new ActivationElliott(), true, 64));

        network.AddLayer(new BasicLayer(new ActivationElliott(), false, myObjects.Patterns[0].Outputs.Length));
        network.Structure.FinalizeStructure();
        network.Reset();

        IMLDataSet trainingSet = new BasicMLDataSet(XORInput, XORIdeal);

        // train the neural network
        //IMLTrain train = new ResilientPropagation(network, trainingSet);
        train = new ResilientPropagation(network, trainingSet);

        epoch = 1;


        //do
        //{
        //    train.Iteration();
        //    UnityEngine.Debug.LogError(@"Epoch #" + epoch + @" Error:" + train.Error);
        //    epoch++;
        //} while (train.Error > 0.0001);

        //Encog.Util.SerializeObject.Save("shodan.ser", network);
        //UnityEngine.Debug.LogError("Shodan is Saved");

        //network = (BasicNetwork)Encog.Util.SerializeObject.Load("encog.ser");

    }

    IMLTrain train;
    int epoch = 0;

    public void Update()
    {
        if (epoch == 0)
            return;

        //if (train.Error < 0.0001)
        //    return;

        train.Iteration();
        text.text = @"Epoch #" + epoch + @" Error:" + train.Error;
        epoch++;

        //if (train.Error < 0.0154)
        if (train.Error < 0.0154 && epoch > 1000)
        {
            Encog.Util.SerializeObject.Save("shodan.ser", network);
            UnityEngine.Debug.LogError("Shodan is Saved");
            epoch = 0;

        }
    }

    public void LoadShodanPatterns()
    {
        string PATH_SHODAN = Application.dataPath + "/Shodan.txt";
        string data = File.ReadAllText(PATH_SHODAN);
        myObjects = JsonUtility.FromJson<MyPatterns>(data);
    }
}
