using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

using Hyperledger.Indy.WalletApi;
using Hyperledger.Indy.DidApi;
using Hyperledger.Indy.PoolApi;
using Hyperledger.Indy.LedgerApi;
using Hyperledger.Indy.CryptoApi;
using Hyperledger.Indy.AnonCredsApi;

using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using UnityEngine.UIElements;

public class DidSystem : MonoBehaviour
{
    public static DidSystem Instance;

    Dictionary<string, DidUser> didUserDictionary;

    string defultWalletPath;
    string walletConfig;
    string walletName;
    string walletCredentials;

    string poolName;
    string poolConfig;

    Wallet wallet;
    Pool pool;

    string genesisFilePath = "C:/Users/82105/Documents/GitHub/Decentralized-MQTT/pool_transactions_genesis.txt";

    CreateAndStoreMyDidResult result;

    
    // Start is called before the first frame update

    void Awake()
    {

    }

    void Start()
    {
        genesisFilePath = Application.dataPath + "/genesis.txn";
        //defultWalletPath = Application.dataPath + "/wallet";
        //Debug.Log("defultWalletPath: " + defultWalletPath);

    }

    public void StartDidSyetem(DidSystem didSystem, HttpClient httpClient, string userDid, string targetDid)
    {
        string genesisFile_ = httpClient.CreateGenesisFile(genesisFilePath);

        Debug.Log("Pool Start");
        
        
        string resolveData = DidResolver.resolve(didSystem, userDid, targetDid);
        JObject resolveDataJson = JObject.Parse(resolveData);

        JArray serviceArray = resolveDataJson.GetValue("service").Value<JArray>();
        Debug.Log("serviceArray: " + serviceArray.ToString());

        string serviceEndpoint = serviceArray[0]["serviceEndpoint"].ToString();
        Debug.Log("serviceEndpoint: " + serviceEndpoint);
        
        ConnectionPool();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Clean Pool Start");
            CleanPool();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            DidDocument didDocument = new DidDocument();
        }
        */
    }

    public static DidSystem GetInstance()
    {
        if (Instance == null)
        {
            Instance = FindObjectOfType<DidSystem>();
            if (Instance == null)
            {
                GameObject container = new("DidSystem");
                Instance = container.AddComponent<DidSystem>();
            }
        }
        return Instance;
    }



    public void SearchWallet()
    {
        Debug.Log("Search Wallet");
        string[] wallet_list = Directory.GetDirectories(defultWalletPath);
        foreach (string wallet in wallet_list)
        {
            Debug.Log(wallet);
        }
    }

    public void CreateWallet()
    {
        Debug.Log("Indy Create Wallet");
        walletName = "test_wallet" + UnityEngine.Random.Range(0, 1000).ToString();
        //string walletConfig = "{\"id\":\"" + walletName + "\"}";
        walletConfig = "{\"id\":\"" + walletName + "\", \"storageConfig\": {\"path\": \""
            + defultWalletPath + "\"}}";
        walletCredentials = "{\"key\":\"wallet_key\"}";

        Debug.Log("walletConfig :" + walletConfig);

        Wallet.CreateWalletAsync(walletConfig, walletCredentials).Wait();
    }

    public Wallet CreateWalletTest()
    {
        /*
        string walletConfig = "{\"id\":\"" + walletName + "\", \"storageConfig\": {\"path\": \"" 
            + defultWalletPath + "\"}}";
        */
        //string walletConfig = "{\"id\":\"" + walletName + "\"}";
        //string walletCredentials = "{\"key\":\"" + wallet_key + "\"}";
        Debug.Log("Indy Create Wallet");
        walletName = "test_wallet" + UnityEngine.Random.Range(0, 1000).ToString();
        walletConfig = "{\"id\":\"" + walletName + "\", \"storageConfig\": {\"path\": \""
            + defultWalletPath + "\"}}";
        walletCredentials = "{\"key\":\"wallet_key\"}";

        Wallet.CreateWalletAsync(walletConfig, walletCredentials).Wait();

        return Wallet.OpenWalletAsync(walletConfig, walletCredentials).Result;
    }

    public void OpenWallet()
    {
        try
        {
            wallet = Wallet.OpenWalletAsync(walletConfig, walletCredentials).Result;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public void CleanWallet()
    {
        try
        {
            Debug.Log("Indy Close Wallet");
            wallet.CloseAsync().Wait();
            Wallet.DeleteWalletAsync(walletConfig, walletCredentials).Wait();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public void CreateDidInWallet()
    {
        //string did_json = "{\"seed\":\"" + didIssuerSeed + "\"}";

        try
        {
            //result = Did.CreateAndStoreMyDidAsync(wallet, did_json).Result;
            Debug.Log("DID: " + result.Did);
            Debug.Log("Verkey: " + result.VerKey);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public CreateAndStoreMyDidResult CreateDidInWalletTest(Wallet wallet, string didSeed)
    {
        string did_json = "{\"seed\":\"" + didSeed + "\"}";

        return Did.CreateAndStoreMyDidAsync(wallet, did_json).Result;
    }

    public void ConnectionPool()
    {
        if (false == File.Exists(genesisFilePath))
        {
            Debug.Log("Genesis File is Null");
            return;
        }

        poolName = "pool" + UnityEngine.Random.Range(0, 1000).ToString();
        poolConfig = "{\"genesis_txn\":\"" + genesisFilePath + "\"}";
        Debug.Log("Pool Config: " + poolConfig);

        Debug.Log("Indy Create Pool Ledger Config");
        Pool.CreatePoolLedgerConfigAsync(poolName, poolConfig).Wait();

        Debug.Log("Indy Open Pool Ledger");
        pool = Pool.OpenPoolLedgerAsync(poolName, poolConfig).Result;
    }

    public void CleanPool()
    {
        try
        {
            Debug.Log("Indy Close Pool Ledger");
            pool.CloseAsync().Wait();
            Pool.DeletePoolLedgerConfigAsync(poolName).Wait();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public string GetNymTransaction(string userDid, string target_did)
    {
        string submitter_did = userDid;

        string nym_request = Ledger.BuildGetNymRequestAsync(submitter_did, target_did).Result;
        Debug.Log("nym_request: " + nym_request);

        string nym_response = Ledger.SubmitRequestAsync(pool, nym_request).Result;
        Debug.Log("nym_response: " + nym_response);

        return nym_response;
    }

    public string GetAttribTransaction(string userDid, string target_did, string attrib)
    {
        string submitter_did = userDid;

        string attrib_request = Ledger.BuildGetAttribRequestAsync(submitter_did, target_did, attrib, null, null).Result;
        Debug.Log("attrib_request: " + attrib_request);

        string attrib_response = Ledger.SubmitRequestAsync(pool, attrib_request).Result;
        Debug.Log("attrib_response: " + attrib_response);

        return attrib_response;
    }


}