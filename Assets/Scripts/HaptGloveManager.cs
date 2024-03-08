using System;
using System.Collections;
using System.Collections.Generic;
using HaptGlove;
using TMPro;
using UnityEditor;
using UnityEngine;

public class HaptGloveManager : MonoBehaviour
{
    public bool isQuest;
    
    public HaptGloveHandler leftHand, rightHand;
    public GameObject btIndicator_L, btIndicator_R, pumpIndicator_L, pumpIndicator_R;

    private string bluetoothLog;

    [SerializeField] private TMP_Text log;

    void Start()
    {
        if (isQuest)
        {
            leftHand.isQuest = true;
            rightHand.isQuest = true;
        }
        else
        {
            leftHand.isQuest = false;
            rightHand.isQuest = false;
        }

        leftHand.onBluetoothConnected += HaptGlove_OnConnected;
        leftHand.onBluetoothConnectionFailed += HaptGlove_OnConnectedFailed;
        leftHand.onBluetoothDisconnected += HaptGlove_OnDisconnected;
        leftHand.onPumpAction += HaptGlove_OnPumpAction;

        rightHand.onBluetoothConnected += HaptGlove_OnConnected;
        rightHand.onBluetoothConnectionFailed += HaptGlove_OnConnectedFailed;
        rightHand.onBluetoothDisconnected += HaptGlove_OnDisconnected;
        rightHand.onPumpAction += HaptGlove_OnPumpAction;

        //Add all layers that you want to interact with HaptGlove
        AddHaptGloveInteractableLayer("HaptGloveInteractable");
    }


    void Update()
    {
        if (log != null)
        {
            log.text = rightHand.GetComponent<HaptGloveHandler>().btText;
        }
    }

    private void HaptGlove_OnConnected(HaptGloveHandler.HandType hand)
    {
        if (hand == HaptGloveHandler.HandType.Left)
        {
            btIndicator_L.SetActive(true);
            bluetoothLog = "Left glove connected: " + "HaptGLove " + hand.ToString();
        }
        else if (hand == HaptGloveHandler.HandType.Right)
        {
            btIndicator_R.SetActive(true);
            bluetoothLog = "Right glove connected: " + "HaptGLove " + hand.ToString();
        }
        
    }


    private void HaptGlove_OnConnectedFailed(HaptGloveHandler.HandType hand)
    {
        if (hand == HaptGloveHandler.HandType.Left)
        {
            btIndicator_L.SetActive(false);
            bluetoothLog = "Left glove connection failed: " + "HaptGlove " + hand.ToString();
        }
        else if(hand == HaptGloveHandler.HandType.Right)
        {
            btIndicator_R.SetActive(false);
            bluetoothLog = "Right glove connection failed: " + "HaptGlove " + hand.ToString();
        }
        
    }

    private void HaptGlove_OnDisconnected(HaptGloveHandler.HandType hand)
    {
        if (hand == HaptGloveHandler.HandType.Left)
        {
            btIndicator_L.SetActive(false);
            bluetoothLog = "Left glove disconnected: " + "HaptGlove " + hand.ToString();
        }
        else if (hand == HaptGloveHandler.HandType.Right)
        {
            btIndicator_R.SetActive(false);
            bluetoothLog = "Right glove disconnected: " + "HaptGlove " + hand.ToString();
        }
    }

    private void HaptGlove_OnPumpAction(HaptGloveHandler.HandType hand, bool state)
    {
        if (hand == HaptGloveHandler.HandType.Left)
        {
            if (state)
                pumpIndicator_L.SetActive(true);
            else
                pumpIndicator_L.SetActive(false);
        }
        else if (hand == HaptGloveHandler.HandType.Right)
        {
            if (state)
                pumpIndicator_R.SetActive(true);
            else
                pumpIndicator_R.SetActive(false);
        }
    }

    public void AddHaptGloveInteractableLayer(string layerName)
    {
        leftHand.hapticsInteratableLayers.Add(LayerMask.NameToLayer(layerName));
        rightHand.hapticsInteratableLayers.Add(LayerMask.NameToLayer(layerName));
    }

    public string[] GetHaptGloveInteractableLayer()
    {
        int[] layers = rightHand.hapticsInteratableLayers.ToArray();
        string[] layerNames = new string[layers.Length];

        for (int i = 0; i < layers.Length; i++)
        {
            layerNames[i] = LayerMask.LayerToName(layers[i]);
        }

        return layerNames;
    }
}
