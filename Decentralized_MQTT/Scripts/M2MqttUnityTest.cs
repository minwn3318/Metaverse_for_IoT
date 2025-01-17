﻿/*
The MIT License (MIT)

Copyright (c) 2018 Giovanni Paolo Vigano'

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using Unity.VisualScripting;
using Hyperledger.Indy.WalletApi;
using Hyperledger.Indy.DidApi;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO;
using Org.BouncyCastle.Bcpg.OpenPgp;

/// <summary>
/// Examples for the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),
/// </summary>
namespace M2MqttUnity.Examples
{
    /// <summary>
    /// Script for testing M2MQTT with a Unity UI
    /// </summary>
    public class M2MqttUnityTest : M2MqttUnityClient
    {
        private List<string> eventMessages = new List<string>();
        private bool updateUI = false;
        

        [Tooltip("Set this to true to perform a testing cycle automatically on startup")]
        public bool autoTest = false;
        [Header("User Interface")]
        public InputField consoleInputField;
        public Toggle encryptedToggle;
        public InputField addressInputField;
        public InputField portInputField;
        public InputField messageInputField;
        public Button connectButton;
        public Button disconnectButton;
        public Button testPublishButton;
        public Button clearButton;
        public Button sendButton;

        [Header("MQTT Settings")]
        [Tooltip("Topic to subscribe to")]
        public List<string> topic;
        public string targetDid;

        private IndyTest indyTest;

        protected override void Start()
        {
            SetUiMessage("Ready.");
            updateUI = true;
            base.Start();

            indyTest = GetComponent<IndyTest>();
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            SetUiMessage("Connected to broker on " + brokerAddress + "\n");

            indyTest.StartIndy();

            if (autoTest)
            {
                TestPublish();
            }
        }

        public async void SendMessage2()
        {
            if (messageInputField)
            {
                string message = messageInputField.text;

                //sing message
                string signedMessage = await indyTest.SignDataAsync(message);

                // is signed message
                if(indyTest.VerifySignature(signedMessage, message))
                {
                    Debug.Log("Verify Signature Success");
                }
                else
                {
                    Debug.Log("Verify Signature Fail");
                }

                if (message != "")
                {
                    foreach (string i in topic)
                    {
                        client.Publish(i, Encoding.UTF8.GetBytes(signedMessage), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

                        
                        AddUiMessage("Message published.");
                    }
                }
            }
        }

        public override void Disconnect()
        {
            indyTest.StopIndy();
            base.Disconnect();
        }



        public void TestPublish()
        {
            foreach (string i in topic)
            {
                client.Publish(i, System.Text.Encoding.UTF8.GetBytes("Test message"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                Debug.Log("Test message published");
                AddUiMessage("Test message published.");
            }
        }

        public void SetBrokerAddress(string brokerAddress)
        {
            if (addressInputField && !updateUI)
            {
                this.brokerAddress = brokerAddress;
            }
        }

        public void SetBrokerPort(string brokerPort)
        {
            if (portInputField && !updateUI)
            {
                int.TryParse(brokerPort, out this.brokerPort);
            }
        }


        

        public void SetEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
            Debug.Log("isEncrypted: " + isEncrypted);
        }


        public void SetUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text = msg;
                updateUI = true;
            }
        }

        public void AddUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text += msg + "\n";
                updateUI = true;
            }
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            SetUiMessage("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
        }

        protected override void SubscribeTopics()
        {
            foreach (string i in topic)
            {
                client.Subscribe(new string[] { i }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            //client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        protected override void UnsubscribeTopics()
        {
            foreach (string i in topic)
            {
                client.Unsubscribe(new string[] { i });
            }
            //client.Unsubscribe(new string[] { topic });
        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            AddUiMessage("CONNECTION FAILED! " + errorMessage);
        }

        protected override void OnDisconnected()
        {
            AddUiMessage("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            AddUiMessage("CONNECTION LOST!");
        }

        private void UpdateUI()
        {
            if (client == null)
            {
                if (connectButton != null)
                {
                    connectButton.interactable = true;
                    disconnectButton.interactable = false;
                    testPublishButton.interactable = false;
                }
            }
            else
            {
                if (testPublishButton != null)
                {
                    testPublishButton.interactable = client.IsConnected;
                }
                if (disconnectButton != null)
                {
                    disconnectButton.interactable = client.IsConnected;
                }
                if (connectButton != null)
                {
                    connectButton.interactable = !client.IsConnected;
                }
            }
            if (addressInputField != null && connectButton != null)
            {
                addressInputField.interactable = connectButton.interactable;
                addressInputField.text = brokerAddress;
            }
            if (portInputField != null && connectButton != null)
            {
                portInputField.interactable = connectButton.interactable;
                portInputField.text = brokerPort.ToString();
            }
            if (encryptedToggle != null && connectButton != null)
            {
                encryptedToggle.interactable = connectButton.interactable;
                encryptedToggle.isOn = isEncrypted;
            }
            if (clearButton != null && connectButton != null)
            {
                clearButton.interactable = connectButton.interactable;
            }
            updateUI = false;
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            
            string msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("Received: " + msg);
            StoreMessage(msg);

            foreach (string i in this.topic)
            {
                if (topic == i)
                {
                    if (autoTest)
                    {
                        autoTest = false;
                        Disconnect();
                    }
                }
            }
             
            /*
            string encryptedMsg = Encoding.UTF8.GetString(message);
            Debug.Log("Received encrypted message: " + encryptedMsg);

            byte[] decryptedMsgBytes = DecryptString(encryptedMsg, privateKey);
            string decryptedMsg = Encoding.UTF8.GetString(decryptedMsgBytes);

            Debug.Log("Decrypted message: " + decryptedMsg);

            base.DecodeMessage(topic, Encoding.UTF8.GetBytes(decryptedMsg));
            */
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        private void ProcessMessage(string msg)
        {
            AddUiMessage("Received: " + msg);
        }

        protected override void Update()
        {
            base.Update(); // call ProcessMqttEvents()

            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
            if (updateUI)
            {
                UpdateUI();
            }
        }



        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnValidate()
        {
            if (autoTest)
            {
                autoConnect = true;
            }
        }

        public string EncryptString(string plainText, string publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    rsa.FromXmlString(publicKey.ToString());
                    var encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), true);
                    var base64Encrypted = Convert.ToBase64String(encryptedData);
                    return base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false; // Set this to false to avoid permission issues.
                }
            }


        }

        public void PublishEncryptedMessage(string topic, string message)
        {
            // Replace 'publicKey' with the actual public key of the receiver.
            
            /*
             * client.Publish(topic,
                           Encoding.UTF8.GetBytes(EncryptString(message, PublicKey)),
                           MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE,
                           false);
            */
        }

        public byte[] DecryptString(string cipherText, string pemPrivateKey)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    string xmlPrivateKey = ConvertPemToXml(pemPrivateKey);
                    rsa.FromXmlString(xmlPrivateKey);

                    var base64Encrypted = Convert.FromBase64String(cipherText);
                    var decryptedData = rsa.Decrypt(base64Encrypted, false); // Use 'false' for PKCS#1 v1.5 padding.

                    return decryptedData;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public void GenerateKeyPair(out string publicKey, out string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false; // Don't store the keys in a key container
                publicKey = rsa.ToXmlString(false); // False to get the public key
                privateKey = rsa.ToXmlString(true); // True to get the private key
            }
        }

        private string ConvertPemToXml(string pemPrivateKey)
        {
            using (var textReader = new StringReader(pemPrivateKey))
            {
                var pemReader = new PemReader(textReader);
                var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);

                var rsaProvider = new RSACryptoServiceProvider();
                rsaProvider.ImportParameters(rsaParams);

                return rsaProvider.ToXmlString(true);
            }
        }
    }
}

