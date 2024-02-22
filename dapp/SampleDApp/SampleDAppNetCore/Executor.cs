using Neo;
using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.Network.RPC.Models;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;
using System;
using Utility = Neo.Network.RPC.Utility;

namespace SampleDAppNetCore
{
    internal class Executor
    {
        internal static async Task ExecuteGetNeoAndGasBalance()
        {
            var client = new RpcClient(new Uri("http://localhost:50012"));
            var walletAPI = new WalletAPI(client);

            string address = "NPeVh32xJpZDMWmXwAUMgGSU5HHDQvDNq8";
            // Get the NEO balance
            uint neoBalance = await walletAPI.GetNeoBalanceAsync(address).ConfigureAwait(false);
            Console.WriteLine($"{neoBalance}");

            // Get the GAS balance
            decimal gasBalance = await walletAPI.GetGasBalanceAsync(address).ConfigureAwait(false);
            Console.WriteLine($"{gasBalance}");
        }

        internal static async Task ExecuteTransaction()
        {
            var client = new RpcClient(new Uri("http://localhost:50012"));
            var sendKey = Utility.GetKeyPair("L53tg72Az8QhYUAyyqTQ3LaXMXBE3S9mJGGZVKHBryZxya7prwhZ");
            var sender = Contract.CreateSignatureContract(sendKey.PublicKey).ScriptHash;
            var receiver = Utility.GetScriptHash("NirHUAteaMr6CqWuAAMaEUScPcS3FDKebM", ProtocolSettings.Default);

            WalletAPI walletAPI = new WalletAPI(client);
            Transaction tx = await walletAPI.TransferAsync(NativeContract.NEO.Hash, sendKey, receiver, 1024).ConfigureAwait(false);

            // print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(client);
            await neoAPI.WaitTransactionAsync(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));
        }

        internal static async Task InvokeContract()
        {
            // choose a neo node with rpc opened
            var client = new RpcClient(new Uri("http://localhost:50012"));
            ContractClient contractClient = new ContractClient(client);

            // get the contract hash
            UInt160 scriptHash = UInt160.Parse("0xff083def81444405d924f838a29c2bdc1c773a57");

            // test invoking the method provided by the contract 
            RpcInvokeResult invokeResult = await contractClient.TestInvokeAsync(scriptHash, "IssueAsset").ConfigureAwait(false);
            Console.WriteLine($"The name is {invokeResult.Stack.Single().GetString()}");
        }

        private static async Task TestToMultiTransfer()
        {
            // choose a neo node with rpc opened
            RpcClient client = new RpcClient(new Uri("http://localhost:50012"));
            // get the KeyPair of your account, which will pay the system and network fee
            KeyPair sendKey = Utility.GetKeyPair("L53tg72Az8QhYUAyyqTQ3LaXMXBE3S9mJGGZVKHBryZxya7prwhZ");
            UInt160 sender = Contract.CreateSignatureContract(sendKey.PublicKey).ScriptHash;

            // get the KeyPair of your accounts
            KeyPair key2 = Utility.GetKeyPair("L1bQBbZWnKbPkpHM3jXWD3E5NwK7nui2eWHYXVZPy3t8jSFF1Qj3");
            KeyPair key3 = Utility.GetKeyPair("KwrJfYyc7KWfZG5h97SYfcCQyW4jRw1njmHo48kZhZmuQWeTtUHM");

            // create multi-signatures contract, this contract needs at least 2 of 3 KeyPairs to sign
            Contract multiContract = Contract.CreateMultiSigContract(2, new List<ECPoint>() { sendKey.PublicKey, key2.PublicKey, key3.PublicKey });
            // get the scripthash of the multi-signature Contract
            UInt160 multiAccount = multiContract.Script.ToScriptHash();

            // construct the script, in this example, we will transfer 1024 GAS to multi-sign account
            // in contract parameter, the amount type is BigInteger, so we need to muliply the contract factor
            UInt160 scriptHash = NativeContract.GAS.Hash;
            byte[] script = scriptHash.MakeScript("transfer", sender, multiAccount, 1024 * NativeContract.GAS.Factor);

            // add Signers, which is a collection of scripthashs that need to be signed
            Signer[] cosigners = new[] { new Signer { Scopes = WitnessScope.CalledByEntry, Account = sender } };

            // initialize the TransactionManager with rpc client and magic
            // fill the script and cosigners
            TransactionManager txManager = await new TransactionManagerFactory(client)
                .MakeTransactionAsync(script, cosigners).ConfigureAwait(false);
            // add signature and sign transaction with the added signature
            Transaction tx = await txManager.AddSignature(sendKey).SignAsync().ConfigureAwait(false);

            // broadcasts the transaction over the Neo network.
            await client.SendRawTransactionAsync(tx).ConfigureAwait(false);
            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            // print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(client);
            await neoAPI.WaitTransactionAsync(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));
        }
    }
}
